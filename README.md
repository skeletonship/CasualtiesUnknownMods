# Casualties Unknown Mods

## Three Wall Jumps v1.0.3

A BepInEx plugin for **Casualties Unknown 1.0.2** that allows **three consecutive wall jumps from the same side**.

**Right wall ×3 → left wall ×3 → right wall ×3**, repeatedly. Contact with the opposite wall resets the count, even without jumping from it. Leaving and retouching the same side does not reset it. Landing, climbing, and swimming also reset it as appropriate to the game.

### Download and install

1. Download ThreeWallJumps.dll. On the file page, click **Download raw file**.
2. Place the file into 'BepInEX/plugins'.
3. Launch the game. `BepInEx/LogOutput.log` should identify **Three Wall Jumps 1.0.3** and report **ACTIVE: verified...**.

Alternatively, download [the install ZIP](downloads/ThreeWallJumps-v1.0.3.zip) and copy its `BepInEx` folder into the game folder.

Requires BepInEx 5. Built against BepInEx **5.4.23.5**, the supplied game assembly, and Unity **2022.3.62** reference modules.


### Gameplay

Use the normal movement and jump controls, pushing toward a wall to make contact. The mod preserves jump force, cooldown, stamina/injury costs, and climbing claws' existing unlimited-jump benefit. It applies to the current player body. There are no hotkeys.

The loader's Unity component is destroyed during startup in this game. The hooks intentionally remain active for the game process after that event. The log message **Loader component destroyed; gameplay patches retained for the game session.** is expected.

To uninstall, close the game and delete the mod DLL from `BepInEx/plugins`.


### Source and rebuilding

The [source project](source/ThreeWallJumps) targets `netstandard2.1`. With a .NET SDK installed, run:

```powershell
dotnet build source/ThreeWallJumps/ThreeWallJumps.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust Data-folder reference paths if needed. Output: `source/ThreeWallJumps/bin/Release/netstandard2.1/ThreeWallJumps.dll`. Game/BepInEx reference assemblies are not redistributed. [SHA-256 checksums](downloads/SHA256SUMS.txt) identify current downloads.
