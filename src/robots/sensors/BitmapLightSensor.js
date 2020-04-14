/**
 * A sensor getting a value for the color of the floor under the robot, based on a bitmap image
 */
class BitmapLightSensor {

    /**
     * Adds light sensor to robot
     * @param {Robot} robot 
     */
    static addTo(robot) {
        robot.commandHandlers['l'] = BitmapLightSensor._sendValue.bind(robot);
    }


    /**
     * Sends sensor value to server
     */
    static _sendValue() {
        // Create Light Value message
        let temp = Buffer.alloc(2);
        temp.write('l');

        // Return result with noise
        let idx = this.room.backgroundImage.width * Math.min(Math.max(this.mainBody.position.y, 0), this.room.backgroundImage.width);
        idx += Math.min(Math.max(this.mainBody.position.x, 0), this.room.backgroundImage.width);
        idx = idx << 2;

        let value = this.room.backgroundImage.data[idx];
        temp.writeUInt8(value, 1);
        this.sendToServer(temp);
    }
}

module.exports = BitmapLightSensor;