import queue
import orjson
import dearpygui.dearpygui as dpg
import asyncio
import websockets
import threading
import re

# Global flags for window visibility
info_window_enabled = True
positions_window_enabled = True
performance_window_enabled = False
editor_window_enabled = False

# Global flags for plot series visibility
scatter_series_enabled = True
item_series_enabled = False
object_series_enabled = False

# WebSocket server settings
WEBSOCKET_HOST = "localhost"
WEBSOCKET_PORT = 8765

class Diff:
    """
    Represents a memory difference in the guest system, with address, value, and length.
    Address, value, and length can be given in hex (with prefix 0x) or in decimal.
    """

    def __init__(self, address, value, length):
        self.address = address
        self.value = value
        self.length = length

    @classmethod
    def from_diff_string(cls, s):
        """
        Constructs a Diff object from a single line of an IDA diff string.
        Converts file offsets to memory offsets.
        """
        address, _, value = s.strip().split()
        address = hex(int(address.removesuffix(':'), 16) + 0x10000)
        value = f'0x{value}'
        return cls(address, value, '1')

    def as_dict(self):
        return {'address': self.address, 'value': self.value, 'length': self.length}

    def __repr__(self):
        return f'<Diff: address:{self.address} value:{self.value} length:{self.length}>'


def handle_write_clicked(sender, app_data, user_data):
    write_queue_from_ui = user_data
    write_queue_from_ui.put({
        'address': dpg.get_value('write_address'),
        'value': dpg.get_value('write_value'),
        'length': dpg.get_value('write_length')
    })

    # Reset inputs
    dpg.set_value('write_address', '')
    dpg.set_value('write_value', '')
    dpg.set_value('write_length', '')


def send_preset(diffs, write_queue):
    print('Sending diffs through queue')
    for diff in diffs:
        write_queue.put(diff.as_dict())


def handle_solobox_clicked(sender, app_data, user_data):
    """
    Changes memory in xemu to allow solo box start and ignore team checks.
    """
    diff_string = '''
        # always_allow_start_game.dif
        0008C514: 32 B0
        0008C515: C0 01
        # startgame_ignore-teamcheck_ignore-endgameteams.dif
        0008C0D2: 01 00
        000F7DEA: 0F 90
        000F7DEB: 84 90
        000F7DEC: 92 90
        000F7DED: 01 90
        000F7DEE: 00 90
        000F7DEF: 00 90
    '''

    diffs = [Diff.from_diff_string(s) for s in diff_string.splitlines() if s and ':' in s and not s.strip().startswith('#')]
    send_preset(diffs, user_data)


def format_map_name(map_name):
    if not map_name:
        return "Unknown Map"
    parts = map_name.split('\\')
    for i in range(len(parts) - 2):
        if parts[i].lower() == 'levels' and parts[i+1].lower() == 'test':
            name_part = parts[i+2]
            formatted = ''.join([word.capitalize() for word in re.sub(r'[^a-zA-Z0-9]', ' ', name_part).split()])
            return formatted
    last_part = parts[-1]
    formatted = re.sub(r'[^a-zA-Z0-9]', ' ', last_part)
    words = formatted.split()
    if not words:
        return "Unknown Map"
    camel_case = words[0].lower() + ''.join(word.capitalize() for word in words[1:])
    return camel_case


