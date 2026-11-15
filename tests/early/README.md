# Early Wall Jump integration checks

The suite loads the **compiled mod** and supplied Harmony DLL under Mono. It executes the real prefix/finalizer against a small stand-in containing the inspected vanilla jump gate. Physics box casts use a controlled provider of wall gaps and hit normals; this does not launch Unity or validate a real scene's colliders.

Requirements: Python 3, .NET SDK, Mono, Mono/.NET Framework reference assemblies, your BepInEx core folder, the built mod, and your game DLL. Example from the repository root:

```sh
python3 tests/early/run_runtime_checks.py \
  --plugin downloads/EarlyWallJump.dll \
  --bepinex-core /path/to/game/BepInEx/core \
  --framework-refs /usr/lib/mono/4.7.2-api \
  --game-assembly /path/to/game/CasualtiesUnknown_Data/Managed/Assembly-CSharp.dll
```

Optional `--dotnet`, `--mono`, and `--mono-config` arguments support relocated tools. Normal Mono environment settings are inherited.

Checks cover injected fields against the actual game DLL, hook registration and survival after the actual OnDestroy callback, incompatibility with Three Wall Jumps, no native console imports, half-tile boundaries, input reversed before contact, one jump per side, repeated side changes, nearest eligible wall selection, contact-timer restoration, real-contact grace, floor/trigger rejection, ground/swim/climb/cooldown/movement restrictions, NPCs, Unity object identity, climbing claws, configurable reach, zero-reach fallback, and original/physics exception paths.

The suite intentionally raises simulated original-jump and physics exceptions to test fallback, logging, and restoration. No game or BepInEx DLLs are redistributed.
