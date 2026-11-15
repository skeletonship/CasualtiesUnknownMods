# Casualties Unknown Mods

## Three Wall Jumps v1.0.1

A BepInEx plugin for **Casualties Unknown 1.0.2** that allows **three consecutive wall jumps from the same side**.

**Right wall ×3 → left wall ×3 → right wall ×3 → left wall ×3**, as often as needed.

Moving into the opposite wall resets the count immediately, even if you do not jump from that wall. Leaving and retouching the same side does not reset it. Landing, climbing, and swimming also reset it as appropriate to the game.

### Download and install

1. Download [ThreeWallJumps.dll](downloads/ThreeWallJumps.dll). On its GitHub file page, click **Download raw file**.
2. Close the game and put the DLL into **`BepInEx\plugins`** in the game folder.
3. Launch the game. `BepInEx\LogOutput.log` should report **Three Wall Jumps 1.0.1 starting**.

Alternatively, download [the install ZIP](downloads/ThreeWallJumps-v1.0.1.zip) and copy its `BepInEx` folder into the game folder.

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

### Update from v1.0.0

Close the game and **replace the old DLL** with v1.0.1; do not keep duplicate copies in different plugin folders. The BepInEx log should identify **Three Wall Jumps 1.0.1** and then report **ACTIVE: verified...**.

This build replaces the transpiler with explicit prefix/postfix/finalizer hooks, restores temporary eligibility after failed attempts, verifies registration, and uses Unity object identity when recognizing the player.

### Testing and troubleshooting

The original v1.0.0 build was reported to load but retain the vanilla one-jump limit, including with other mods removed. Its in-game root cause has not been confirmed: the earlier build passed a simplified Mono integration test. v1.0.1 changes that path and adds diagnostics to expose what happens in the game.

**60 checks passed under Mono** with the supplied Harmony DLL, executing the compiled v1.0.1 hooks against a small game stand-in. They cover runtime registration, jump limits/resets, exception paths, counter logging, and error logging. Injected field names/types also match the actual uploaded game assembly. See [the validation record](docs/VALIDATION.txt) and [runnable test sources](tests).

**A running-game test and a Windows console UI test are still needed.** The standalone test does not include the real Unity engine or world.

Unequip climbing claws, jump three times from one wall, and verify a fourth jump fails. Reach the opposite wall and repeat. If behavior is wrong, send the new `BepInEx/LogOutput.log` after trying several jumps. Its `PLAYER DETECTED`, `REGISTERED`, `WAITING`, and `LIMIT` messages distinguish the execution paths. Compatibility with the other QoL mods is still subject to an in-game test.

### Source and rebuilding

The [source project](source/ThreeWallJumps) targets `netstandard2.1`. With a .NET SDK installed, run from the repository folder:

```powershell
dotnet build source/ThreeWallJumps/ThreeWallJumps.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust the project reference paths if the game's Data folder has a different name. Output: `source/ThreeWallJumps/bin/Release/netstandard2.1/ThreeWallJumps.dll`.

Game assemblies and BepInEx dependencies are not included. [SHA-256 checksums](downloads/SHA256SUMS.txt) identify the compiled downloads.
