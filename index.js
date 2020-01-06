require('dotenv').config();
const express = require('express');
const helmet = require('helmet');
const socketio = require('socket.io');
const os = require('os');
const path = require('path');

const socketMain = require('./socketMain.js');

const app = express();
app.use(helmet());
app.use(express.static(path.join(__dirname, 'public')));

const port = process.env.PORT | 8000;
const expressServer = app.listen(port, () => console.log(`Listening on port ${port}!`));
const io = socketio(expressServer);
socketMain(io);
