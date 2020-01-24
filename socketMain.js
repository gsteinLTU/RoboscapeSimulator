const _ = require('lodash');
const debug = require('debug')('roboscape-sim:socketMain');

const Room = require('./src/Room');

const settings = {
    updateRate: 30,
    maxRobots: 5
};

/**
 * @param {SocketIO.Server} io
 */
function socketMain(io) {
    // Create a virtual environment for this session
    var testRoom = new Room();

    function sendFullUpdate(socket) {
        socket.emit(
            'fullUpdate',
            _.keyBy(testRoom.getBodies(false), body => body.label)
        );
    }

    function sendUpdate(socket) {
        let updateBodies = testRoom.getBodies(true);

        if (updateBodies.length > 0) {
            socket.emit(
                'update',
                _.keyBy(updateBodies, body => body.label)
            );
        }
    }

    let updateInterval = setInterval(() => {
        // Check for dead bots
        if (testRoom.removeDeadRobots() !== false) {
            sendFullUpdate(io.to('testroom'));
        } else {
            sendUpdate(io.to('testroom'));
        }
    }, 1000 / settings.updateRate);

    io.on('connect', socket => {
        debug(`Socket ${socket.id} connected`);
        socket.join('testroom');

        // Create robot if not too many
        let robot = null;

        if (testRoom.robots.length < settings.maxRobots) {
            // Add new robot and tell everyone about it
            robot = testRoom.addRobot();
            sendFullUpdate(io.to('testroom'));
        } else {
            // Begin sending updates
            sendFullUpdate(socket);
        }

        // Temporary feature to reset example environment
        socket.on('reset', confirm => {
            if (confirm) {
                testRoom = new Room();
                sendFullUpdate(io.to('testroom'));
            }
        });

        // Temporary feature to demonstrate input from VR
        socket.on('controllerInput', data => {
            robot.sendToServer('F0');
        });

        // Clean up on disconnect
        socket.on('disconnect', () => {
            clearInterval(updateInterval);
        });
    });
}

module.exports = socketMain;
