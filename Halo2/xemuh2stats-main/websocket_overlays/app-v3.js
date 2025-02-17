let client = new websocket_client();
let update_handle = null;
let current_life_cycle = "none";

// Store the previous deaths count for each player
let previous_deaths = {};

// Duration to highlight the row (in seconds)
let highlight_duration = 10; // Default value

// Store map name
let current_map_name = '';

// Add a handler to update highlight_duration and store map_name
client.add_message_recieved_callback('get_variant_details', (map_name, game_type) => {
    // Store the map name for later use
    current_map_name = map_name;
    console.log('Map Name:', current_map_name);

    // Set the highlight_duration based on game_type
    if (game_type === 2) {
        highlight_duration = 5; // Set highlight_duration to 5 if game_type is 2
    } else {
        highlight_duration = 10; // Default highlight_duration
    }
    console.log('Updated highlight_duration:', highlight_duration);
});

function players_sort_team(a, b) {
    if (a.player.team_index < b.player.team_index) {
        return -1;
    }
    if (a.player.team_index > b.player.team_index) {
        return 1;
    }
    return 0;
}

// Function to get the weapon for a player
function get_player_weapon(player_name, callback) {
    client.add_message_recieved_callback('get_player_weapon', (response) => {
        let weapon_name = Object.keys(response)[0]; // Extract weapon name
        callback(weapon_name); // Pass the weapon name to the callback function
    });
    
    client.request_player_weapon(player_name); // Request weapon data for the player
}

// Function to create and return an img element for the weapon image
function create_weapon_image(weapon_name) {
    // Remove spaces in the weapon name to match the image file
    let image_name = weapon_name.replace(/\s+/g, '');
    let img = document.createElement('img');
    img.src = `Weapons/${image_name}.png`; // Path to the weapon image
    img.alt = ``; // Leave alt blank for now
    img.style.float = 'right'; // Align image to the right
    img.style.height = '30px'; // Set image size (adjust as necessary)
    return img;
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
        if (player.player.team_index == 0) {
            red_players[player_name] = player;
        }
        if (player.player.team_index == 1) {
            blue_players[player_name] = player;
        }
    });

    let red_table_rows = document.querySelectorAll('#red-team-table tbody tr');
    let blue_table_rows = document.querySelectorAll("#blue-team-table tbody tr");

    player_names.forEach(player_name => {
		get_player_weapon(player_name, (weapon_name) => {
			// Process red players
			for (let i = 0; i < 4; i++) {
				if (Object.keys(red_players).length > i) {
					let red_row = red_table_rows[i];
					let red_cells = red_row.querySelectorAll('td');
					let red_player_name = Object.keys(red_players)[i];
					let player = red_players[red_player_name];

					// Check if the deaths increased by 1
					if (previous_deaths[red_player_name] !== undefined && player.game_stats.deaths == previous_deaths[red_player_name] + 1) {
						let original_background = red_row.style.backgroundColor || '#A10000'; // Light red
						let original_text_color = red_cells[0].style.color || 'white';
						red_row.style.backgroundColor = '#800000'; // Darker red
						red_cells.forEach(cell => cell.style.color = '#BBBBBB'); // Darker text shade
						setTimeout(() => {
							red_row.style.backgroundColor = original_background;
							red_cells.forEach(cell => cell.style.color = original_text_color);
						}, highlight_duration * 1000); // Use updated highlight_duration
					}
					previous_deaths[red_player_name] = player.game_stats.deaths; // Update deaths count

					red_cells[0].textContent = red_player_name; // Keep player name left aligned
					red_cells[0].style.textAlign = 'left'; // Ensure text is left-aligned
					
					// Create a new weapon image element for this player
					let weapon_img = create_weapon_image(weapon_name);
					console.log('Red Team');
					console.log(weapon_img);
					red_cells[0].appendChild(weapon_img); // Add weapon image to the cell

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
			}

			// Process blue players
			for (let i = 0; i < 4; i++) {
				if (Object.keys(blue_players).length > i) {
					let blue_row = blue_table_rows[i];
					let blue_cells = blue_row.querySelectorAll('td');
					let blue_player_name = Object.keys(blue_players)[i];
					let player = blue_players[blue_player_name];

					// Check if the deaths increased by 1
					if (previous_deaths[blue_player_name] !== undefined && player.game_stats.deaths == previous_deaths[blue_player_name] + 1) {
						let original_background = blue_row.style.backgroundColor || '#2A4C91'; // Light blue
						let original_text_color = blue_cells[0].style.color || 'white';
						blue_row.style.backgroundColor = '#1A3A75'; // Darker blue
						blue_cells.forEach(cell => cell.style.color = '#BBBBBB'); // Darker text shade
						setTimeout(() => {
							blue_row.style.backgroundColor = original_background;
							blue_cells.forEach(cell => cell.style.color = original_text_color);
						}, highlight_duration * 1000); // Use updated highlight_duration
					}
					previous_deaths[blue_player_name] = player.game_stats.deaths; // Update deaths count

					blue_cells[3].textContent = blue_player_name; // Keep player name left aligned
					blue_cells[3].style.textAlign = 'left'; // Ensure text is left-aligned
					
					// Create a new weapon image element for this player
					let weapon_img = create_weapon_image(weapon_name);
					console.log('Blue Team');
					console.log(weapon_img);
					blue_cells[3].appendChild(weapon_img); // Add weapon image to the cell

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
	});


});



function update() {
    if (current_life_cycle == 'in_game') {
        client.request_players("full");
    }
    client.request_life_cycle();
    client.request_variant_details(); // Request the variant details to get map name and game type
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
