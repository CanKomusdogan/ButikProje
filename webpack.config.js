const path = require('path');

module.exports = {
    entry: './src/js/main.js',
    output: {
        path: path.resolve(__dirname, 'dist/js'),  // Output folder for bundled files
        filename: 'bundle.js'  // Name of the bundled output file
    },
    module: {
        rules: [
            {
                test: /\.js$/,  // Rule for transpiling JS files
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader',
                    options: {
                        presets: ['@babel/preset-env']
                    }
                }
            },
            {
                test: /\.css$/,  // Rule for handling CSS files
                use: ['style-loader', 'css-loader']
            }
        ]
    },
    mode: 'development',  // Set to 'production' for production mode
};
