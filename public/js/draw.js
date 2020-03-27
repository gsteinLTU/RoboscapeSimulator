/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */

/**
 * Begin updating the canvas
 */
function draw() {
    // Reset canvas
    context.setTransform(1, 0, 0, 1, 0, 0);
    context.clearRect(-wWidth, -wHeight, wWidth * 2, wHeight * 2);
    context.scale(cameraZoom, cameraZoom);
    context.translate(cameraPos.x, cameraPos.y);
    let frameTime = Date.now();
    for (let label of Object.keys(bodies)) {
        let body = bodies[label];
        context.fillStyle = '#222222';
        let { x, y } = body.pos;
        let { height, width, image } = bodiesInfo[label];
        let angle = body.angle;
        // Extrapolate/Interpolate position and rotation
        x += ((nextBodies[label].pos.x - x) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        y += ((nextBodies[label].pos.y - y) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        angle += ((nextBodies[label].angle - angle) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        if (images[image] !== undefined) {
            let imageData = images[image];
            // Transform
            context.translate(x, y);
            context.rotate(angle + imageData.offsetAngle);
            // Draw sprite
            context.drawImage(imageData, -width / 2 + width * imageData.offset.left, -height / 2 + width * imageData.offset.top, width + width * imageData.offset.right, height + width * imageData.offset.bottom);
            // Draw LEDs (if available)
            if (imageData.ledPositions !== undefined) {
                for (let i in imageData.ledPositions) {
                    let pos = imageData.ledPositions[i];
                    if (nextBodies[label].ledStatus[i] != 0) {
                        context.fillStyle = 'orange';
                        context.beginPath();
                        context.arc(pos.x, pos.y, 5, 0, 2 * Math.PI);
                        context.fill();
                    }
                }
            }
            // Undo transform
            context.rotate(-angle - imageData.offsetAngle);
            context.translate(-x, -y);
        }
        else {
            // Transform
            context.translate(x, y);
            context.rotate(angle);
            // Default to rectangle
            context.fillRect(-width / 2, -height / 2, width, height);
            // Undo transform
            context.rotate(-angle);
            context.translate(-x, -y);
        }
    }

    if(running){
        requestAnimationFrame(draw);
    }
}
