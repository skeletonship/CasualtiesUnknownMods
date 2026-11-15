Three Wall Jumps v1.0.2
Casualties Unknown 1.0.2 / BepInEx 5.4.23.5 / Windows Unity Mono

INSTALL / UPDATE
Close the game. Copy BepInEx/plugins/ThreeWallJumps.dll into your game's
BepInEx/plugins folder, replacing older builds. Remove duplicates, including ThreeWallJumps(1).dll.
The startup log should identify Three Wall Jumps 1.0.2 and say ACTIVE:
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

STARTUP FIX / VALIDATION
The v1.0.1 log confirmed that startup destroyed the mod's loader component.
Its OnDestroy cleanup then removed the hooks and closed the console.
v1.0.2 retains both for the game session. The process releases console resources
when the game exits. The log may say:
  Loader component destroyed; gameplay patches and live console retained...
This is expected and no longer disables the mod.

The new lifecycle regression test reproduced the v1.0.1 failure, then verified
that the actual compiled v1.0.2 callback preserves hooks and console state.
63 Mono checks passed, including jump behavior after component destruction.
Field bindings match the uploaded game assembly. Tests use a game stand-in;
live gameplay and the native Windows console still need an in-game test.
If it still fails, send a fresh LogOutput.log after attempting wall jumps.

UNINSTALL
Close the game and delete ThreeWallJumps.dll from BepInEx/plugins.
The plugin changes runtime behavior; it does not replace Assembly-CSharp.dll.

BUILD
Source targets netstandard2.1. With a .NET SDK, from the source folder run:
  dotnet build -c Release -p:GameDir="C:\path\to\Casualties Unknown"
Adjust Data-folder reference paths if needed. Dependencies are not bundled.
