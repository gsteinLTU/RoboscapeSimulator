/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */

let wHeight = $(window).height();
let wWidth = $(window).width();
let canvas = document.querySelector('#mainCanvas');
let context = canvas.getContext('2d');
canvas.width = wWidth;
canvas.height = wHeight;

let socket = io.connect();
let bodies = {};
let nextBodies = {};
let availableRooms = [];
let lastUpdateTime = Date.now();
let nextUpdateTime = Date.now();

socket.on('availableRooms', data => {
    availableRooms = data;

    //$('#rooms-select').append('<option value="create">Create a new room</option>');

    for (let room of availableRooms) {
        $('#rooms-select').append(`<option value=${room}>${room}</option>`);
    }
});

// Handle incremental updates
socket.on('update', data => {
    bodies = { ...nextBodies };
    nextBodies = { ...bodies, ...data };
    lastUpdateTime = nextUpdateTime;
    nextUpdateTime = Date.now();
});

// Handle full updates
socket.on('fullUpdate', data => {
    bodies = data;
    nextBodies = data;
    lastUpdateTime = Date.now();
    nextUpdateTime = Date.now();
});

function reset() {
    socket.emit('reset', true);
}

function draw() {
    // Reset canvas
    context.setTransform(1, 0, 0, 1, 0, 0);
    context.clearRect(-wWidth, -wHeight, wWidth * 2, wHeight * 2);

    let frameTime = Date.now();

    for (let label of Object.keys(bodies)) {
        let body = bodies[label];
        context.fillStyle = '#222222';

        let { x, y } = body.pos;
        let angle = body.angle;

        // Extrapolate/Interpolate position and rotation
        x += ((nextBodies[label].pos.x - x) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        y += ((nextBodies[label].pos.y - y) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        angle += ((nextBodies[label].angle - angle) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);

        // Draw rect for body
        context.translate(x, y);
        context.rotate(angle);
        context.fillRect(-body.width / 2, -body.height / 2, body.width, body.height);
        context.rotate(-angle);
        context.translate(-x, -y);
    }

    requestAnimationFrame(draw);
}

// Window resize handler
window.addEventListener('resize', () => {
    wHeight = $(window).height();
    wWidth = $(window).width();
    canvas.width = wWidth;
    canvas.height = wHeight;
});

// Start running immediately
draw();

$('#modal').modal({ keyboard: false, backdrop: 'static' });
$('#rooms-select').change(e => {
    $('#room-join-button').prop('disabled', e.target.value == '-1');
});

$('#room-join-button').click(() => {
    socket.emit('joinRoom', { roomID: $('#rooms-select').val() });
    $('#modal').modal('hide');
});
