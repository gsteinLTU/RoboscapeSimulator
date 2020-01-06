require('dotenv').config();
const express = require('express');
const helmet = require('helmet');
const socketio = require('socket.io');
const path = require('path');

const socketMain = require('./socketMain.js');

const port = process.env.PORT | 8000;

// Create Express server
const app = express();
app.use(helmet());
app.use(express.static(path.join(__dirname, 'public')));
const expressServer = app.listen(port, () => console.log(`Listening on port ${port}!`));

// Start socket.io server
const io = socketio(expressServer);
socketMain(io);
