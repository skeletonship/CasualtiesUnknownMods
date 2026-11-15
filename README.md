# Casualties Unknown Mods

## Three Wall Jumps v1.0.2

A BepInEx plugin for **Casualties Unknown 1.0.2** that allows **three consecutive wall jumps from the same side**.

**Right wall ×3 → left wall ×3 → right wall ×3 → left wall ×3**, as often as needed.

Moving into the opposite wall resets the count immediately, even if you do not jump from that wall. Leaving and retouching the same side does not reset it. Landing, climbing, and swimming also reset it as appropriate to the game.

### Download and install

1. Download [ThreeWallJumps.dll](downloads/ThreeWallJumps.dll). On its GitHub file page, click **Download raw file**.
2. Close the game and put the DLL into **`BepInEx\plugins`** in the game folder.
3. Launch the game. `BepInEx\LogOutput.log` should report **Three Wall Jumps 1.0.2 starting**.

Alternatively, download [the install ZIP](downloads/ThreeWallJumps-v1.0.2.zip) and copy its `BepInEx` folder into the game folder.

Requires an existing **BepInEx 5** installation. Built against BepInEx **5.4.23.5**, the supplied game assembly, and Unity **2022.3.62** reference modules.

Use the normal movement and jump controls, pushing toward a wall to make contact. The mod preserves jump force, cooldown, stamina costs, injury effects, and climbing claws' existing unlimited-jump benefit. It applies to the current player body. The live console is enabled by default; no hotkeys are required.

To uninstall, close the game and delete `BepInEx\plugins\ThreeWallJumps.dll`.

### Live console

A Windows console opens automatically when the mod starts (or the existing BepInEx console is reused). It shows:

- **ACTIVE** only after all four runtime hooks have been verified.
- **PLAYER DETECTED** when a hook recognizes the player.
- **Green REGISTERED events** with the current wall count (`RIGHT 1/3`, `2/3`, `3/3`) and session total.
- Wall contact and budget-reset events, plus reasons such as missing contact or a reached limit.
- **Red ERROR messages** for installation failures, hook failures, and exceptions from the original jump method.

For a newly opened console, the window title also shows the current side, budget, and total. It is a live log window, not an interactive command prompt. You can minimize it. To disable it, close the game and set `ShowConsole = false` in `BepInEx/config/local.casualtiesunknown.threewalljumps.cfg`. `LogJumpAttempts = false` reduces event logging. Restart after changing settings. Events and errors also go to `BepInEx/LogOutput.log`.

### Update from older builds

Close the game and **replace the old DLL** with v1.0.2. Remove older copies, including renamed downloads such as `ThreeWallJumps(1).dll`, from all plugin subfolders. Keep just one current copy.

The BepInEx log should identify **Three Wall Jumps 1.0.2**, report **ACTIVE: verified...**, and may then report **Loader component destroyed; gameplay patches and live console retained for the game session.** That last message is expected: the hooks and live console now remain active for the lifetime of the game process.

### Confirmed startup fix in v1.0.2

The supplied v1.0.1 log showed successful patch installation followed immediately by **Three Wall Jumps stopped; patches removed.**, before the run began. The loader's Unity component was destroyed during startup, and the mod's `OnDestroy` cleanup removed its gameplay hooks and closed its console. This explains both the disappearing console and vanilla jumping in that run.

v1.0.2 retains the Harmony patches and static console/log state when the loader component is destroyed. The process releases the console resources on game exit. No extra persistent Unity component is required.

### Testing and troubleshooting

A new regression test invokes the **compiled plugin's actual `OnDestroy` callback**. It reproduced v1.0.1 removing the jump/contact hooks and clearing the console reference. The fixed build keeps all four hooks and the console reference alive, then passes the jump tests after that destruction event.

**63 checks passed under Mono** with the supplied Harmony DLL and compiled v1.0.2, using a small game stand-in. These include lifecycle survival, jump limits/resets, exception paths, counter logging, and error logging. Injected field names/types also match the actual uploaded game assembly. See [the validation record](docs/VALIDATION.txt) and [runnable test sources](tests).

**A running-game test and a Windows console UI test are still needed.** The standalone test replaces the real Unity engine/world and checks console lifetime without creating a native Windows window.

Unequip climbing claws, jump three times from one wall, and verify a fourth jump fails. Reach the opposite wall and repeat. If behavior is wrong, send the new `BepInEx/LogOutput.log` after trying several jumps. The `PLAYER DETECTED`, `REGISTERED`, `WAITING`, and `LIMIT` messages identify the execution path.

### Source and rebuilding

The [source project](source/ThreeWallJumps) targets `netstandard2.1`. With a .NET SDK installed, run from the repository folder:

```powershell
dotnet build source/ThreeWallJumps/ThreeWallJumps.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust the project reference paths if the game's Data folder has a different name. Output: `source/ThreeWallJumps/bin/Release/netstandard2.1/ThreeWallJumps.dll`.

Game assemblies and BepInEx dependencies are not included. [SHA-256 checksums](downloads/SHA256SUMS.txt) identify the compiled downloads.
