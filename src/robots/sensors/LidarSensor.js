const Matter = require('matter-js');
const Vector = Matter.Vector;

const RangeSensor = require('./RangeSensor');

/**
 * A sensor detecting distance to objects in multiple lines extending from the front of the robot
 */
class LidarSensor extends RangeSensor {

    /**
     * Adds Range sensor to robot
     * @param {Robot} robot  
     * @param {Number=} minangle
     * @param {Number=} maxangle
     * @param {Number=} rays
     */
    static addTo(robot, minangle = -Math.PI / 2, maxangle = Math.PI / 2, rays = 19) {
        robot.commandHandlers['R'] = LidarSensor._sendRange.bind(robot);
        robot.lidarSettings = {
            minangle: minangle,
            maxangle: maxangle,
            rays: rays
        };
    }

    /**
     * Sends range sensor values to server
     */
    static _sendRange() {
        // Create Range message with room for all sensor data
        let temp = Buffer.alloc(1 + 2 * this.lidarSettings.rays);
        temp.write('R');

        // Difference between rays
        let angleDiff = (this.lidarSettings.maxangle - this.lidarSettings.minangle) / (this.lidarSettings.rays - 1);

        for (let i = 0; i < this.lidarSettings.rays; i++) {
            // Create vector in angle
            const direction = Vector.rotate(Vector.create(0, 1), this.body.angle + this.lidarSettings.minangle + i * angleDiff);
            
            const rayStart = Vector.add(this.body.position, Vector.mult(direction, 1));
            const rayEnd = Vector.add(this.body.position, Vector.mult(direction, 325));
            
            let distance = RangeSensor._hitTest.call(this, rayStart, rayEnd);
            
            // Return result with noise
            temp.writeInt16LE(distance * (Math.random() / 10 + 0.95), 1 + 2 * i);
        }

        this.sendToServer(temp);
    }
        
}

module.exports = LidarSensor;