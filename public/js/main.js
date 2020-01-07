let wHeight = $(window).height();
let wWidth = $(window).width();
let canvas = document.querySelector('#mainCanvas');
let context = canvas.getContext('2d');
canvas.width = wWidth;
canvas.height = wHeight;

let socket = io.connect('http://localhost:8000');
var bodies = [];

socket.on('update', data => {
    bodies = { ...bodies, ...data };
});

socket.on('fullUpdate', data => {
    bodies = data;
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

    for (let body of Object.values(bodies)) {
        context.fillStyle = '#222222';
        //context.fillStyle = orb.color;

        let { x, y } = body.pos;
        context.translate(x, y);
        context.rotate(body.angle);
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
