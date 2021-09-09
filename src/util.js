/**
 * Generates a random hex string resembling a MAC address
 */
function generateRandomMAC() {
    let mac = '';
    for (var i = 0; i < 12; i++) {
        if (i % 2 === 0 && i > 0 && i < 12) {
            mac += ':';
        }
        let hexval = Math.floor(Math.random() * 16);
        mac += hexval.toString(16);
    }
    return mac;
}

/**
 * Test that a robot has handlers for a list of commands
 * @param {Array<String>} commands 
 * @param {Robot} testRobot 
 */
function expectCommandHandlers(commands, testRobot){

    let expectedCommands = {};

    commands.forEach((command) => {
        expectedCommands[command] = expect.any(Function);
    });

    expect(testRobot.commandHandlers).toMatchObject(expect.objectContaining(expectedCommands));
}

module.exports = { generateRandomMAC, expectCommandHandlers };
