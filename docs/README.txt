Three Wall Jumps v1.0.0
For Casualties Unknown 1.0.2, BepInEx 5.4.23.5 (Windows, Unity Mono).

INSTALL
1. Close the game.
2. Copy ThreeWallJumps.dll into the game's BepInEx\plugins folder.
   If using the ZIP package, copy its BepInEx folder into the game folder.
3. Launch the game. BepInEx\LogOutput.log should contain:
   [Info   :Three Wall Jumps] Three Wall Jumps 1.0.0 loaded: ...

BEHAVIOR
You can wall jump three times in a row from the same wall side.
Example: right x3, left x3, right x3, left x3, indefinitely.
Moving into the opposite wall resets the budget immediately, even before
you jump from it. Simply leaving and retouching the same side does not reset it.
Landing, climbing, and swimming also reset it as appropriate to the game.
Keep using normal movement/jump controls and push toward the wall to make contact.
The game's wall-contact checks, jump force, cooldown, stamina costs, injury
effects, and climbing claws' existing unlimited-jump benefit are preserved.
The change applies to the current player body, not NPCs.
There are no hotkeys or configuration settings.

CHECK
Test without climbing claws equipped: jump from a wall three times. A fourth
jump from that side should fail. Reach the opposite wall: you get three jumps
there, and returning to the original wall gives three more.
If it doesn't work, send the new BepInEx\LogOutput.log.
Your log lists other QoL mods; conflicts with their movement patches are
possible and have not been tested in a running game.

UNINSTALL
Close the game and delete BepInEx\plugins\ThreeWallJumps.dll.
The plugin changes behavior in memory; it does not replace Assembly-CSharp.dll.

VALIDATION
Compiled against the uploaded Assembly-CSharp.dll and BepInEx/core DLLs.
Used UnityEngine reference modules for Unity 2022.3.62.
74 offline assertions passed, covering the actual compiled budget class,
patch anchors in the uploaded game assembly, and the compiled transpiler
operating on parsed original Jump IL with resolved wall fields and labels.
The game was not available to launch; live gameplay and compatibility with
the other installed plugins still need an in-game test.

SOURCE / REBUILD
Source files are included. Install a .NET SDK supporting netstandard2.1.
In the source folder, run:
  dotnet build -c Release -p:GameDir="C:\path\to\Casualties Unknown"
Adjust the project reference paths if the game's Data folder has another name.
Build output: bin\Release\netstandard2.1\ThreeWallJumps.dll
Game and BepInEx reference assemblies are not included in this package.
