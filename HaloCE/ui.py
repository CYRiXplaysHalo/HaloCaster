import queue
import orjson
import dearpygui.dearpygui as dpg

# Global flags for window visibility
info_window_enabled = True
positions_window_enabled = True
performance_window_enabled = False
editor_window_enabled = False

# Global flags for plot series visibility
scatter_series_enabled = True
item_series_enabled = False
object_series_enabled = False


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

                # Scatter series for team 0 and team 1 (without assigning colors)
                if scatter_series_enabled:
                    dpg.add_scatter_series([], [], parent="y_axis", tag="team_1_series")  # For team 0
                    dpg.add_scatter_series([], [], parent="y_axis", tag="team_0_series")  # For team 1

                
                dpg.bind_item_theme("team_1_series", "plot_theme")
                dpg.bind_item_theme("team_0_series", "plot_theme")
    # Keep track of the annotation items for player indices
    player_annotations = []

    perf_x, perf_y, perf_y_2, perf_y_3, memory_mbytes_count_y = [], [], [], [], []

    if performance_window_enabled:
        with dpg.window(label='performance', pos=(900, 550), tag="performance"):
            with dpg.plot(label="performance", height=400, width=600):
                dpg.add_plot_legend()
                dpg.add_plot_axis(dpg.mvXAxis, label="x", tag="perf_x_axis")
                dpg.add_plot_axis(dpg.mvYAxis, label="y", tag="perf_y_axis")
                dpg.add_plot_axis(dpg.mvYAxis, label="y", tag="counts_y_axis")
                dpg.add_line_series(perf_x, perf_y, label="game_info_ms", parent="perf_y_axis", tag="series_tag")
                dpg.add_line_series(perf_x, perf_y_2, label="loop_ms", parent="perf_y_axis", tag="series_tag_2")
                dpg.add_line_series(perf_x, perf_y_3, label="post_steps_ms", parent="perf_y_axis", tag="series_tag_3")
                dpg.add_line_series(perf_x, memory_mbytes_count_y, label="memory_mbytes", parent="counts_y_axis", tag="series_tag_4")

    if editor_window_enabled:
        with dpg.window(label='editor', pos=(900, 550), tag="editor"):
            dpg.add_button(tag='send_solobox_startgame', label='Allow solo box start', callback=handle_solobox_clicked, user_data=write_queue_from_ui)
            dpg.add_input_text(tag='write_address', label='Address')
            dpg.add_input_text(tag='write_value', label='Value')
            dpg.add_input_text(tag='write_length', label='Length')
            dpg.add_button(tag='write_button', label='Write', callback=handle_write_clicked, user_data=write_queue_from_ui)

    dpg.setup_dearpygui()
    dpg.show_viewport()

    while dpg.is_dearpygui_running():
        try:
            game_info = game_info_queue_for_ui.get(block=False)
            player_info_string = orjson.dumps(game_info, option=orjson.OPT_INDENT_2 | orjson.OPT_NON_STR_KEYS).decode()

            if filter_string := dpg.get_value('filter'):
                player_info_string = '\n'.join([line for line in player_info_string.splitlines() if filter_string in line])
            dpg.set_value('player_info', value=player_info_string)

            # Separate positions for team 0 and team 1, along with player indices
            x_positions_team_0, y_positions_team_0 = [], []
            x_positions_team_1, y_positions_team_1 = [], []
            annotations = []

            if scatter_series_enabled and 'players' in game_info:
                for player in game_info['players']:
                    if player['player_object_data']:
                        player_index = player['player_index']  # Correct index reference

                        if player['team'] == 0:
                            x_positions_team_0.append(player['player_object_data']['x'])
                            y_positions_team_0.append(player['player_object_data']['y'])
                            annotations.append((player['player_object_data']['x'], player['player_object_data']['y'], str(player_index)))

                        elif player['team'] == 1:
                            x_positions_team_1.append(player['player_object_data']['x'])
                            y_positions_team_1.append(player['player_object_data']['y'])
                            annotations.append((player['player_object_data']['x'], player['player_object_data']['y'], str(player_index)))

                # Update the series for team 0 and team 1
                dpg.set_value('team_0_series', [x_positions_team_0, y_positions_team_0])
                dpg.set_value('team_1_series', [x_positions_team_1, y_positions_team_1])

                # Clear previous annotations
                for annotation in player_annotations:
                    dpg.delete_item(annotation)

                # Add annotations for each player index
                player_annotations = []


        except queue.Empty:
            pass

        dpg.render_dearpygui_frame()

    dpg.destroy_context()


if __name__ == '__main__':
    start_ui(queue.Queue(), queue.Queue())
