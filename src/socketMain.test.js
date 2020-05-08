const socketio = require('socket.io');
const socketioclient = require('socket.io-client');
const app = require('express')();

const socketMain = require('./socketMain');

describe('socketMain tests', () =>{
    var io;
    var url;
    var client;

    beforeAll(() => {
        const server = app.listen(0);

        // Start socket.io server
        io = socketio(server);
        socketMain(io);

        url = `http://localhost:${server.address().port}`;
    });

    beforeEach(() => {
        // Create client 
        client = socketioclient(url, { autoConnect: false });

        // Connect event handler required
        client.on('connect', () => {

        });
    });

    afterEach(() => {
        // Clean up clients
        if(client != null && client.connected){
            client.disconnect();
        }
    });

    test('should connect', async () => {
        client.open();

        await new Promise((r) => setTimeout(r, 100));

        // Assert that client connected
        expect(client.connected).toBe(true);
    });


    test('should get room list and canCreate after connect', async () => {
        let list = null;
        let create = null;

        client.on('availableRooms', (data) => {
            list = data.availableRooms;
            create = data.canCreate;
        });

        client.open();

        await new Promise((r) => setTimeout(r, 200));

        // Assert that client was given a list
        expect(list).not.toBe(null);
        expect(create).toBe(true);
    });


    afterAll(() => {
        // Cleanup
        io.close();
    });
    
});