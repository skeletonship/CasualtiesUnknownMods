Three Wall Jumps v1.0.3
Casualties Unknown 1.0.2 / BepInEx 5.4.23.5

INSTALL / UPDATE
Close the game. Replace the older DLL in BepInEx/plugins with ThreeWallJumps.dll.
Remove duplicate old copies, including names such as ThreeWallJumps(1).dll.
The startup log should identify Three Wall Jumps 1.0.3 and ACTIVE: verified...

BEHAVIOR
Right wall x3 -> left wall x3 -> right wall x3, repeatedly.
Opposite-wall contact resets the count even without jumping there.
Leaving and retouching the same side does not reset it.
Landing, climbing, and swimming also reset it as appropriate to the game.
Normal force, cooldown, stamina/injury costs and climbing-claw benefits remain.

NO EXTRA CONSOLE
The testing console has been removed. Existing ShowConsole entries are ignored.
Startup/errors use BepInEx/LogOutput.log. Optional per-jump file diagnostics:
  BepInEx/config/local.casualtiesunknown.threewalljumps.cfg
  LogJumpAttempts = false
The default is false for new configurations; existing values are preserved.
The mod does not control a console separately enabled by BepInEx.

LIFETIME / VALIDATION
The hooks remain active when the loader component is destroyed at startup.
v1.0.2's jumping behavior was confirmed working in-game by the user.
v1.0.3 removes console output; 63 offline Mono checks pass, including hook
survival, jump limits/resets, logging, and absence of the extra console.
The suite uses a game stand-in, not a running Unity game.

UNINSTALL
Close the game and delete ThreeWallJumps.dll from BepInEx/plugins.

BUILD
Source targets netstandard2.1. With a .NET SDK, from the source folder run:
  dotnet build -c Release -p:GameDir="C:\path\to\Casualties Unknown"
Adjust reference paths if needed. Dependencies are not bundled.
