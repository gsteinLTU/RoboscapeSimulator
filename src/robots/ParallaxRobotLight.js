const ParallaxRobot = require('./ParallaxRobot');

/**
 * Class to create ParallaxRobot with Lidar sensor
 */
class ParallaxRobotLight extends ParallaxRobot {
    constructor(mac = null, position = null, engine = null, settings = {}) {

        if(settings.extraSensors == undefined){
            settings.extraSensors = ['light'];
        }

        super(mac, position, engine, settings);
    }
}

module.exports = ParallaxRobotLight;