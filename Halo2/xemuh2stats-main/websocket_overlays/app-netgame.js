let client = new websocket_client();
let update_handle = null;
let current_life_cycle = "none";

client.add_message_recieved_callback('life_cycle', (life_cycle_state) => {
    current_life_cycle = life_cycle_state.life_cycle;
});

client.add_message_recieved_callback('get_netgame_items', (response) => {
    for(var i = 0; i < response.length; i++)
    {
        var item = response[i];
        if(item.respawn_time > 60)
            item.respawn_time = 60;
        var element = document.querySelector(`[data-type="${item.type}"]`);
        if(element == null) {
            var clone = document.querySelector("#item-template").content.cloneNode(true);
            clone.querySelector('.netgame-item').setAttribute("data-type", item.type);
            clone.querySelector('.netgame-name').textContent = item.type;
            clone.querySelector('.netgame-progress-time').textContent = item.respawn_timer;
            clone.querySelector('.netgame-progress-line').style = "width:0%;";
            document.querySelector('.netgame-container').appendChild(clone);
            element = clone;
            element.style = "display:none;";
        }

        if(item.is_respawning)
        {
            element.style = "display:block;";
            var progress = (item.respawn_timer / item.respawn_time) * 100;
            //console.log(progress);
            element.querySelector('.netgame-progress-time').textContent = item.respawn_timer;
            element.querySelector('.netgame-progress-line').style = `width:${progress}%;`;
        } 
        else
        {
            element.style = "display:none;";
        }
    }
});

function update()
{
    if(current_life_cycle == 'in_game')
    {
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
