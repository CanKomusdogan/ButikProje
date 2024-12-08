const fs = require('fs');
const path = require('path');
const JavaScriptObfuscator = require('javascript-obfuscator');

// Input directories (Array of directories)
const inputDirs = ['./CompiledTS', './src']; // Add more directories as needed
const outputDir = './dist'; // Output directory

// Ensure the output directory exists
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

// Function to recursively read and obfuscate files
function obfuscateFilesInDir(dir, baseDir) {
    fs.readdir(dir, (err, files) => {
        if (err) {
            console.error('Error reading directory:', err);
            return;
        }

        files.forEach(file => {
            const fullPath = path.join(dir, file);
            const stat = fs.statSync(fullPath);

            if (stat.isDirectory()) {
                // Recurse into subdirectories
                obfuscateFilesInDir(fullPath, baseDir);
            } else if (path.extname(file) === '.js') {
                // Process only JavaScript files
                const fileContents = fs.readFileSync(fullPath, 'utf8');
                console.log(`Obfuscating: ${fullPath}`);

                // Obfuscate the JavaScript code
                const obfuscatedCode = JavaScriptObfuscator.obfuscate(fileContents).getObfuscatedCode();

                // Set the output path (preserve directory structure relative to the base directory)
                const relativePath = path.relative(baseDir, fullPath);
                const outputFilePath = path.join(outputDir, relativePath);
                const outputDirPath = path.dirname(outputFilePath);

                // Create necessary directories in the output path
                if (!fs.existsSync(outputDirPath)) {
                    fs.mkdirSync(outputDirPath, { recursive: true });
                }

                // Write the obfuscated code to the output directory
                fs.writeFileSync(outputFilePath, obfuscatedCode, 'utf8');
                console.log(`Obfuscated and saved to: ${outputFilePath}`);
            }
        });
    });
}

// Loop through each input directory
inputDirs.forEach(inputDir => {
    const absoluteBaseDir = path.resolve(inputDir);
    obfuscateFilesInDir(inputDir, absoluteBaseDir);
});