async def websocket_server(websocket, path, game_info_queue_for_ui):
    series_score = {"red": 0, "blue": 0}
    previous_players_signature = None

    def get_player_signature(players):
        sorted_players = sorted(players, key=lambda x: x['name'])
        signature_parts = [f"{p['name']}:{p['team']}" for p in sorted_players]
        return ','.join(signature_parts)

    while True:
        try:
            game_info = game_info_queue_for_ui.get(block=False)
            current_players = game_info.get("players", [])
            current_signature = get_player_signature(current_players)
            events = game_info.get("events", [])
            
            # Process game events
            game_ended = any("game ended" in e.lower() for e in events)
            game_started = any("game started" in e.lower() for e in events)

            if game_ended:
                red_kills = sum(p['kills'] for p in current_players if p['team'] == 0)
                blue_kills = sum(p['kills'] for p in current_players if p['team'] == 1)
                if red_kills > blue_kills:
                    series_score["red"] += 1
                elif blue_kills > red_kills:
                    series_score["blue"] += 1

            if game_started:
                if previous_players_signature and current_signature != previous_players_signature:
                    series_score.update({"red": 0, "blue": 0})
                previous_players_signature = current_signature

            # Prepare data to send
            data = {
                "map_name": format_map_name(game_info.get("multiplayer_map_name")),
                "game_type": game_info.get("game_type", "Unknown Game Type"),
                "variant": game_info.get("variant", "Unknown Variant"),
                "real_time_elapsed": game_info.get("game_time_info", {}).get("real_time_elapsed", 0),
                "events": events,
                "players": current_players,
                "red_team_kills": sum(p['kills'] for p in current_players if p['team'] == 0),
                "blue_team_kills": sum(p['kills'] for p in current_players if p['team'] == 1),
                "series_score": series_score
            }
            await websocket.send(orjson.dumps(data).decode())
        except queue.Empty:
            await asyncio.sleep(0.1)


def start_websocket_server(game_info_queue_for_ui):
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    start_server = websockets.serve(lambda ws, path: websocket_server(ws, path, game_info_queue_for_ui), WEBSOCKET_HOST, WEBSOCKET_PORT)
    loop.run_until_complete(start_server)
    loop.run_forever()


def start_ui(game_info_queue_for_ui, write_queue_from_ui):
    dpg.create_context()
    dpg.create_viewport(title='Xemu Memory Watcher', width=1680, height=1050)

    # Setup windows with visibility flags
    if info_window_enabled:
        with dpg.window(label="info", tag="info"):
            dpg.add_input_text(tag="filter", label="Filter")
            dpg.add_input_text(tag='player_info', width=800, height=900, multiline=True, readonly=True)

    if positions_window_enabled:
        with dpg.window(label="positions", pos=(900, 0), tag="positions"):
            with dpg.theme(tag="plot_theme"):
                with dpg.theme_component(dpg.mvScatterSeries):
                    dpg.add_theme_style(dpg.mvPlotStyleVar_Marker, dpg.mvPlotMarker_Circle, category=dpg.mvThemeCat_Plots)
                    dpg.add_theme_style(dpg.mvPlotStyleVar_MarkerSize, 20, category=dpg.mvThemeCat_Plots)
                    
            with dpg.plot(label='positions', width=600, height=600):
                dpg.add_plot_axis(dpg.mvXAxis, label="x", tag="x_axis", no_gridlines=True, no_tick_marks=True)
                dpg.set_axis_limits(dpg.last_item(), -20, 20)
                dpg.add_plot_axis(dpg.mvYAxis, label="y", tag="y_axis", no_gridlines=True, no_tick_marks=True)
                dpg.set_axis_limits(dpg.last_item(), -20, 20)

                if scatter_series_enabled:
                    dpg.add_scatter_series([], [], parent="y_axis", tag="team_1_series")
                    dpg.add_scatter_series([], [], parent="y_axis", tag="team_0_series")

                dpg.bind_item_theme("team_1_series", "plot_theme")
                dpg.bind_item_theme("team_0_series", "plot_theme")

    # Keep other window definitions the same...

    dpg.setup_dearpygui()
    dpg.show_viewport()

    while dpg.is_dearpygui_running():
        try:
            game_info = game_info_queue_for_ui.get(block=False)
            player_info_string = orjson.dumps(game_info, option=orjson.OPT_INDENT_2 | orjson.OPT_NON_STR_KEYS).decode()

            if filter_string := dpg.get_value('filter'):
                player_info_string = '\n'.join([line for line in player_info_string.splitlines() if filter_string in line])
            dpg.set_value('player_info', value=player_info_string)

            # Update positions and other UI elements...

        except queue.Empty:
            pass

        dpg.render_dearpygui_frame()

    dpg.destroy_context()


if __name__ == '__main__':
    game_info_queue = queue.Queue()
    write_queue = queue.Queue()

    websocket_thread = threading.Thread(target=start_websocket_server, args=(game_info_queue,))
    websocket_thread.daemon = True
    websocket_thread.start()

    start_ui(game_info_queue, write_queue)