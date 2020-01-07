const _ = require('lodash');

const Room = require('./src/Room');

const settings = {
    updateRate: 60,
    maxRobots: 5
};

/**
 * @param {SocketIO.Server} io
 */
function socketMain(io) {
    // Create a virtual environment for this session
    var testRoom = new Room();

    io.on('connect', socket => {
        console.log(`Socket ${socket.id} connected`);
        socket.join('testroom');

        // Create robot if not too many
        let robot = null;

        if (testRoom.robots.length < settings.maxRobots) {
            robot = testRoom.addRobot();
        }

        // Begin sending updates
        socket.emit(
            'fullUpdate',
            _.keyBy(testRoom.getBodies(false), body => body.label)
        );
        let updateInterval = setInterval(() => {
            let updateBodies = testRoom.getBodies(true);

            if (updateBodies.length > 0) {
                socket.emit(
                    'update',
                    _.keyBy(updateBodies, body => body.label)
                );
            }
        }, 1000 / settings.updateRate);

        // Temporary feature to reset example environment
        socket.on('reset', confirm => {
            if (confirm) {
                testRoom = new Room();
                io.to('testroom').emit(
                    'fullUpdate',
                    _.keyBy(testRoom.getBodies(false), body => body.label)
                );
            }
        });

        // Clean up on disconnect
        socket.on('disconnect', () => {
            clearInterval(updateInterval);
        });
    });
}

module.exports = socketMain;
