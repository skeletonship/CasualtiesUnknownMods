# Runtime integration checks

These checks load the **compiled mod** with the supplied **BepInEx/Harmony DLLs** under Mono, run the same explicit patch installer used at startup, and execute patched jump/contact methods on a small game stand-in. Its wall eligibility gate follows the inspected game code. The suite also validates every injected field name and type against the real game assembly using Cecil.

This is an offline integration test. The stand-in replaces Unity and the game world; passing does not establish that a Windows console opens or that a particular in-game failure has been fixed.

Requirements: Python 3, .NET SDK (C# compiler), Mono, Mono/.NET Framework reference assemblies, your BepInEx `core` folder, and your game DLL. No game or BepInEx assemblies are included here.

Example (adapt paths):

```sh
python3 tests/run_runtime_checks.py \
  --plugin downloads/ThreeWallJumps.dll \
  --bepinex-core /path/to/game/BepInEx/core \
  --framework-refs /usr/lib/mono/4.7.2-api \
  --game-assembly /path/to/game/CasualtiesUnknown_Data/Managed/Assembly-CSharp.dll
```

Use `--dotnet`, `--mono`, and `--mono-config` to select relocated tools if necessary. A relocated Mono installation may also require its usual `MONO_PATH`, `MONO_CFG_DIR`, and `LD_LIBRARY_PATH` settings.

Covered: four-hook registration and ownership, three jumps followed by a blocked fourth, repeated wall switches, contact-only resets, landing resets, failed/no-contact attempts, NPC behavior, climbing claws, Unity native-object identity, exception propagation and allowance restoration, accepted-jump counters, and diagnostics/error events. One intentional simulated exception verifies that original game errors remain visible.
