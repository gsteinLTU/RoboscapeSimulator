const util = require('./util');

describe('generateRandomMAC tests', () => {
    test('generateRandomMAC should match the correct format', () => {
        expect(util.generateRandomMAC()).toEqual(expect.stringMatching(/^([0-9a-fA-F]{2}:){5}[0-9a-fA-F]{2}$/));
    });
});
