# PKHeX CLI

This is a Command Line Interface (CLI) tool for interacting with Pokémon save files. It provides various functionalities to manage and modify Pokémon save data in a convenient and scriptable manner.

## Features

- **Load Save File**: Load a Pokémon save file from a specified path.
- **View Trainer Info**: View information about the trainer in the save file.
- **View/Edit Inventory**: View and edit the inventory of the trainer.
- **Exit**: Exit the program.

## Building

### Prerequisites

- .NET Core SDK 8.x

### Steps

1. Clone this repository to your local machine.
2. Navigate to the project directory.
3. Compile the program using `dotnet build`.
4. Run the program using `dotnet run -- [options]`.

## Releasing

1. `git tag vX.Y.Z HEAD`
2. `git push origin --tags`
3. If pipeline doesn't run automatically, trigger it via the [actions page](https://github.com/arleypadua/PKHeX.CLI/actions/workflows/dotnet.yml)
   1. select the tag as the target, otherwise the Github Release won't be created

## Usage

1. Download the latest version for your system [here](https://github.com/arleypadua/PKHeX.CLI/releases)
2. Put the file somewhere that's visible from your PATH directory
3. Run `pkhex-cli /path/to/savefile.bin`

### Command Line Options

- **savefile**: (Optional) The path to the save file. Defaults to "./data/savedata.bin".

### Example Usage

```bash
pkhex-cli /path/to/savefile.bin
```

# Credits

This CLI tool utilizes the following libraries:

* [PKHeX.Core](https://github.com/kwsch/PKHeX/tree/master) for interacting with Pokémon save files.
* [Spectre.Console](https://github.com/spectreconsole/spectre.console) for enhancing the command line interface.