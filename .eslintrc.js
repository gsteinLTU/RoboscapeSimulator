module.exports = {
    env: {
        browser: true,
        node: true,
        es2020: true,
        jest: true
    },
    parserOptions: {  
        sourceType: 'module',
    },
    extends: 'eslint:recommended',
    rules: {
        // enable additional rules
        indent: ['error', 4],
        quotes: ['error', 'single'],
        semi: ['error', 'always'],
        'space-infix-ops': [
            'error',
            {
                int32Hint: false
            }
        ],
        // override default options for rules from base configurations
        'comma-dangle': ['error', 'only-multiline'],
        'no-cond-assign': ['error', 'always'],
        // disable rules from base configurations
        'no-console': 'off'
    }
};
