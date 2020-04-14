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
        let testPoint = {
            x: this.mainBody.position.x + offsetVec.x,
            y: this.mainBody.position.y + offsetVec.y
        };

        let value = BitmapLightSensor._getValueFromImage(testPoint, this.room.backgroundImage);
        
        // Return result
        temp.writeUInt8(value, 1);
        this.sendToServer(temp);
    }

    /**
     * Get value at a given position in an image
     * @param {Object} testPoint 
     * @param {PNG} image 
     * @param {Number=} defaultValue Value to use if testPoint is outside image bounds
     */
    static _getValueFromImage(testPoint, image, defaultValue = 255) {
        // If point is invalid, return default value
        let value = defaultValue;
        
        // Make sure point is on image
        if (testPoint.x >= 0 && testPoint.x < image.width && testPoint.y >= 0 && testPoint.y < image.height) {
            let idx = image.width * Math.round(testPoint.y);
            idx += Math.round(testPoint.x);
            idx = idx << 2;
            value = image.data[idx];
        }

        return value;
    }
}

module.exports = BitmapLightSensor;