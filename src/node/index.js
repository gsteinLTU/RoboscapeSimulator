const readline = require('readline');
const fs = require('fs');
const reader = fs.createReadStream(null, {fd: Number.parseInt(process.argv[2])});
const writer = fs.createWriteStream(null, {fd: Number.parseInt(process.argv[3])});
const { hrtime } = require('process');

const startTime = hrtime.bigint();
reader.on('data', data => {
    
});

setInterval(()=> {}, 1000 * 60 * 60);

const httpServer = require("http").createServer();
const io = require("socket.io")(httpServer, {
  
});

io.on("connection", (socket) => {
    
});

httpServer.listen(9001);