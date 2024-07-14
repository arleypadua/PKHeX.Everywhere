#!/bin/bash

# Variables
GITHUB_REPO="arleypadua/PKHeX.Everywhere"
INSTALL_DIR="$HOME/.local/bin/pkhex-cli"
EXECUTABLE_NAME="pkhex-cli"

# Determine the platform and architecture
OS=$(uname | tr '[:upper:]' '[:lower:]')
ARCH=$(uname -m)

if [[ "$OS" == "linux" && "$ARCH" == "x86_64" ]]; then
    ZIP_NAME="pkhex-cli-linux-x64.zip"
elif [[ "$OS" == "darwin" && "$ARCH" == "x86_64" ]]; then
    ZIP_NAME="pkhex-cli-osx-x64.zip"
elif [[ "$OS" == "darwin" && "$ARCH" == "arm64" ]]; then
    ZIP_NAME="pkhex-cli-osx-arm64.zip"
else
    echo "Unsupported platform or architecture: $OS $ARCH"
    exit 1
fi

# Fetch the latest release URL
GITHUB_URL=$(curl -s https://api.github.com/repos/$GITHUB_REPO/releases/latest | grep "browser_download_url.*$ZIP_NAME" | cut -d '"' -f 4)

# Check if the URL was fetched successfully
if [ -z "$GITHUB_URL" ]; then
    echo "Error: Could not fetch the latest release URL. Please check your GitHub repository and artifact name."
    exit 1
fi

# Download the zip file
echo "Downloading $ZIP_NAME from $GITHUB_URL..."
curl -L -o $ZIP_NAME $GITHUB_URL &> /dev/null

# Create the install directory if it doesn't exist
if [ ! -d "$INSTALL_DIR" ]; then
    echo "Creating install directory $INSTALL_DIR..."
    mkdir -p $INSTALL_DIR &> /dev/null
fi

# Unzip the file
echo "Unzipping $ZIP_NAME..."
unzip -o $ZIP_NAME -d $INSTALL_DIR &> /dev/null

# Make the executable part of the PATH
echo "Making $EXECUTABLE_NAME executable..."
chmod +x $INSTALL_DIR/$EXECUTABLE_NAME &> /dev/null

# Add to PATH if not already in PATH
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
    echo "Adding $INSTALL_DIR to PATH..."
    export PATH=$PATH:$INSTALL_DIR
    echo 'export PATH=$PATH:'$INSTALL_DIR >> ~/.bashrc
fi

# Clean up
echo "Cleaning up..."
rm $ZIP_NAME &> /dev/null

echo "Installation complete. You can now use $EXECUTABLE_NAME from anywhere."
