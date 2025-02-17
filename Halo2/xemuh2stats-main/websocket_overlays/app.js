let client = new websocket_client();
let update_handle = null;
let current_life_cycle = "none";

function players_sort_team(a, b ) {
    if ( a.player.team_index < b.player.team_index ){
      return -1;
    }
    if ( a.player.team_index > b.player.team_index ){
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
        if(player.player.team_index == 0)
            red_players[player_name] = player;
        if(player.player.team_index == 1)
            blue_players[player_name] = player;
    });

    let red_table_rows = document.querySelectorAll('#red-team-table tbody tr');
    let blue_table_rows = document.querySelectorAll("#blue-team-table tbody tr");

    for(let i = 0; i < 4; i++)
    {
        if(Object.keys(red_players).length > i)
        {
            let red_cells = red_table_rows[i].querySelectorAll('td');
            let player_name = Object.keys(red_players)[i];
            let player = red_players[player_name];
            red_cells[0].textContent = player_name;
            red_cells[1].textContent = player.game_stats.kills;
            red_cells[2].textContent = player.game_stats.assists;
            red_cells[3].textContent = player.game_stats.deaths;
        }
        else
        {
            let red_cells = red_table_rows[i].querySelectorAll('td');
            red_cells[0].textContent = "";
            red_cells[1].textContent = "";
            red_cells[2].textContent = "";
            red_cells[3].textContent = "";
        }

        if(Object.keys(blue_players).length > i)
        {
            let blue_cells = blue_table_rows[i].querySelectorAll('td');
            let player_name = Object.keys(blue_players)[i];
            let player = blue_players[player_name];
            blue_cells[3].textContent = player_name;
            blue_cells[0].textContent = player.game_stats.kills;
            blue_cells[1].textContent = player.game_stats.assists;
            blue_cells[2].textContent = player.game_stats.deaths;
        }
        else
        {
            let blue_cells = blue_table_rows[i].querySelectorAll('td');
            blue_cells[0].textContent = "";
            blue_cells[1].textContent = "";
            blue_cells[2].textContent = "";
            blue_cells[3].textContent = "";
        }
    }
});


function update()
{
    if(current_life_cycle == 'in_game')
    {
        client.request_players("full");
    }
    client.request_life_cycle();
}

function start()
{
    update_handle = setInterval(update, 500);
}

function stop()
{
    if(update_handle)
    {
        clearInterval(update_handle);
        update_handle = null;
    }
}

document.addEventListener("DOMContentLoaded", function(){
    client.connect(`ws://${window.config.host}:${window.config.port}`).then(() =>  {
        start();
    }).catch((error) => {
        alert("WebSocket Connection Error");
        console.error("Connection Error:", error);
    });
});
