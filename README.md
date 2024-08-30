# PKHeX Everywhere

This repository offers 2 different ways of accessing PKHex features in any operating system: a web-based version and a terminal-based version compiled to all major operating systems (macOS, Linux and Windows).

## PKHeX.Web
The PKHeX Web version ([pkhex-web.github.io](https://pkhex-web.github.io)) provides a user-friendly interface accessible via any web browser.

![](./docs/pkhex-web-demo.gif)

This version allows you to manage your party, pokemon box, items, and custom actions through plug-ins, like auto-legality mode, helpers for nuzlocking or live running your save file in a browser-based emulator fully integrated with the app. To learn more check the [wiki](https://github.com/arleypadua/PKHeX.Everywhere/wiki).

## PKHeX.CLI
The PKHeX Command Line Interface (CLI) version allows users to interact with PKHeX via the terminal. It provides a streamlined way to use PKHeX features directly from the command line.

![](./docs/pkhex-cli-demo.gif)

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
3. For more information, refer to the documentation [here](./src/PKHeX.CLI).
