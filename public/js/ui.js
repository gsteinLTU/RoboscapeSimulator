/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */

$('#env-select-group').hide();
$('#room-modal').modal({ keyboard: false, backdrop: 'static' });
$('#rooms-select').change(e => {
    $('#room-join-button').prop('disabled', e.target.value == '-1');

    if (e.target.value == 'create') {
        $('#env-select-group').show();
    } else {
        $('#env-select-group').hide();
    }
});

$('#room-join-button').click(() => {
    socket.emit('joinRoom', { roomID: $('#rooms-select').val(), env: $('#env-select').val() }, result => {
        console.log(result);
        if (result !== false) {
            // Reset camera settings
            cameraPos.x = 0;
            cameraPos.y = 0;
            cameraZoom = 1;

            // Start running
            draw();
            $('#room-modal').modal('hide');
            $('#side-panel').removeClass('hidden');
            $('#mainCanvas').focus();
        } else {
            $('#room-error').show();
        }
    });
});

// Window resize handler
window.addEventListener('resize', () => {
    wHeight = $(window).height();
    wWidth = $(window).width();
    canvas.width = wWidth;
    canvas.height = wHeight;
});

// Capture key events from canvas
$('#mainCanvas').keydown(e => {
    keysdown.add(e.which);
});

$('#mainCanvas').keyup(e => {
    keysdown.delete(e.which);
});

// Hide/show side panel
$('#panel-toggle').click(e => {
    $('#side-panel').toggleClass('collapsed');
});

function updateRobotsPanel(){
    $('#robomenu').html('');

    for(let body of Object.values(bodiesInfo)){
        // Check for robots by detecting MAC address label
        if (/^([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}$/.test(body.label)){
            $('#robomenu').append(
                `<li>
                    <a href="#">${body.label}</a>
                    <ul class="list-unstyled robosublist">
                        <!-- <li>
                            <button class="btn btn-light">Button</button>
                        </li> --!>
                    </ul>
                </li>`);
        }
    }
    
}