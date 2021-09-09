(async () => {
    require('dotenv').config();
    const express = require('express');
    const helmet = require('helmet');
    const geckos = (await import('@geckos.io/server')).geckos;
    const path = require('path');
    const debug = require('debug')('roboscape-sim:index');

    const socketMain = require('./src/socketMain.js');

    const PORT = process.env.PORT || 8000;

    // Create Express server
    const app = express();
    app.use(helmet());
    app.use(express.static(path.join(__dirname, 'public')));
    app.listen(PORT, () => debug(`Listening on port ${PORT}!`));

    socketMain(geckos({
        portRange: {
            min: 50000,
            max: 50250
        }
    }));
})();