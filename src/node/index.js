const readline = require('readline');
const fs = require('fs');
const process = require('process');
const isRunning = require('is-running');

const reader = fs.createReadStream(null, {fd: Number.parseInt(process.argv[2])});
const writer = fs.createWriteStream(null, {fd: Number.parseInt(process.argv[3])});

const lineReader = readline.createInterface({
    input: reader,
    crlfDelay: Infinity
});

const ppid = process.ppid;

const sockets = {};

// Handle input from other process
lineReader.on('line', data => {
    if (data[0] == '0') {
        // Message to be sent
        let destSocket = data.substring(1, 21);
        let message = data.substring(21);

        let eventName = message.substring(0, message.indexOf(' '));

        try {
            message = JSON.parse(message.substring(eventName.length + 1));
            
            //console.log("Message for " + destSocket + ": type: " + eventName + " data: " + message);
            if (Object.keys(sockets).includes(destSocket)) {
                sockets[destSocket].emit(eventName, message);
            }
                        
        } catch (error) {
            console.error(error);
        }
    }
});

// Socket.IO setup
const httpServer = require("http").createServer();
const io = require("socket.io")(httpServer, {
    cors: {
        origin: "*",
        credentials: false
    }
});

io.on("connection", (socket) => {
    sockets[socket.id] = socket;

    // Send socket connected message
    writer.write("1" + socket.id + "\r\n");

    // Forward messages to server program
    socket.onAny((event, ...args) => {
        writer.write("0" + socket.id + event + " " + JSON.stringify(args) + "\r\n");
    });
});

httpServer.listen(9001);

console.debug('Node server started');

// Detect main process crash
setInterval(() => {
    if(!isRunning(ppid) || process.ppid != ppid){
        process.exit();
    }
}, 1000);