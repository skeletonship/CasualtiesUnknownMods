# Casualties Unknown Mods

## Three Wall Jumps v1.0.3

A BepInEx plugin for **Casualties Unknown 1.0.2** that allows **three consecutive wall jumps from the same side**.

**Right wall ×3 → left wall ×3 → right wall ×3**, repeatedly. Contact with the opposite wall resets the count, even without jumping from it. Leaving and retouching the same side does not reset it. Landing, climbing, and swimming also reset it as appropriate to the game.

### Download and install

1. Download [ThreeWallJumps.dll](downloads/ThreeWallJumps.dll). On the file page, click **Download raw file**.
2. Close the game and replace your old DLL in **`BepInEx\plugins`**. Remove duplicate old copies, including renamed downloads such as `ThreeWallJumps(1).dll`.
3. Launch the game. `BepInEx/LogOutput.log` should identify **Three Wall Jumps 1.0.3** and report **ACTIVE: verified...**.

Alternatively, download [the install ZIP](downloads/ThreeWallJumps-v1.0.3.zip) and copy its `BepInEx` folder into the game folder.

Requires BepInEx 5. Built against BepInEx **5.4.23.5**, the supplied game assembly, and Unity **2022.3.62** reference modules.

### Console removed

**v1.0.3 removes the testing console entirely.** It does not allocate a console, open a window, or write colored console output. Existing `ShowConsole` settings are ignored. The working jump hooks and startup-lifetime fix from v1.0.2 are retained.

Startup messages and errors still use normal BepInEx logging in `BepInEx/LogOutput.log`. Optional per-jump diagnostics use `LogJumpAttempts` in `BepInEx/config/local.casualtiesunknown.threewalljumps.cfg`. This defaults to `false` for new configurations; an existing value is preserved. Set it to `false` if you want less file logging. Any separately enabled BepInEx console is controlled by BepInEx itself.

### Gameplay

Use the normal movement and jump controls, pushing toward a wall to make contact. The mod preserves jump force, cooldown, stamina/injury costs, and climbing claws' existing unlimited-jump benefit. It applies to the current player body. There are no hotkeys.

The loader's Unity component is destroyed during startup in this game. The hooks intentionally remain active for the game process after that event. The log message **Loader component destroyed; gameplay patches retained for the game session.** is expected.

To uninstall, close the game and delete the mod DLL from `BepInEx/plugins`.

### Validation

The user confirmed v1.0.2 working in-game, including three jumps, fourth-jump rejection, and wall-switch resets. v1.0.3 only removes the extra console/status output and changes the default verbosity for new configurations.

**63 offline Mono checks pass** against the compiled v1.0.3 with the supplied Harmony DLL and a game stand-in. They cover patch installation, hook survival after the real `OnDestroy` callback, jump limits/resets, failed attempts, and BepInEx logging. They also verify the extra console class/state is absent. Field bindings match the uploaded game DLL. See [validation](docs/VALIDATION.txt) and [runnable checks](tests).

The offline checks do not replace a running Unity game. For troubleshooting, send a new `BepInEx/LogOutput.log` after trying wall jumps.

### Source and rebuilding

The [source project](source/ThreeWallJumps) targets `netstandard2.1`. With a .NET SDK installed, run:

```powershell
dotnet build source/ThreeWallJumps/ThreeWallJumps.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust Data-folder reference paths if needed. Output: `source/ThreeWallJumps/bin/Release/netstandard2.1/ThreeWallJumps.dll`. Game/BepInEx reference assemblies are not redistributed. [SHA-256 checksums](downloads/SHA256SUMS.txt) identify current downloads.
