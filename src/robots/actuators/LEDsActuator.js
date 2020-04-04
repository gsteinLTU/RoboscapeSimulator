/**
 * A set of lights attached to the robot
 */
class LEDsActuator {

    /**
     * Adds LEDs to robot
     * @param {Robot} robot 
     * @param {Number} numLights Number of LEDs on robot
     */
    static addTo(robot, numLights = 2) {
        robot.body.ledStatus = Array(numLights).fill(0);
        robot.commandHandlers['L'] = LEDsActuator.updateLEDs.bind(robot);
    }

    /**
     * Handle an incoming "set LED" message
     * @param {Buffer} msg Message from server to this robot
     */
    static updateLEDs(msg) {
        // Decompose message into parts
        let led = msg.readUInt8(1);
        let command = msg.readUInt8(2);

        // Tell client LED changed
        if (led < this.body.ledStatus.length) {
            this.body.ledStatus[led] = command;
        }
    }

}

module.exports = LEDsActuator;