const _ = require('lodash');
const Matter = require('matter-js');
const Bodies = Matter.Bodies;
const World = Matter.World;

const Robot = require('./Robot');

/**
 * Represents a Parallax ActivityBot robot
 */
class ParallaxRobot extends Robot {
    constructor(mac = null, position = null, engine = null, settings = {}) {
        // Allow overriding sprite setting
        if (settings.image == undefined) {
            settings.image = 'parallax_robot';
        }

        settings = _.defaults(settings, Robot.defaultSettings);

        super(mac, position, engine, settings);
    }
}

module.exports = ParallaxRobot;
