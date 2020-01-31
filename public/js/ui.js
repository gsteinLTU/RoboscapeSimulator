/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */

$('#room-modal').modal({ keyboard: false, backdrop: 'static' });
$('#rooms-select').change(e => {
    $('#room-join-button').prop('disabled', e.target.value == '-1');
});

$('#room-join-button').click(() => {
    socket.emit('joinRoom', { roomID: $('#rooms-select').val() }, result => {
        console.log(result);
        if (result !== false) {
            $('#room-modal').modal('hide');
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
