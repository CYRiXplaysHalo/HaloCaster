import tkinter as tk
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import queue
import threading
import time


class ScatterPlotApp:
    def __init__(self, root, game_info_queue_for_ui):
        self.root = root
        self.root.title("Matplotlib Scatter Plot with Real-time Data")

        # Create a Matplotlib figure and axis
        self.fig, self.ax = plt.subplots()

        # Store the queue that receives game data
        self.game_info_queue_for_ui = game_info_queue_for_ui

        # Add a Matplotlib canvas to the Tkinter window
        self.canvas = FigureCanvasTkAgg(self.fig, master=self.root)
        self.canvas.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)

        # Start the plot update loop
        self.update_plot()

    def update_plot(self):
        if not self.game_info_queue_for_ui.empty():
            game_info = self.game_info_queue_for_ui.get()

            # Clear the plot for new data
            self.ax.clear()

            # Set the axis ranges to -20 to 20 for both X and Y axes
            self.ax.set_xlim(-20, 20)
            self.ax.set_ylim(-20, 20)

            # Separate data for teams
            x_team_0, y_team_0, labels_team_0 = [], [], []
            x_team_1, y_team_1, labels_team_1 = [], [], []

            for player in game_info['players']:
                player_index = player['player_index']
                x = player['player_object_data']['x']
                y = player['player_object_data']['y']

                if player['team'] == 0:
                    x_team_0.append(x)
                    y_team_0.append(y)
                    labels_team_0.append(f"Player {player_index}")
                else:
                    x_team_1.append(x)
                    y_team_1.append(y)
                    labels_team_1.append(f"Player {player_index}")

            # Scatter plot for Team 0 (Red)
            self.ax.scatter(x_team_0, y_team_0, color='red', label='Team 0')

            # Scatter plot for Team 1 (Blue)
            self.ax.scatter(x_team_1, y_team_1, color='blue', label='Team 1')

            # Annotate points for Team 0
            for i, label in enumerate(labels_team_0):
                self.ax.annotate(label, (x_team_0[i], y_team_0[i]), textcoords="offset points", xytext=(0, 10), ha='center')

            # Annotate points for Team 1
            for i, label in enumerate(labels_team_1):
                self.ax.annotate(label, (x_team_1[i], y_team_1[i]), textcoords="offset points", xytext=(0, 10), ha='center')

            # Set axis labels and title
            self.ax.set_xlabel("X Axis")
            self.ax.set_ylabel("Y Axis")
            self.ax.set_title("Scatter Plot of Players with Annotations")
            self.ax.legend()

            # Redraw the canvas
            self.canvas.draw()

        # Reduce delay to 10 ms for near real-time updates
        self.root.after(10, self.update_plot)


def start_ui(game_info_queue_for_ui, write_queue_from_ui):
    # Create the Tkinter window
    root = tk.Tk()

    # Create the scatter plot app, passing in the game data queue
    app = ScatterPlotApp(root, game_info_queue_for_ui)

    # Run the Tkinter main loop
    root.mainloop()


# Function to simulate data ingestion in a separate thread
def ingest_game_data(game_info_queue_for_ui):
    # Simulate data being ingested over time
    data_stream = [
        {'players': [
            {'player_index': 1, 'team': 0, 'player_object_data': {'x': 1, 'y': 2}},
            {'player_index': 2, 'team': 1, 'player_object_data': {'x': 2, 'y': 4}},
        ]},
        {'players': [
            {'player_index': 1, 'team': 0, 'player_object_data': {'x': 1.5, 'y': 2.5}},
            {'player_index': 2, 'team': 1, 'player_object_data': {'x': 2.5, 'y': 4.5}},
            {'player_index': 3, 'team': 0, 'player_object_data': {'x': 3, 'y': 5}},
        ]}
    ]

    for data in data_stream:
        game_info_queue_for_ui.put(data)
        time.sleep(0.5)  # Simulate faster data ingestion (0.5 seconds delay)

if __name__ == '__main__':
    # Create a queue to hold game data
    game_info_queue = queue.Queue()

    # Start the game data ingestion in a separate thread
    data_thread = threading.Thread(target=ingest_game_data, args=(game_info_queue,), daemon=True)
    data_thread.start()

    # Start the UI with the game data queue
    start_ui(game_info_queue, None)
