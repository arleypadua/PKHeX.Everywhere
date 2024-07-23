# PKHeX Everywhere

This repository offers 2 different ways of accessing PKHex features in any operating system: a web-based version and a terminal-based version compiled to all major operating systems (macOS, Linux and Windows).

## PKHeX.Web
The PKHeX Web version provides a user-friendly interface accessible via any web browser. This version supports plug-ins and includes built-in auto legality mode for ensuring the legality of Pokémon data.

### Access:
Visit [PKHeX Web](https://pkhex-web.github.io) to start using the web interface.

## PKHeX.CLI
The PKHeX Command Line Interface (CLI) version allows users to interact with PKHeX via the terminal. It provides a streamlined way to use PKHeX features directly from the command line.

### Installation Methods:
1. **Curl Script**:
   ```sh
   curl -sL https://raw.githubusercontent.com/arleypadua/PKHeX.Everywhere/main/install.sh | sudo bash
   ```
2. **Homebrew**:
   ```sh
   brew tap arleypadua/homebrew-pkhex-cli
   brew install pkhex-cli
   ```
3. For more information, refer to the documentation at `./src/PKHeX.CLI`.
