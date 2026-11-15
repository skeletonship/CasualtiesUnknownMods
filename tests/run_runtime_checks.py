"""Run compiled-plugin hooks on a small game stand-in with the supplied Harmony."""
import argparse
import os
from pathlib import Path
import shutil
import subprocess
import tempfile

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--plugin", type=Path, required=True)
parser.add_argument("--bepinex-core", type=Path, required=True)
parser.add_argument("--framework-refs", type=Path, required=True)
parser.add_argument("--game-assembly", type=Path, required=True)
parser.add_argument("--dotnet", default="dotnet")
parser.add_argument("--mono", default="mono")
parser.add_argument("--mono-config", type=Path)
parser.add_argument("--expect-lifecycle-failure", action="store_true", help="Reproduce the known v1.0.1 startup cleanup failure")
args = parser.parse_args()
sources = Path(__file__).resolve().parent
sdk_line = subprocess.check_output([args.dotnet, "--list-sdks"], text=True).strip().splitlines()[-1]
version, location = sdk_line.split(" ", 1)
csc = Path(location.strip("[]")) / version / "Roslyn/bincore/csc.dll"
references = [args.framework_refs.resolve() / f for f in ("mscorlib.dll", "System.dll", "System.Core.dll")]
with tempfile.TemporaryDirectory(prefix="three-wall-jumps-test-") as temporary:
    work = Path(temporary)
    for dll in args.bepinex_core.resolve().glob("*.dll"):
        shutil.copyfile(dll, work / dll.name)
    shutil.copyfile(args.plugin.resolve(), work / "ThreeWallJumps.dll")

    def compile_file(output, source, extra=(), executable=False):
        command = [args.dotnet, str(csc), "-nologo", "-nostdlib+", "-optimize+", "-langversion:latest",
                   "-target:" + ("exe" if executable else "library"), "-out:" + str(work / output)]
        command += ["-r:" + str(p) for p in references]
        command += ["-r:" + str(work / p) for p in extra]
        command += [str(sources / source)]
        subprocess.run(command, check=True)

    compile_file("UnityEngine.CoreModule.dll", "UnityStub.cs")
    compile_file("UnityEngine.dll", "UnityFacade.cs", ["UnityEngine.CoreModule.dll"])
    compile_file("Assembly-CSharp.dll", "GameStub.cs", ["UnityEngine.CoreModule.dll"])
    compile_file("RuntimeTest.exe", "RuntimeTest.cs", ["Assembly-CSharp.dll", "UnityEngine.CoreModule.dll",
                 "UnityEngine.dll", "BepInEx.dll", "0Harmony.dll", "Mono.Cecil.dll"], executable=True)
    command = [args.mono]
    if args.mono_config:
        command += ["--config", str(args.mono_config.resolve())]
    command += [str(work / "RuntimeTest.exe"), str(args.game_assembly.resolve())]
    if args.expect_lifecycle_failure:
        command += ["--expect-lifecycle-failure"]
    subprocess.run(command, cwd=work, env=os.environ.copy(), check=True)
