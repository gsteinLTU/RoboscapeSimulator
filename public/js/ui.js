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
    joinRoom($('#rooms-select').val(), $('#env-select').val());
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

function updateRobotsPanel() {
    $('#robomenu').html('');

    for (let body of Object.values(bodiesInfo)) {
        // Check for robots by detecting MAC address label
        if (/^([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}$/.test(body.label)) {
            $('#robomenu').append(
                `<li class="roboinfo" data-robot="${body.label}">
                    <a href="#">${body.label}</a>
                    ${body.image == 'parallax_robot' ? `<ul class="list-unstyled robosublist">
                        <li>
                            <button class="btn btn-light hwbtn">Hardware Button</button>
                        </li>
                    </ul>` : ''}
                </li>`);
        }
    }


    $('.hwbtn').mousedown(handleHWButton.bind(null, true));

    $('.hwbtn').mouseup(handleHWButton.bind(null, false));

    function handleHWButton(val, e) {
        let mac = $(e.target).closest('.roboinfo')[0].dataset['robot'];
        // Tell server button was clicked
        sendClientEvent('parallax_hw_button', { mac: mac, status: val });
    }
}