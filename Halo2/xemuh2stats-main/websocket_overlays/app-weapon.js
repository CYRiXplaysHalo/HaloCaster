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

client.add_message_recieved_callback('get_player_weapon', (response) => {
    var table_cells = document.querySelectorAll("#weapon-table tbody td");
    var weapon_name = Object.keys(response)[0];
    var weapon = response[weapon_name];

    table_cells[0].textContent = weapon_name;
    table_cells[1].textContent = weapon.kills;
    table_cells[2].textContent = weapon.deaths;
    table_cells[3].textContent = weapon.suicide;
    table_cells[4].textContent = weapon.shots_hit;
    table_cells[5].textContent = weapon.shots_fired;
    table_cells[6].textContent = weapon.head_shots;
});

client.add_message_recieved_callback('get_netgame_items', (response) => {
    console.log(response);
});

function update()
{
    if(current_life_cycle == 'in_game')
    {
        client.request_player_weapon("kant");
        client.request_netgame_items();
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
