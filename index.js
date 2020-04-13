require('dotenv').config();
const express = require('express');
const helmet = require('helmet');
const socketio = require('socket.io');
const path = require('path');
const debug = require('debug')('roboscape-sim:index');

const socketMain = require('./src/socketMain.js');

const port = process.env.PORT || 8000;
process.env.sensors = process.env.sensors || 'parallax';

// Create Express server
const app = express();
app.use(helmet());
app.use(express.static(path.join(__dirname, 'public')));
const expressServer = app.listen(port, () => debug(`Listening on port ${port}!`));

// Start socket.io server
const io = socketio(expressServer);
socketMain(io);
