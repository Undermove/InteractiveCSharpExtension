#!/bin/bash

# Build script for the C# Script Runner Rider Extension

# Set the project directory
PROJECT_DIR="src/InteractiveExtension"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Building C# Script Runner Rider Extension...${NC}"

# Make the script executable
chmod +x "$0"

# Restore packages
echo -e "${YELLOW}Restoring packages...${NC}"
dotnet restore "$PROJECT_DIR" || { echo -e "${RED}Failed to restore packages${NC}"; exit 1; }

# Create NuGet package
echo -e "${YELLOW}Creating NuGet package...${NC}"
dotnet pack "$PROJECT_DIR" -c Release || { echo -e "${RED}Failed to create NuGet package${NC}"; exit 1; }

# Create output directory if it doesn't exist
mkdir -p "output"

# Copy the NuGet package to the output directory
echo -e "${YELLOW}Copying package to output directory...${NC}"
cp "$PROJECT_DIR/bin/Release/"*.nupkg "output/"

# Create a zip file for manual installation
echo -e "${YELLOW}Creating zip file for manual installation...${NC}"
PLUGIN_NAME="CSharpScriptRunner"
PLUGIN_VERSION="1.0.0"
ZIP_DIR="output/${PLUGIN_NAME}-${PLUGIN_VERSION}"
mkdir -p "$ZIP_DIR"

# Copy the necessary files to the zip directory
cp -r "$PROJECT_DIR/bin/Release/net8.0/"* "$ZIP_DIR/"
cp "$PROJECT_DIR/plugin.xml" "$ZIP_DIR/"

# Create the zip file
cd output
zip -r "${PLUGIN_NAME}-${PLUGIN_VERSION}.zip" "${PLUGIN_NAME}-${PLUGIN_VERSION}"
cd ..

echo -e "${GREEN}Build completed successfully!${NC}"
echo -e "${YELLOW}The extension files are available in the 'output' directory:${NC}"
echo -e "${YELLOW}1. NuGet package: output/*.nupkg${NC}"
echo -e "${YELLOW}2. Zip file: output/${PLUGIN_NAME}-${PLUGIN_VERSION}.zip${NC}"
echo -e "${YELLOW}To install the extension in Rider 2025:${NC}"
echo -e "${YELLOW}1. Go to Settings > Plugins > Install Plugin from Disk${NC}"
echo -e "${YELLOW}2. Select either the .nupkg file or the .zip file${NC}"