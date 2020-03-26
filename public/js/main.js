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
let bodiesInfo = {};
let availableRooms = [];
let availableEnvironments = [{ name: 'Default', file: 'default' }];
let lastUpdateTime = Date.now();
let nextUpdateTime = Date.now();

// Load sprites
const images = {};
images['parallax_robot'] = new Image();
images['parallax_robot'].src = '/img/parallax_robot.png';
images['parallax_robot'].offsetAngle = Math.PI;
images['parallax_robot'].offset = { left: -0.6, right: 0.6, top: -1, bottom: 1.1 };
images['parallax_robot'].ledPositions = [
    {x: 5, y: -10},
    {x: 5, y: 10}
];

images['omni_robot'] = new Image();
images['omni_robot'].src = '/img/omni_robot.png';
images['omni_robot'].offsetAngle = Math.PI / 2;
images['omni_robot'].offset = { left: 0, right: 0, top: 0, bottom: 0 };

socket.on('availableRooms', data => {
    availableRooms = data.availableRooms;

    $('#rooms-select').html('<option value="-1" selected>Choose...</option>');
    if (data.canCreate) {
        $('.create-text').show();
        $('#rooms-select').append('<option value="create">Create a new room</option>');
    } else {
        $('.create-text').hide();
    }

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
    bodiesInfo = data;
    bodies = data;
    nextBodies = data;
    lastUpdateTime = Date.now();
    nextUpdateTime = Date.now();
});

socket.on('error', error => {
    console.log(error);
});

socket.on('availableEnvironments', list => {
    availableEnvironments = list;

    $('#env-select').html('');
    for (let environment of availableEnvironments) {
        $('#env-select').append(`<option value=${environment.file}>${environment.name}</option>`);
    }
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
        let { height, width, image } = bodiesInfo[label];
        let angle = body.angle;

        // Extrapolate/Interpolate position and rotation
        x += ((nextBodies[label].pos.x - x) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        y += ((nextBodies[label].pos.y - y) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        angle += ((nextBodies[label].angle - angle) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);

        if (images[image] !== undefined) {
            let imageData = images[image];

            // Transform
            context.translate(x, y);
            context.rotate(angle + imageData.offsetAngle);

            // Draw sprite
            context.drawImage(
                imageData,
                -width / 2 + width * imageData.offset.left,
                -height / 2 + width * imageData.offset.top,
                width + width * imageData.offset.right,
                height + width * imageData.offset.bottom
            );

            // Draw LEDs (if available)
            if(imageData.ledPositions !== undefined){
                for (let i in imageData.ledPositions)
                {
                    let pos = imageData.ledPositions[i];
                    context.fillStyle = 'red';
                    context.beginPath();
                    context.arc(pos.x, pos.y, 5, 0, 2 * Math.PI);
                    context.fill();
                }
            }

            // Undo transform
            context.rotate(-angle - imageData.offsetAngle);
            context.translate(-x, -y);
        } else {
            // Transform
            context.translate(x, y);
            context.rotate(angle);

            // Default to rectangle
            context.fillRect(-width / 2, -height / 2, width, height);

            // Undo transform
            context.rotate(-angle);
            context.translate(-x, -y);
        }
    }

    requestAnimationFrame(draw);
}

// Start running immediately
draw();
