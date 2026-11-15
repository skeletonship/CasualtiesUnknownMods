# Casualties Unknown Mods

## Three Wall Jumps v1.0.0

A BepInEx plugin for **Casualties Unknown 1.0.2** that allows **three consecutive wall jumps from the same side**.

**Right wall ×3 → left wall ×3 → right wall ×3 → left wall ×3**, as often as needed.

Moving into the opposite wall resets the count immediately, even if you do not jump from that wall. Leaving and retouching the same side does not reset it. Landing, climbing, and swimming also reset it as appropriate to the game.

### Download and install

1. Download [ThreeWallJumps.dll](downloads/ThreeWallJumps.dll). On its GitHub file page, click **Download raw file**.
2. Close the game and put the DLL into **`BepInEx\plugins`** in the game folder.
3. Launch the game. `BepInEx\LogOutput.log` should report **Three Wall Jumps 1.0.0 loaded**.

Alternatively, download [the install ZIP](downloads/ThreeWallJumps-v1.0.0.zip) and copy its `BepInEx` folder into the game folder.

Requires an existing **BepInEx 5** installation. Built against BepInEx **5.4.23.5**, the supplied game assembly, and Unity **2022.3.62** reference modules.

Use the normal movement and jump controls, pushing toward a wall to make contact. The mod preserves jump force, cooldown, stamina costs, injury effects, and climbing claws' existing unlimited-jump benefit. It applies to the current player body. There are no configuration settings or hotkeys.

To uninstall, close the game and delete `BepInEx\plugins\ThreeWallJumps.dll`.

### Testing and troubleshooting

74 offline assertions passed, covering the compiled jump budget, patch locations in the supplied game assembly, and the compiled transpiler operating on parsed original jump instructions. **The mod has not yet been tested in a running game.** See [the validation record](docs/VALIDATION.txt).

For an in-game check, unequip climbing claws, jump three times from one wall, and verify that a fourth same-side jump fails. Reach the opposite wall and verify that three more jumps are available. Return to the first wall and repeat.

If it fails to load or behave correctly, check `BepInEx\LogOutput.log`. Compatibility with the other installed QoL mods still needs an in-game test. Unexpected wall-jump code causes the plugin to remove its own patches and log an error.

### Source and rebuilding

The [source project](source/ThreeWallJumps) targets `netstandard2.1`. With a .NET SDK installed, run from the repository folder:

```powershell
dotnet build source/ThreeWallJumps/ThreeWallJumps.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust the project reference paths if the game's Data folder has a different name. Output: `source/ThreeWallJumps/bin/Release/netstandard2.1/ThreeWallJumps.dll`.

Game assemblies and BepInEx dependencies are not included. [SHA-256 checksums](downloads/SHA256SUMS.txt) identify the compiled downloads.
