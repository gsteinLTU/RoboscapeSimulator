const _ = require('lodash');

const Robot = require('./Robot');

/**
 * Represents a Parallax ActivityBot robot
 */
class ParallaxRobot extends Robot {
    constructor(mac = null, position = null, settings = {}) {
        // Allow overriding sprite setting
        if (settings.image == undefined) {
            settings.image = 'parallax_robot';
        }

        settings = _.defaults(settings, Robot.defaultSettings);

        super(mac, position, settings);
    }
}

module.exports = ParallaxRobot;
