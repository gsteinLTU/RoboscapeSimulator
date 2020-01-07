let wHeight = $(window).height();
let wWidth = $(window).width();
let canvas = document.querySelector('#mainCanvas');
let context = canvas.getContext('2d');
canvas.width = wWidth;
canvas.height = wHeight;

let socket = io.connect('http://localhost:8000');
let bodies = {};
let nextBodies = {};
let lastUpdateTime = Date.now();
let nextUpdateTime = Date.now();

socket.on('update', data => {
    bodies = { ...nextBodies };
    nextBodies = { ...bodies, ...data };
    lastUpdateTime = nextUpdateTime;
    nextUpdateTime = Date.now();
});

socket.on('fullUpdate', data => {
    bodies = data;
    nextBodies = data;
    lastUpdateTime = Date.now();
    nextUpdateTime = Date.now();
});

function reset() {
    socket.emit('reset', true);
}

function draw() {
    context.setTransform(1, 0, 0, 1, 0, 0);

    // let camX = -player.locX + canvas.width / 2;
    // let camY = -player.locY + canvas.height / 2;
    // context.translate(camX, camY);

    context.clearRect(-wWidth, -wHeight, wWidth * 2, wHeight * 2);

    let frameTime = Date.now();

    for (let label of Object.keys(bodies)) {
        let body = bodies[label];
        context.fillStyle = '#222222';
        //context.fillStyle = orb.color;

        let { x, y } = body.pos;
        let angle = body.angle;
        x += ((nextBodies[label].pos.x - x) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        y += ((nextBodies[label].pos.y - y) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        angle += ((nextBodies[label].angle - angle) * (frameTime - lastUpdateTime)) / Math.max(1, nextUpdateTime - lastUpdateTime);
        context.translate(x, y);
        context.rotate(angle);
        context.fillRect(-body.width / 2, -body.height / 2, body.width, body.height);
        context.rotate(-body.angle);
        context.translate(-x, -y);
    }

    requestAnimationFrame(draw);
}

window.addEventListener('resize', e => {
    wHeight = $(window).height();
    wWidth = $(window).width();
    canvas.width = wWidth;
    canvas.height = wHeight;
});

draw();
