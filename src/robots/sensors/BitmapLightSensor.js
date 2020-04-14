const Matter = require('matter-js');
const Vector = Matter.Vector;

/**
 * A sensor getting a value for the color of the floor under the robot, based on a bitmap image
 */
class BitmapLightSensor {

    /**
     * Adds light sensor to robot
     * @param {Robot} robot 
     * @param {Object} offset
     */
    static addTo(robot, offset = {x: 0, y: 0}) {
        robot.commandHandlers['l'] = BitmapLightSensor._sendValue.bind(robot);
        robot.lightSensorOffset = offset;
    }

    /**
     * Sends sensor value to server
     */
    static _sendValue() {
        // Create Light Value message
        let temp = Buffer.alloc(2);
        temp.write('l');

        // Find position with offset
        let offsetVec = Vector.rotate(Vector.create(this.lightSensorOffset.x, this.lightSensorOffset.y), this.body.angle);

        // Determine if position is outside of 
        let testPoint = {
            x: this.mainBody.position.x + offsetVec.x,
            y: this.mainBody.position.y + offsetVec.y
        };

        // If point is invalid, return default value
        let value = 255;
        
        // Make sure point is on image
        if (testPoint.x >= 0 && testPoint.x < this.room.backgroundImage.width && testPoint.y >= 0 && testPoint.y < this.room.backgroundImage.height){
            let idx = this.room.backgroundImage.width * Math.round(testPoint.y);
            idx += Math.round(testPoint.x);
            idx = idx << 2;
            value = this.room.backgroundImage.data[idx];
        }
        
        // Return result
        temp.writeUInt8(value, 1);
        this.sendToServer(temp);
    }
}

module.exports = BitmapLightSensor;