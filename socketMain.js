const _ = require('lodash');

const Room = require('./src/Room');

const settings = {
    updateRate: 60
};

/**
 * @param {SocketIO.Server} io
 */
function socketMain(io) {
    io.on('connect', socket => {
        console.log(`Socket ${socket.id} connected`);

        // Create a virtual environment for this session
        const testRoom = new Room();

        // Begin sending updates
        socket.emit(
            'update',
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

        // Clean up on disconnect
        socket.on('disconnect', () => {
            clearInterval(updateInterval);
        });
    });
}

module.exports = socketMain;
