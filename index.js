require('dotenv').config();
const express = require('express');
const helmet = require('helmet');
const socketio = require('socket.io');
const path = require('path');
const debug = require('debug')('roboscape-sim:index');

const socketMain = require('./src/socketMain.js');

const PORT = process.env.PORT || 8000;

// Create Express server
const app = express();
app.use(helmet());
app.use(express.static(path.join(__dirname, 'public')));
const expressServer = app.listen(PORT, () => debug(`Listening on port ${PORT}!`));

// Start socket.io server
const io = socketio(expressServer);
socketMain(io);
