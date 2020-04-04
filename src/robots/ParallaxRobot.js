const _ = require('lodash');
const Matter = require('matter-js');
const Vector = Matter.Vector,
    World = Matter.World;

const Robot = require('./Robot');
const WhiskersSensor = require('./sensors/WhiskersSensor');
const RangeSensor = require('./sensors/RangeSensor');

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
        RangeSensor.addTo(this);

        // Setup LEDs
        this.body.ledStatus = [0, 0];
        this.commandHandlers['L'] = this.updateLEDs.bind(this);

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
     * Handle an incoming "set LED" message
     * @param {Buffer} msg Message from server to this robot
     */
    updateLEDs(msg)
    {
        // Decompose message into parts
        let led = msg.readUInt8(1);
        let command = msg.readUInt8(2);

        // Tell client LED changed
        if (led < this.body.ledStatus.length) {
            this.body.ledStatus[led] = command;
        }
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
