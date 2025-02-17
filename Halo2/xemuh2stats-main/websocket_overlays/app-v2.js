let client = new websocket_client();
let update_handle = null;
let current_life_cycle = "none";

// Store the previous deaths count for each player
let previous_deaths = {};

// Duration to highlight the row (in seconds)
let highlight_duration = 8; // You can change this value later

function players_sort_team(a, b) {
    if (a.player.team_index < b.player.team_index) {
        return -1;
    }
    if (a.player.team_index > b.player.team_index) {
        return 1;
    }
    return 0;
}

client.add_message_recieved_callback('life_cycle', (life_cycle_state) => {
    current_life_cycle = life_cycle_state.life_cycle;
});

client.add_message_recieved_callback('get_players', (players_data) => {
    let player_names = Object.keys(players_data);

    let red_players = {};
    let blue_players = {};

    player_names.forEach(player_name => {
        const player = players_data[player_name];
        if (player.player.team_index == 0)
            red_players[player_name] = player;
        if (player.player.team_index == 1)
            blue_players[player_name] = player;
    });

    let red_table_rows = document.querySelectorAll('#red-team-table tbody tr');
    let blue_table_rows = document.querySelectorAll("#blue-team-table tbody tr");

    for (let i = 0; i < 4; i++) {
        if (Object.keys(red_players).length > i) {
            let red_row = red_table_rows[i];
            let red_cells = red_row.querySelectorAll('td');
            let player_name = Object.keys(red_players)[i];
            let player = red_players[player_name];

            // Check if the deaths increased by 1
            if (previous_deaths[player_name] !== undefined && player.game_stats.deaths == previous_deaths[player_name] + 1) {
                // Save the original row background and text color
                let original_background = red_row.style.backgroundColor || '#A10000'; // Light red
                let original_text_color = red_cells[0].style.color || 'white';

                // Change the row background and text color to a darker shade
                red_row.style.backgroundColor = '#800000'; // Darker red
                red_cells.forEach(cell => cell.style.color = '#BBBBBB'); // Darker text shade

                // Set a timeout to revert the color back after 8 seconds
                setTimeout(() => {
                    red_row.style.backgroundColor = original_background; // Revert background color
                    red_cells.forEach(cell => cell.style.color = original_text_color); // Revert text color
                }, highlight_duration * 1000); // Convert seconds to milliseconds
            }
            previous_deaths[player_name] = player.game_stats.deaths; // Update the stored deaths count

            red_cells[0].textContent = player_name;
            red_cells[1].textContent = player.game_stats.kills;
            red_cells[2].textContent = player.game_stats.assists;
            red_cells[3].textContent = player.game_stats.deaths;
        } else {
            let red_cells = red_table_rows[i].querySelectorAll('td');
            red_cells[0].textContent = "";
            red_cells[1].textContent = "";
            red_cells[2].textContent = "";
            red_cells[3].textContent = "";
        }

        if (Object.keys(blue_players).length > i) {
            let blue_row = blue_table_rows[i];
            let blue_cells = blue_row.querySelectorAll('td');
            let player_name = Object.keys(blue_players)[i];
            let player = blue_players[player_name];

            // Check if the deaths increased by 1
            if (previous_deaths[player_name] !== undefined && player.game_stats.deaths == previous_deaths[player_name] + 1) {
                // Save the original row background and text color
                let original_background = blue_row.style.backgroundColor || '#2A4C91'; // Light blue
                let original_text_color = blue_cells[0].style.color || 'white';

                // Change the row background and text color to a darker shade
                blue_row.style.backgroundColor = '#1A3A75'; // Darker blue
                blue_cells.forEach(cell => cell.style.color = '#BBBBBB'); // Darker text shade

                // Set a timeout to revert the color back after 8 seconds
                setTimeout(() => {
                    blue_row.style.backgroundColor = original_background; // Revert background color
                    blue_cells.forEach(cell => cell.style.color = original_text_color); // Revert text color
                }, highlight_duration * 1000); // Convert seconds to milliseconds
            }
            previous_deaths[player_name] = player.game_stats.deaths; // Update the stored deaths count

            blue_cells[3].textContent = player_name;
            blue_cells[0].textContent = player.game_stats.kills;
            blue_cells[1].textContent = player.game_stats.assists;
            blue_cells[2].textContent = player.game_stats.deaths;
        } else {
            let blue_cells = blue_table_rows[i].querySelectorAll('td');
            blue_cells[0].textContent = "";
            blue_cells[1].textContent = "";
            blue_cells[2].textContent = "";
            blue_cells[3].textContent = "";
        }
    }
});

function update() {
    if (current_life_cycle == 'in_game') {
        client.request_players("full");
    }
    client.request_life_cycle();
}

function start() {
    update_handle = setInterval(update, 500);
}

function stop() {
    if (update_handle) {
        clearInterval(update_handle);
        update_handle = null;
    }
}

document.addEventListener("DOMContentLoaded", function () {
    client.connect(`ws://${window.config.host}:${window.config.port}`).then(() => {
        start();
    }).catch((error) => {
        alert("WebSocket Connection Error");
        console.error("Connection Error:", error);
    });
});
