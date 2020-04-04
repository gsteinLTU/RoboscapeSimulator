const _ = require('lodash');
const debug = require('debug')('roboscape-sim:socketMain');

const Room = require('./Room');

const settings = {
    updateRate: 20,
    maxRobots: 5,
    maxRooms: 5
};

const rooms = [];

/**
 * @param {SocketIO.Server} io
 */
function socketMain(io) {
    /**
     * Sends all body information to socket
     * @param {SocketIO.Socket} socket Socket to send update to
     * @param {String} room Room to get bodies from
     */
    function sendFullUpdate(socket, room) {
        socket.emit(
            'fullUpdate',
            _.keyBy(room.getBodies(false, true), body => body.label)
        );
    }

    /**
     * Sends update-relevant (position, orientation) body information to socket
     * @param {SocketIO.Socket} socket Socket to send update to
     * @param {String} room Room to get bodies from
     */
    function sendUpdate(socket, room) {
        let updateBodies = room.getBodies(true, false);

        if (updateBodies.length > 0) {
            socket.emit(
                'update',
                _.keyBy(updateBodies, body => body.label)
            );
        }
    }

    /**
     * Sends list of rooms to socket
     * @param {SocketIO.Socket} socket Socket to send list to
     */
    function sendAvailableRooms(socket) {
        socket.emit('availableRooms', { availableRooms: rooms.map(room => room.roomID), canCreate: rooms.length < settings.maxRooms });
        Room.listEnvironments().then(list => {
            socket.emit('availableEnvironments', list);
        });
    }

    /**
     * Add a user to a room
     * @param {String} roomID Room to join
     * @param {SocketIO.Socket} socket The user's socket
     */
    function joinRoom(roomID, socket) {
        let room = rooms[rooms.map(room => room.roomID).indexOf(roomID)];
        socket.join(roomID);
        socket.activeRoom = room;
        
        // Create robot if not too many
        if (room.robots.length < settings.maxRobots) {
            // Add new robot and tell everyone about it
            room.addRobot();
            sendFullUpdate(io.to(roomID), room);
        } else {
            // Begin sending updates
            sendFullUpdate(socket, room);
        }
    }

    // eslint-disable-next-line no-unused-vars
    let updateInterval = setInterval(() => {
        for (let room of rooms) {
            // Check for dead bots
            if (room.removeDeadRobots()) {
                sendFullUpdate(io.to(room.roomID), room);
            } else {
                sendUpdate(io.to(room.roomID), room);
            }
        }
    }, 1000 / settings.updateRate);

    io.on('connect', socket => {
        debug(`Socket ${socket.id} connected`);

        // Join room for users in no real room
        socket.join('waiting-room');
        socket.activeRoom = null;

        sendAvailableRooms(socket);

        // Allow joining a room
        socket.on('joinRoom', (data, cb) => {
            // Check if in waiting-room
            if (Object.keys(socket.rooms).indexOf('waiting-room') !== -1) {
                let roomID = data.roomID;

                // Check that room is valid
                if (rooms.map(room => room.roomID).indexOf(roomID) !== -1) {
                    joinRoom(roomID, socket);

                    socket.leave('waiting-room');

                    cb(roomID);
                } else if (roomID === 'create' && rooms.length < settings.maxRooms) {
                    debug(`Socket ${socket.id} requested to create room`);
                    // Create a virtual environment
                    let tempRoom = new Room({ environment: data.env });
                    rooms.push(tempRoom);

                    // Delay to allow environment to finish loading from file first
                    setTimeout(() => {
                        roomID = tempRoom.roomID;
                        joinRoom(roomID, socket);
                        cb(roomID);
                        socket.leave('waiting-room');

                        // Tell other users about the new room
                        sendAvailableRooms(io.to('waiting-room'));
                    }, 100);
                } else {
                    debug(`Socket ${socket.id} attempted to join invalid room!`);
                    cb(false);
                }
            } else {
                debug(`Socket ${socket.id} attempted to join second room!`);
                cb(false);
            }
        });

        socket.on('clientEvent', eventData => {
            // Check if in real room
            if (socket.activeRoom != null) {
                let {type, data} = eventData;

                // Send to Room to handle
                socket.activeRoom.onClientEvent(type, data, socket);
            }
        });
    });
}

module.exports = socketMain;
