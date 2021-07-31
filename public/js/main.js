/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */

let wHeight = $(window).height();
let wWidth = $(window).width();
let canvas = document.querySelector('#mainCanvas');
let context = canvas.getContext('2d');
canvas.width = wWidth;
canvas.height = wHeight;

let socket = geckos();
let bodies = {};
let nextBodies = {};
let bodiesInfo = {};
let roomID = null;
let roomInfo = {};
let roomBG = new Image();
let availableRooms = [];
let availableEnvironments = [{ name: 'Default', file: 'default' }];
let lastUpdateTime = Date.now();
let nextUpdateTime = Date.now();
let running = true;
let keysdown = new Set();

// Camera data
let cameraPos = { x: 0, y: 0 };
let cameraZoom = 1;

// Load sprites
const images = {};
images['parallax_robot'] = new Image();
images['parallax_robot'].src = '/img/robots/parallax_robot.png';
images['parallax_robot'].offsetAngle = Math.PI;
images['parallax_robot'].offset = { left: -0.6, right: 0.6, top: -1, bottom: 1.1 };
images['parallax_robot'].ledPositions = [
    { x: 5, y: -10 },
    { x: 5, y: 10 }
];

images['omni_robot'] = new Image();
images['omni_robot'].src = '/img/robots/omni_robot.png';
images['omni_robot'].offsetAngle = Math.PI / 2;
images['omni_robot'].offset = { left: 0, right: 0, top: 0, bottom: 0 };

/**
 * Replace HTML characters with entities for safe display
 * Thanks to Ben Vinegar
 * @param {string} str String to be sanitized
 * @returns {string} str with risky characters replaced
 */
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;')
        .replace(/\//g, '&#x2F;');
}


socket.onConnect(e => {
    // Populate available rooms list when received
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

    // Handle room info
    socket.on('roomInfo', info => {
        roomInfo = info;

        if (info.background != '') {
            roomBG.src = `/img/backgrounds/${info.background}.png`;
        }
    });

    socket.on('error', error => {
        console.log(error);
    });

    // Populate environments list when received
    socket.on('availableEnvironments', list => {
        availableEnvironments = list;

        $('#env-select').html('');
        for (let environment of availableEnvironments) {
            let optionNode = document.createElement('option');
            optionNode.setAttribute('value', environment.file);
            optionNode.innerHTML = escapeHtml(environment.name);
            $('#env-select').append(optionNode);
        }
    });

    // If we were previously connected, let server know we had an issue
    socket.on('reconnect', attempt => {
        console.log(`Reconnected after ${attempt} attempts!`);
        socket.emit('postReconnect', roomID);
    });

    // Allow server to request refresh
    socket.on('forceRefesh', reason => {
        location.reload();
    });

    // Room joined message
    socket.on('roomJoined', result => {
        if (result !== false) {
            console.log(`Joined room ${result}`);
            roomID = result;

            // Create room link
            let roomLink = window.location.protocol + '//' + window.location.host + window.location.pathname + '?join=' + roomID;
            $('#room-link').attr('href', roomLink);
            $('#room-link').html(escapeHtml(roomLink));
            $('#room-link-copy').click(() => {
                navigator.clipboard.writeText(roomLink);
            });

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
            // Failed to join room
            $('#room-error').show();
        }
    });

    // Detect request to join existing room
    let urlParams = new URLSearchParams(window.location.search);
    if (urlParams.has('join')) {
        joinRoom(urlParams.get('join'));

        // Remove param from url
        if (window.history.pushState) {
            urlParams.delete('join');
            window.history.pushState({}, document.title, window.location.protocol + '//' + window.location.host + window.location.pathname + '?' + urlParams.toString());
        }
    }
});

function sendClientEvent(type, data) {
    socket.emit('clientEvent', { type: type, data: data });
}

/**
 * Send message to join room
 * @param {string} room
 * @param {string} env
 */
function joinRoom(room, env = '') {
    // Prevent joining a second room
    if (roomID !== null) {
        throw 'Already in room.';
    }

    socket.emit('joinRoom', { roomID: room, env });

}
