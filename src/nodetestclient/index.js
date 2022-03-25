const io = require("socket.io-client");

const numClients = Number.parseInt(process.argv[2] ?? '1');
console.log(`Starting ${numClients} client${numClients > 1 ? 's' : ''}...`);

if (numClients <= 0) {
    throw new Error("Invalid number of clients");
}

let knownRooms = [];

//for (let i = 0; i < numClients; i++) {
let createSocket = (i) => {
    const socket = io("http://localhost:9001", { forceNew: true, withCredentials: false });
    const user = Math.round(Math.random() * 100000);
    let averageDiff = 0;
    let numUpdates = 0;
    let lastUpdate = Date.now();
    let room = undefined;

    const log = (msg) => {
        console.log(`${Date.now()} Client ${i}: ${msg}`);
    };

    socket.on('connect', (e) => {
        log('Client connected');
    });


    socket.on('reconnect', (e) => {
        log('Reconnected');
        if (room !== undefined) {
            socket.emit('joinRoom', { roomID: room });
        }
    });
    socket.on('u', () => {
        let now = Date.now();

        let diff = now - lastUpdate;
        averageDiff = ((averageDiff * numUpdates) + diff) / ++numUpdates;
        lastUpdate = now;

        //console.log('Update received');
    });

    socket.on('fullUpdate', () => {
        if (averageDiff == 0) {
            log(`Full Update received`);
        } else {
            log(`Full Update received, average update rate: ${1000 / averageDiff} per second`);
        }
    });

    socket.on('connect_error', (e) => {
        log('Error connecting: ' + e);
    });

    socket.on('availableRooms', (e) => {
        //log('Available Rooms received');
    });

    socket.on('availableEnvironments', (e) => {
        //log('Available Enviroments received');
    });

    socket.on('roomJoined', result => {
        if (result != false) {
            log('Joined room ' + result);
            room = result;
            knownRooms.push(room);
            lastUpdate = Date.now();
        } else {
            log('Failed to join room');
        }
    });

    socket.connect();

    socket.emit('getRooms', user);

    //setTimeout(() => {
    //    socket.emit('joinRoom', { roomID: 'create', env: 'default', password: '', namespace: user });
    //}, 250);

    let checkInRoom = () => {
        if (room == undefined) {
            socket.emit('joinRoom', { roomID: 'create', env: 'default', password: '', namespace: user });
            setTimeout(checkInRoom, 1000);
        }
    };

    checkInRoom();

    if (i > 1) {
        setTimeout(createSocket.bind(null, i - 1), 300);
    }
};

//let i = numClients;
setTimeout(createSocket.bind(null, numClients), 100);
