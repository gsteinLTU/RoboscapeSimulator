const socketio = require('socket.io');
const socketioclient = require('socket.io-client');
const app = require('express')();

const socketMain = require('./socketMain');

describe('socketMain tests', () =>{
    var server;
    var io;
    var url;

    beforeAll(() => {
        const server = app.listen(0);

        // Start socket.io server
        io = socketio(server);
        socketMain(io);

        url = `http://localhost:${server.address().port}`;
    });

    test('should connect', async () => {
        // Create client 
        let client = socketioclient(url, { autoConnect: false });
        
        // Connect event handler required
        client.on('connect', () => {

        });
        client.open();

        await new Promise((r) => setTimeout(r, 100));

        // Assert that client connected
        expect(client.connected).toBe(true);

        client.disconnect();
    });

    afterAll(() => {
        // Cleanup
        io.close();
    });
    
});