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
let running = true;
let keysdown = new Set();

// Camera data
let cameraPos = {x: 0, y: 0};
let cameraZoom = 1;

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
    updateRobotsPanel();
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

