const _ = require('lodash');
const Matter = require('matter-js');
const World = Matter.World;

const Robot = require('./Robot');
const WhiskersSensor = require('./sensors/WhiskersSensor');
const RangeSensor = require('./sensors/RangeSensor');
const LEDsActuator = require('./actuators/LEDsActuator');
const LidarSensor = require('./sensors/LidarSensor');
const LightSensor = require('./sensors/BitmapLightSensor');

/**
 * Represents a Parallax ActivityBot robot
 */
class ParallaxRobot extends Robot {
    constructor(mac = null, position = null, engine = null, settings = {}) {
        // Allow overriding sprite setting
        if (settings.image == undefined) {
            settings.image = 'parallax_robot';
        }

        if(settings.speedToTicks == undefined){
            settings.speedToTicks = (15000 / 60);
        }

        settings = _.defaults(settings, Robot.defaultSettings);

        
        super(mac, position, engine, { ...settings });

        // Add sensors
        WhiskersSensor.addTo(this);
        LEDsActuator.addTo(this, 2);
        
        // Allow replacing single range sensor with a lidar
        if(settings.extraSensors.indexOf('lidar') !== -1){
            LidarSensor.addTo(this);    
        } else {
            RangeSensor.addTo(this);
        }

        // Optional light sensor
        if(settings.extraSensors.indexOf('light' !== -1)){
            LightSensor.addTo(this, {x: 0, y: 20});
        }
        
        // Setup ticks
        this.ticks = {left: 0, right: 0};
        this.commandHandlers['T'] = this.sendTicks.bind(this);

        // Update center of mass position
        this.body.positionPrev.x = this.mainBody.position.x - (this.body.position.x - this.body.positionPrev.x);
        this.body.positionPrev.y = this.mainBody.position.y - (this.body.position.y - this.body.positionPrev.y);
        this.body.position.x = this.mainBody.position.x;
        this.body.position.y = this.mainBody.position.y;

        this.body.width = this.settings.width;
        this.body.height = this.settings.height;
        this.body.image = this.mainBody.image;

        World.add(engine.world, this.body);
    }

    /**
     * Applies force of wheels to robot
     */
    drive() {
        this.ticks.left += this.setSpeed.left * this.settings.speedToTicks;
        this.ticks.right += this.setSpeed.right * this.settings.speedToTicks;
        super.drive();
    }

    /**
     * Send ticks information to server
     */
    sendTicks() {
        let temp = Buffer.alloc(9);
        temp.write('T');

        // Add ticks to message
        temp.writeInt32LE(this.ticks.left,1);
        temp.writeInt32LE(this.ticks.right,5);

        this.sendToServer(temp);
    }
}

module.exports = ParallaxRobot;
