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

module.exports = { generateRandomMAC };