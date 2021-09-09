const fs = require('fs');
const path = require('path');

const PNG = require('pngjs').PNG;
const Matter = require('matter-js');
const Engine = Matter.Engine;

const Robot = require('../Robot');
const BitmapLightSensor = require('./BitmapLightSensor');

describe('BitmapLightSensor tests', () => {
    let engine;
    let testRobot;
    let dummyRoom = {};

    beforeAll(() => {
        engine = engine = Engine.create();
        testRobot = new Robot(null, null, engine);
        testRobot.room = dummyRoom;
        BitmapLightSensor.addTo(testRobot);
        var data = fs.readFileSync(path.join(__dirname, '..', '..', '..', 'public', 'img', 'backgrounds', 'lineloop.png'));
        var image = PNG.sync.read(data);
        dummyRoom.backgroundImage = image;
    });

    test('Added expected command handler', () => {
        expect(testRobot.commandHandlers).toMatchObject(expect.objectContaining({
            'l': expect.any(Function)
        }));
    });

    test('_getValueFromImage returns default outside image', () => {
        expect(BitmapLightSensor._getValueFromImage({ x: -1, y: -1000 }, dummyRoom.backgroundImage, 500)).toEqual(500);
        expect(BitmapLightSensor._getValueFromImage({ x: dummyRoom.backgroundImage.width + 1, y: 0 }, dummyRoom.backgroundImage, 500)).toEqual(500);
        expect(BitmapLightSensor._getValueFromImage({ x: 0, y: dummyRoom.backgroundImage.height + 1 }, dummyRoom.backgroundImage, 500)).toEqual(500);
    });

    test('_getValueFromImage returns correct value inside image', () => {
        expect(BitmapLightSensor._getValueFromImage({ x: 500, y: 500 }, dummyRoom.backgroundImage, 500)).toEqual(255);
    });
});
