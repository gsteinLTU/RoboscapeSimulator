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
        let updateInterval = setInterval(() => {
            socket.emit(
                'update',
                _.keyBy(testRoom.getBodies(), body => body.label)
            );
        }, 1000 / settings.updateRate);

        // Clean up on disconnect
        socket.on('disconnect', () => {
            clearInterval(updateInterval);
        });
    });
}

module.exports = socketMain;
