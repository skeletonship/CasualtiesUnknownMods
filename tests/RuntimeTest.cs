using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BepInEx.Logging;
using Mono.Cecil;
using HarmonyLib;
class RuntimeTest {
 static int checks;
 static void Check(bool b,string m) { if(!b) throw new Exception(m); checks++; }
 static void Main(string[] args) {
  var a=Assembly.LoadFrom("ThreeWallJumps.dll");
  var events=new List<LogEventArgs>();
  var log=new ManualLogSource("RuntimeTest");
  log.LogEvent+=(sender,e)=>events.Add(e);
  var initialize=a.GetType("ThreeWallJumps.ModLog").GetMethod("Initialize",BindingFlags.Static|BindingFlags.NonPublic);
  initialize.Invoke(null,initialize.GetParameters().Length==3?new object[]{log,false,true}:new object[]{log,true});
  if(args.Length>=1) {
   using(var game=AssemblyDefinition.ReadAssembly(args[0]))
   using(var mod=AssemblyDefinition.ReadAssembly("ThreeWallJumps.dll")) {
    var gameBody=game.MainModule.Types.Single(t=>t.Name=="Body");
    foreach(var type in mod.MainModule.Types.Where(t=>t.Name=="JumpPatch" || t.Name=="ContactPatch"))
     foreach(var method in type.Methods)
      foreach(var parameter in method.Parameters.Where(p=>p.Name.StartsWith("___"))) {
       var field=gameBody.Fields.SingleOrDefault(f=>f.Name==parameter.Name.Substring(3));
       string expected=parameter.ParameterType.IsByReference?((ByReferenceType)parameter.ParameterType).ElementType.FullName:parameter.ParameterType.FullName;
       Check(field!=null && field.FieldType.FullName==expected,"Game field mismatch: "+parameter.Name);
      }
    Console.WriteLine("Verified all injected field bindings against the uploaded game assembly");
   }
  }
  var harmony=new Harmony("local.casualtiesunknown.threewalljumps");
  var installer=a.GetType("ThreeWallJumps.WallJumpPatches");
  installer.GetMethod("Install",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{harmony});
  var info=Harmony.GetPatchInfo(typeof(Body).GetMethod("Jump"));
  Check(info!=null && info.Prefixes.Count==1 && info.Postfixes.Count==1 && info.Finalizers.Count==1,"Four-hook installer did not attach Jump hooks");
  Console.WriteLine("Verified runtime patch registration: Jump prefix/postfix/finalizer + contact postfix");
  // Reproduce the real startup event seen in the user's BepInEx log. Use the
  // compiled plugin's actual OnDestroy callback rather than a second version.
  var pluginType=a.GetType("ThreeWallJumps.Plugin");
  var component=FormatterServices.GetUninitializedObject(pluginType);
  pluginType.GetField("harmony",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(component,harmony);
  var consoleType=a.GetType("ThreeWallJumps.LiveConsole");
  object consoleSentinel=consoleType==null?null:FormatterServices.GetUninitializedObject(consoleType);
  var consoleField=a.GetType("ThreeWallJumps.ModLog").GetField("console",BindingFlags.Static|BindingFlags.NonPublic);
  if(consoleField!=null) consoleField.SetValue(null,consoleSentinel);
  pluginType.GetMethod("OnDestroy",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(component,null);
  info=Harmony.GetPatchInfo(typeof(Body).GetMethod("Jump"));
  bool jumpAttached=info!=null && info.Prefixes.Any(p=>p.owner==harmony.Id) && info.Postfixes.Any(p=>p.owner==harmony.Id) && info.Finalizers.Any(p=>p.owner==harmony.Id);
  var contactInfo=Harmony.GetPatchInfo(typeof(Body).GetMethod("HandleGroundedState",BindingFlags.Instance|BindingFlags.NonPublic));
  bool contactAttached=contactInfo!=null && contactInfo.Postfixes.Any(p=>p.owner==harmony.Id);
  bool consoleRetained=consoleField!=null && ReferenceEquals(consoleField.GetValue(null),consoleSentinel);
  if(consoleField!=null) consoleField.SetValue(null,null);
  if(args.Length>=2 && args[1]=="--expect-lifecycle-failure") {
   Check(!jumpAttached && !contactAttached && !consoleRetained,"Expected v1.0.1 lifecycle failure was not reproduced");
   Console.WriteLine("REPRODUCED v1.0.1: plugin destruction removes jump/contact hooks and clears the console.");
   return;
  }
  Check(jumpAttached,"Plugin destruction removed jump hooks");
  Check(contactAttached,"Plugin destruction removed contact hook");
  Check(consoleType==null && consoleField==null,"Extra console implementation remains in the release build");
  Console.WriteLine("Verified hooks survive OnDestroy; extra console implementation removed");
  var body=new Body(); PlayerCamera.main=new PlayerCamera {body=body};
  foreach(int side in new[]{1,-1,1,-1}) {
   for(int i=1;i<=3;i++) {
    body.Contact(side); int before=body.Successes; body.TryJump();
    Check(body.Successes==before+1,"Expected jump "+i+" from side "+side);
   }
   body.Contact(side); int count=body.Successes; body.TryJump();
   Check(body.Successes==count,"Fourth jump must fail");
  }
  body.Contact(1); // Opposite-wall contact without jumping also resets the budget.
  body.Contact(-1);
  int prior=body.Successes; body.TryJump();
  Check(body.Successes==prior+1,"Opposite contact without jumping must reset");
  body.forceWalk=true; body.Contact(-1); prior=body.Successes; body.TryJump();
  Check(body.Successes==prior,"Forced walk must prevent a jump");
  body.forceWalk=false;
  for(int i=0;i<2;i++) { body.Contact(-1); body.TryJump(); }
  prior=body.Successes; body.Contact(-1); body.TryJump();
  Check(body.Successes==prior,"Failed attempts must not change budget");
  body.Contact(0); prior=body.Successes; body.TryJump();
  Check(body.Successes==prior,"No-contact jump must fail");
  body.Contact(-1); body.TryJump();
  Check(body.Successes==prior,"Leaving and retouching same wall must not reset");
  body.grounded=true; body.Contact(0); body.TryJump();
  body.grounded=false; body.Contact(-1);
  for(int i=0;i<3;i++){ prior=body.Successes; body.Contact(-1); body.TryJump(); Check(body.Successes==prior+1,"Landing must restore all three wall jumps"); }
  body.Claws=true; body.Contact(1); body.TryJump();
  for(int i=0;i<10;i++) { prior=body.Successes; body.Contact(1); body.TryJump(); Check(body.Successes==prior+1,"Climbing claws should retain unlimited jumps"); }
  var npc=new Body();
  npc.Contact(1); npc.TryJump(); prior=npc.Successes; npc.Contact(1); npc.TryJump();
  Check(npc.Successes==prior,"NPC wall-jump behavior should remain vanilla");
  // Unity can expose different managed wrappers for one native object.
  var aliasBody=new Body(); PlayerCamera.main.body=new Body {NativeId=aliasBody.NativeId};
  for(int i=0;i<3;i++){ prior=aliasBody.Successes; aliasBody.Contact(1); aliasBody.TryJump(); Check(aliasBody.Successes==prior+1,"Native Unity identity player check failed"); }
  aliasBody.Contact(-1); aliasBody.TryJump();
  aliasBody.Contact(-1); aliasBody.ThrowOnJump=true;
  bool error=false;
  try{ aliasBody.TryJump(); }catch(InvalidOperationException){ error=true; }
  Check(error,"Original jump exception should remain visible");
  aliasBody.ThrowOnJump=false;
  for(int i=0;i<2;i++){ prior=aliasBody.Successes; aliasBody.Contact(-1); aliasBody.TryJump(); Check(aliasBody.Successes==prior+1,"Exception should not consume an extra jump"); }
  prior=aliasBody.Successes; aliasBody.Contact(-1); aliasBody.TryJump();
  Check(aliasBody.Successes==prior,"Exception path leaked the temporary allowance");
  var registered=events.Where(e=>e.Data.ToString().StartsWith("REGISTERED:")).ToList();
  int total=(int)a.GetType("ThreeWallJumps.WallJumpHooks").GetProperty("TotalWallJumps",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
  Check(registered.Count==total,"Live counters and accepted-jump event count disagree");
  Check(events.Any(e=>e.Data.ToString().StartsWith("PLAYER DETECTED:")),"No player detection diagnostic");
  Check(events.Any(e=>e.Data.ToString().StartsWith("LIMIT:")),"No exhausted-budget diagnostic");
  Check(events.Any(e=>e.Data.ToString().StartsWith("WAITING: no recent wall contact")),"No missing-contact diagnostic");
  Check(events.Any(e=>e.Level==LogLevel.Error && e.Data.ToString().Contains("Simulated original game jump error")),"Original error not sent to error log");
  Console.WriteLine("PASS: "+checks+" runtime integration checks");
 }
}
