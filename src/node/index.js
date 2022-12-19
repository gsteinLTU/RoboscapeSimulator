const readline = require('readline');
const fs = require('fs');
const process = require('process');
const isRunning = require('is-running');

const reader = fs.createReadStream(null, { fd: Number.parseInt(process.argv[2]) });
const writer = fs.createWriteStream(null, { fd: Number.parseInt(process.argv[3]) });
const ppid = process.ppid;

const lineReader = readline.createInterface({
    input: reader,
    crlfDelay: Infinity
});

const PORT = 9001;

const sockets = {};

// Handle input from other process
lineReader.on('line', data => {
    if (data[0] == '0') {
        // Message to be sent
        let destSocket = data.substring(1, 21);
        let message = data.substring(21);

        let eventName = message.substring(0, message.indexOf(' '));

        try {
            if (message.length > eventName.length + 1) {
                message = JSON.parse(message.substring(eventName.length + 1));
            } else {
                message = "";
            }

            //console.log('Message for ' + destSocket + ': type: ' + eventName + ' data: ' + message);
            if (Object.keys(sockets).includes(destSocket)) {
                sockets[destSocket].emit(eventName, message);
            }

        } catch (error) {
            console.error(message);
            console.error(error);
        }
    }
});

// Socket.IO setup
const { Server } = require('socket.io');
const io = new Server({
    cors: {
        origin: '*',
        credentials: false
    }
});

io.on('connection', (socket) => {
    console.debug('Socket ' + socket.id + ' connected');

    sockets[socket.id] = socket;

    // Send socket connected message
    writer.write('1' + socket.id + '\r\n');

    // Forward messages to server program
    socket.onAny((event, ...args) => {
        writer.write('0' + socket.id + event + ' ' + JSON.stringify(args) + '\r\n');
    });

    socket.on('disconnect', () => {
        writer.write('2' + socket.id + '\r\n');
    });
});


io.listen(PORT, () => {
    console.debug('Node server listening on ' + PORT);
});

console.debug('Node server started');

// Detect main process crash
setInterval(() => {
    if (!isRunning(ppid) || process.ppid != ppid) {
        process.exit();
    }
}, 1000);