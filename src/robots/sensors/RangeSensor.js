const Matter = require('matter-js');
const Vector = Matter.Vector;

/**
 * A sensor detecting distance to objects in a line extending from the front of the robot
 */
class RangeSensor {

    /**
     * Adds Whiskers sensors to robot
     * @param {Robot} robot 
     */
    static addTo(robot) {
        // Setup range sensor
        robot.commandHandlers['R'] = RangeSensor._sendRange.bind(robot);
    }


    /**
     * Sends range sensor value to server
     */
    static _sendRange() {
        // Create Range message
        let temp = Buffer.alloc(3);
        temp.write('R');

        const upVec = Vector.rotate(Vector.create(0, 1), this.body.angle);

        const rayStart = Vector.add(this.body.position, Vector.mult(upVec, 1));
        const rayEnd = Vector.add(this.body.position, Vector.mult(upVec, 325));

        // Find objects in sensor path
        let query = Matter.Query.ray(
            this.engine.world.bodies.filter(b => !b.label.includes(this.body.label)),
            rayStart,
            rayEnd
        );

        // Find closest object
        query = query.sort(r => Math.pow(r.bodyA.position.x - this.body.position.x, 2) + Math.pow(r.bodyA.position.y - this.body.position.y, 2));
        const maxDist = 300;
        let distance = maxDist;
        if (query !== undefined && query.length > 0) {
            let target = query[0].bodyA;

            let targetVerts = [...target.vertices];
            let targetEdges = [];

            // Get all edges
            for (let i in targetVerts) {
                targetEdges.push({
                    p1x: targetVerts[i].x,
                    p1y: targetVerts[i].y,
                    p2x: targetVerts[(i + 1) % targetVerts.length].x,
                    p2y: targetVerts[(i + 1) % targetVerts.length].y
                });
            }

            // Test for intersections
            let x1 = rayStart.x;
            let x2 = rayStart.x + upVec.x * maxDist;
            let y1 = rayStart.y;
            let y2 = rayStart.y + upVec.y * maxDist;
            for (let edge of targetEdges) {
                let x3 = edge.p1x;
                let y3 = edge.p1y;
                let x4 = edge.p2x;
                let y4 = edge.p2y;

                let t = (x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4);
                t /= (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

                let u = (x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3);
                u /= (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
                u *= -1;

                // No intersection between segments
                if (t > 1 || t < 0 || u > 1 || u < 0) {
                    continue;
                }

                distance = Math.max(4, Math.min(distance, t * maxDist) - this.body.height / 4);
            }
        }

        // Return result with noise
        temp.writeInt16LE(distance * (Math.random() / 10 + 0.95), 1);
        this.sendToServer(temp);
    }

}

module.exports = RangeSensor;