#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Make the script executable
chmod +x "$0"

# Check if Rider is installed
RIDER_APP="/Applications/Rider.app"
if [ ! -d "$RIDER_APP" ]; then
    echo -e "${RED}Rider not found at $RIDER_APP${NC}"
    exit 1
fi

# Find the plugins directory
PLUGINS_DIR="$HOME/Library/Application Support/JetBrains/Rider2025.1/plugins"
if [ ! -d "$PLUGINS_DIR" ]; then
    echo -e "${YELLOW}Creating plugins directory: $PLUGINS_DIR${NC}"
    mkdir -p "$PLUGINS_DIR"
fi

# Create the plugin directory
PLUGIN_DIR="$PLUGINS_DIR/CSharpScriptRunner"
if [ -d "$PLUGIN_DIR" ]; then
    echo -e "${YELLOW}Removing existing plugin installation...${NC}"
    rm -rf "$PLUGIN_DIR"
fi

echo -e "${YELLOW}Creating plugin directory: $PLUGIN_DIR${NC}"
mkdir -p "$PLUGIN_DIR/lib"

# Copy the plugin files
echo -e "${YELLOW}Copying plugin files...${NC}"
cp -r output/CSharpScriptRunner/* "$PLUGIN_DIR/"

echo -e "${GREEN}Plugin installed successfully!${NC}"
echo -e "${YELLOW}Please restart Rider to activate the plugin.${NC}"