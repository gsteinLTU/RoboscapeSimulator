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

        await new Promise((r) => setTimeout(r, 150));

        // Assert that client was given a list
        expect(list).not.toBe(null);
        expect(create).toBe(true);
    });

    test('should be able to create room', async () => {
        let list = null;
        let create = null;
        let createdRoom = null;

        client.on('availableRooms', (data) => {
            list = data.availableRooms;
            create = data.canCreate;
        });

        client.open();

        await new Promise((r) => setTimeout(r, 150));

        // Make sure we were allowed to make room
        expect(create).toBe(true);

        // Use callback to get new room
        client.emit('joinRoom', 'create', 'default', newRoom => createdRoom = newRoom);

        await new Promise((r) => setTimeout(r, 150));
        expect(createdRoom).not.toBe(null);
    });

    test('should be able to create room', async () => {
        let list = null;
        let create = null;
        let createdRoom = null;

        client.on('availableRooms', (data) => {
            list = data.availableRooms;
            create = data.canCreate;
        });

        client.open();

        await new Promise((r) => setTimeout(r, 150));

        // Make sure we were allowed to make room
        expect(create).toBe(true);

        // Use callback to get new room
        client.emit('joinRoom', 'create', 'default', newRoom => createdRoom = newRoom);

        await new Promise((r) => setTimeout(r, 150));
        expect(createdRoom).not.toBe(null);
        expect(createdRoom).not.toBe(false);
    });

    test('should be able to join existing room', async () => {
        let list = null;
        let joinedRoom = null;

        client.on('availableRooms', (data) => {
            list = data.availableRooms;
        });

        client.open();

        await new Promise((r) => setTimeout(r, 150));

        // Make sure list is valid
        expect(list).not.toBe(null);
        expect(list).not.toHaveLength(0);

        // Use callback to get new room
        client.emit('joinRoom', list[0], '', newRoom => joinedRoom = newRoom);

        await new Promise((r) => setTimeout(r, 150));
        expect(joinedRoom).not.toBe(null);
        expect(joinedRoom).toMatch(list[0]);
    });

    test('should not be able to join non-existent room', async () => {
        let create = null;
        let createdRoom = null;

        client.on('availableRooms', (data) => {
            create = data.canCreate;
        });

        client.open();

        await new Promise((r) => setTimeout(r, 150));

        // Make sure we were allowed to make room
        expect(create).toBe(true);

        // Use callback to get new room
        client.emit('joinRoom', 'badenvdoesnotexist', '', newRoom => createdRoom = newRoom);

        await new Promise((r) => setTimeout(r, 150));
        expect(createdRoom).not.toBe(null);
        expect(createdRoom).toBe(false);
    });

    afterAll(() => {
        // Cleanup
        io.close();
    });
    
});