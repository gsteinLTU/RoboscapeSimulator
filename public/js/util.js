/**
 * Replace HTML characters with entities for safe display
 * Thanks to Ben Vinegar
 * @param {string} str String to be sanitized
 * @returns {string} str with risky characters replaced
 */
function escapeHtml(str) {
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;')
        .replace(/\//g, '&#x2F;');
}

function interpolate(x1, x2, dx1, dx2, t) {

    let a = dx2 - dx1;

    dx1 = x2 - x1;

    return x1 + t * dx1 + a / 2.0 * t * t; 
}