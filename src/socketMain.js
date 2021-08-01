const _ = require('lodash');
const debug = require('debug')('roboscape-sim:socketMain');

const Room = require('./Room');

const settings = {
    updateRate: 15,
    maxRobots: 3,
    maxRooms: 10
};

const rooms = [];

/**
 * @param {} io
 */
function socketMain(io) {
    debug('Setting up geckos server...');
    
    /**
     * Sends all body information to socket
     * @param {Object} socket Socket to send update to
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
     * @param {Object} socket Socket to send update to
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
     * @param {Object} socket Socket to send list to
     */
    function sendAvailableRooms(socket) {
        debug('Sending available rooms to ' + socket.id);
        socket.emit('availableRooms', { availableRooms: rooms.map(room => room.roomID), canCreate: rooms.length < settings.maxRooms });
        Room.listEnvironments().then(list => {
            socket.emit('availableEnvironments', list);
        });
    }

    /**
     * Add a user to a room
     * @param {String} roomID Room to join
     * @param {Object} socket The user's socket
     */
    function joinRoom(roomID, socket) {
        let room = rooms[getRoomIndex(roomID)];
        socket.join(roomID);
        socket.activeRoom = room;

        // Give new client information about room
        room.sendRoomInfo(socket);

        // Create robot if not too many
        if (room.robots.length < settings.maxRobots) {
            // Add new robot and tell everyone about it
            room.addRobot();
            sendFullUpdate(io.room(roomID), room);
        } else {
            // Begin sending updates
            sendFullUpdate(socket, room);
        }
    }

    /**
     * Get the index of a room ID in the list of all rooms
     * @param {String} roomID ID of Room to locate
     */
    function getRoomIndex(roomID) {
        return rooms.map(room => room.roomID).indexOf(roomID);
    }

    // eslint-disable-next-line no-unused-vars
    let updateInterval = setInterval(() => {
        for (let room of rooms) {
            // Check for dead bots
            if (room.removeDeadRobots()) {
                sendFullUpdate(io.room(room.roomID), room);
            } else {
                sendUpdate(io.room(room.roomID), room);
            }
        }
    }, 1000 / settings.updateRate);

    io.onConnection(socket => {
        debug(`Socket ${socket.id} connected`);

        // Join room for users in no real room
        socket.join('waiting-room');
        socket.activeRoom = null;

        sendAvailableRooms(socket);

        // Allow joining a room
        socket.on('joinRoom', (data) => {

            let { roomID, env } = data;

            // Check if in waiting-room
            if (socket.roomId == 'waiting-room') {

                // Check that room is valid
                if (getRoomIndex(roomID) !== -1) {
                    joinRoom(roomID, socket);
                    socket.emit('roomJoined', roomID);
                } else if (roomID === 'create' && rooms.length < settings.maxRooms) {
                    debug(`Socket ${socket.id} requested to create room ${env}`);
                    // Create a virtual environment
                    let tempRoom = new Room({ environment: env });
                    rooms.push(tempRoom);

                    // Delay to allow environment to finish loading from file first
                    setTimeout(() => {
                        roomID = tempRoom.roomID;
                        joinRoom(roomID, socket);
                        socket.emit('roomJoined', roomID);

                        // Tell other users about the new room
                        sendAvailableRooms(io.room('waiting-room'));
                    }, 100);
                } else {
                    debug(`Socket ${socket.id} attempted to join invalid room!`);
                    socket.emit('roomJoined', false);
                }
            } else {
                debug(`Socket ${socket.id} attempted to join second room!`);
                socket.emit('roomJoined', false);
            }
        });

        socket.on('clientEvent', eventData => {
            // Check if in real room
            if (socket.activeRoom != null) {
                let { type, data } = eventData;

                // Send to Room to handle
                socket.activeRoom.onClientEvent(type, data, socket);
            }
        });

        // If user reconnects, determine if they can be readded to their room
        socket.on('postReconnect', roomID => {
            if (roomID != null && getRoomIndex(roomID) !== -1) {
                joinRoom(roomID, socket);
            } else {
                debug(`Client attempted to join room ${roomID} that no longer exists`);
                socket.emit('forceRefesh', 0);
            }
        });
    });

    io.listen(9208);
}

module.exports = socketMain;