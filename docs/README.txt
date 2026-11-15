Three Wall Jumps v1.0.1
Casualties Unknown 1.0.2 / BepInEx 5.4.23.5 / Windows Unity Mono

INSTALL / UPDATE
Close the game. Copy BepInEx/plugins/ThreeWallJumps.dll into your game's
BepInEx/plugins folder, replacing v1.0.0. Remove duplicate old copies.
The startup log should identify Three Wall Jumps 1.0.1 and say ACTIVE:
verified Jump prefix/postfix/finalizer and wall-contact postfix.

BEHAVIOR
Right wall x3 -> left wall x3 -> right wall x3, repeatedly.
Opposite-wall contact resets the budget even without jumping there.
Leaving and retouching the same side does not reset it.
Landing, climbing, and swimming also reset it as appropriate to the game.
Use normal controls and move toward the wall to make contact.
Normal cooldown, force, stamina/injury costs and climbing-claw benefits remain.

LIVE STATUS CONSOLE
Opens automatically on Windows, or reuses an existing BepInEx console.
Shows player detection, green accepted-jump counts, wall resets, session total,
and red errors. Missing wall contact and exhausted budgets are logged too.
A newly opened window's title shows the current side, count and total.
The console is a live log display, not an interactive command prompt.
Minimize it, or disable it with ShowConsole = false in:
  BepInEx/config/local.casualtiesunknown.threewalljumps.cfg
LogJumpAttempts = false reduces event logging. Restart after config changes.
Logs are also written through BepInEx to BepInEx/LogOutput.log.

VALIDATION / DIAGNOSTICS
v1.0.0 was reported to load but retain vanilla jumping. The exact in-game
cause remains unconfirmed. This build reworks the hooks and adds diagnostics.
60 Mono checks passed against a simplified game stand-in with the actual
supplied Harmony DLL. Field bindings match the uploaded game assembly.
This is not a live Unity/gameplay test; the Windows console UI is also untested.
If it still fails, send a new LogOutput.log after attempting wall jumps.
Look for ACTIVE, PLAYER DETECTED, REGISTERED, WAITING, LIMIT, or ERROR.

UNINSTALL
Close the game and delete ThreeWallJumps.dll from BepInEx/plugins.
The plugin changes runtime behavior; it does not replace Assembly-CSharp.dll.

BUILD
Source targets netstandard2.1. With a .NET SDK, from the source folder run:
  dotnet build -c Release -p:GameDir="C:\path\to\Casualties Unknown"
Adjust Data-folder reference paths if needed. Dependencies are not bundled.
