const readline = require('readline');
const fs = require('fs');
const reader = fs.createReadStream(null, {fd: Number.parseInt(process.argv[2])});
const writer = fs.createWriteStream(null, {fd: Number.parseInt(process.argv[3])});
const { hrtime } = require('process');

const startTime = hrtime.bigint();
reader.on('data', data => {
    // Data from other process
});

const httpServer = require("http").createServer();
const io = require("socket.io")(httpServer, {
    cors: {
        origin: "*",
        credentials: false
    }
});

io.on("connection", (socket) => {
    // send socket connected message
    writer.write("1" + socket.id + "\r\n");

    socket.onAny((event, ...args) => {
        writer.write("0" + socket.id + event + " " + JSON.stringify(args) + "\r\n");
    });
});

httpServer.listen(9001);