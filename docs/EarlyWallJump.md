# Early Wall Jump v1.0.0

For Casualties Unknown 1.0.2 with BepInEx 5. Allows a wall jump when the side of the character is within **half a tile** of a wall, even if movement input already points away from it.

**Keeps vanilla's one wall jump per side.** After jumping from the right wall, the next wall jump must come from the left (and vice versa). Climbing claws retain their normal upgrade behavior.

## Install

1. Close the game and remove **ThreeWallJumps.dll** and any renamed copies from `BepInEx/plugins`. Choose one of these mods; Early Wall Jump declares them incompatible so the three-jump mod cannot silently change this one's limit.
2. Download **EarlyWallJump.dll** and put it in `BepInEx/plugins`.
3. Launch the game. `BepInEx/LogOutput.log` should identify **Early Wall Jump 1.0.0 ACTIVE** with a reach of **0.5 tiles**.

If using the ZIP package, copy its `BepInEx` folder into the game folder after removing the other mod. There is no extra console window.

## Reach setting

After the first launch, the setting is in:

```text
BepInEx/config/local.casualtiesunknown.earlywalljump.cfg
```

```ini
[Wall Jump]
ReachTiles = 0.5
```

The value is the maximum gap from the player's collider edge to the wall, measured in tiles. The inspected game grid maps one tile to one world unit. The default is half a tile. You can use values from 0 to 2; 0 disables the added proximity checks. Close the game before editing and restart afterward.

## How it behaves

The mod checks nearby terrain only when the original game would reject an airborne jump. It does not require input toward the wall. If a valid wall is in range, it temporarily supplies wall contact for that jump call and immediately restores the original contact timers afterward, including on exceptions.

Vanilla still decides the allowed side, jump force, cooldown, stamina cost, injuries, and climbing-claw effects. Actual wall sliding, friction, grounded detection, climbing, swimming, and the existing real-contact grace period retain their original behavior. The range is not a permanent wall-contact or wall-stick effect. NPCs are excluded. Floor/ceiling normals and trigger volumes are rejected as proximity walls.

In a narrow gap where both walls are in range, the mod chooses the nearest eligible wall. If vanilla requires the opposite side, only that side is eligible.

The hooks remain active when the game's startup destroys the loader component. Uninstall by closing the game and deleting EarlyWallJump.dll.

## Validation

65 offline assertions passed under Mono with the supplied Harmony DLL and the compiled mod. The suite checks the reach boundary, reversed input, one-jump rule, repeated alternating walls, timer restoration, filters, vanilla grace/cooldown/gear, startup lifetime, error paths, absence of native console imports, and the incompatibility declaration. All injected field names/types match the uploaded game DLL.

Physics queries use a controlled stand-in for Unity, so the new distance behavior still needs an in-game test. No proprietary game or BepInEx DLLs are bundled.

## Build

With a .NET SDK installed, run from this repository:

```powershell
dotnet build source/EarlyWallJump/EarlyWallJump.csproj -c Release -p:GameDir="C:\path\to\Casualties Unknown"
```

Adjust the Data-folder reference paths if necessary. Output: `source/EarlyWallJump/bin/Release/netstandard2.1/EarlyWallJump.dll`.
