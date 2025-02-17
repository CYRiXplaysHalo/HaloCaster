let client = new websocket_client();
let update_handle = null;
let current_life_cycle = "none";
let current_id = 0;

client.add_message_recieved_callback('life_cycle', (life_cycle_state) => {
    current_life_cycle = life_cycle_state.life_cycle;
});

client.add_message_recieved_callback('game_event_push', (game_event) => {
    var elm = document.createElement("p");
    elm.id = "event_" + current_id;
    elm.textContent = game_event;
    document.body.prepend(elm);
    current_id++;
    fadeOutAndRemove(elm, 2000, 3000);
});

function fadeOutAndRemove(element, duration, delay) {
    var startOpacity = 1;
    var startTime = null;

    function fade(timestamp) {
        if (!startTime) startTime = timestamp;
        var elapsed = timestamp - startTime;
        var progress = elapsed / duration;
        var opacity = Math.max(startOpacity - progress, 0);
        element.style.opacity = opacity;

        if (opacity > 0) {
            requestAnimationFrame(fade);
        } else {
            if (element.parentNode) {
                element.parentNode.removeChild(element);
            }
        }
    }

    // Start the fade-out animation after the specified delay
    setTimeout(function() {
        requestAnimationFrame(fade);
    }, delay);
}

function start()
{
    client.request_feature_enable("events_push");
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
