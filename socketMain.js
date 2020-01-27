const _ = require('lodash');
const debug = require('debug')('roboscape-sim:socketMain');

const Room = require('./src/Room');

const settings = {
    updateRate: 30,
    maxRobots: 5
};

const rooms = [];

/**
 * @param {SocketIO.Server} io
 */
function socketMain(io) {
    // Create a virtual environment
    rooms.push(new Room());

    function sendFullUpdate(socket, room) {
        socket.emit(
            'fullUpdate',
            _.keyBy(room.getBodies(false), body => body.label)
        );
    }

    function sendUpdate(socket, room) {
        let updateBodies = room.getBodies(true);

        if (updateBodies.length > 0) {
            socket.emit(
                'update',
                _.keyBy(updateBodies, body => body.label)
            );
        }
    }

    let updateInterval = setInterval(() => {
        for (let room of rooms) {
            // Check for dead bots
            if (room.removeDeadRobots() !== false) {
                sendFullUpdate(io.to(room.roomID), room);
            } else {
                sendUpdate(io.to(room.roomID), room);
            }
        }
    }, 1000 / settings.updateRate);

    io.on('connect', socket => {
        debug(`Socket ${socket.id} connected`);

        socket.emit(
            'availableRooms',
            rooms.map(room => room.roomID)
        );

        // Allow joining a room
        socket.on('joinRoom', data => {
            let roomID = data.roomID;

            // Check that room is valid
            if (rooms.map(room => room.roomID).indexOf(roomID) !== -1) {
                let room = rooms[rooms.map(room => room.roomID).indexOf(roomID)];

                socket.join(roomID);

                // Create robot if not too many
                let robot = null;

                if (room.robots.length < settings.maxRobots) {
                    // Add new robot and tell everyone about it
                    robot = room.addRobot();
                    sendFullUpdate(io.to(roomID), room);
                } else {
                    // Begin sending updates
                    sendFullUpdate(socket, room);
                }

                // Temporary feature to reset example environment
                socket.on('reset', confirm => {
                    if (confirm) {
                        room.close();
                        rooms[roomID] = new Room({ roomID: roomID });
                        room = rooms[roomID];
                        sendFullUpdate(io.to(roomID), room);
                    }
                });
            }
        });
    });
}

module.exports = socketMain;
