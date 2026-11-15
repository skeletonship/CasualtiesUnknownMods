using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using UnityEngine;

internal class RuntimeTest {
 static int checks;
 static void Check(bool value,string message){if(!value)throw new Exception(message);checks++;}
 static Assembly plugin; static Type pluginType;
 static object Read(Body b,string field)=>typeof(Body).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(b);
 static void Set(Body b,string field,object value)=>typeof(Body).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(b,value);
 static Body Fresh(){Physics2D.Reset();var b=new Body();PlayerCamera.main=new PlayerCamera{body=b};return b;}
 static void Reach(float reach)=>pluginType.GetField("ReachTiles",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,reach);
 static void Main(string[] args) {
  plugin=Assembly.LoadFrom("EarlyWallJump.dll");pluginType=plugin.GetType("EarlyWallJump.Plugin");
  var logs=new List<LogEventArgs>();var logger=new ManualLogSource("EarlyTest");logger.LogEvent+=(s,e)=>logs.Add(e);
  pluginType.GetField("Log",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,logger);
  using(var mod=AssemblyDefinition.ReadAssembly("EarlyWallJump.dll")) {
   var entry=mod.MainModule.Types.Single(t=>t.FullName=="EarlyWallJump.Plugin");
   Check(entry.CustomAttributes.Any(a=>a.AttributeType.Name=="BepInIncompatibility" && (string)a.ConstructorArguments[0].Value=="local.casualtiesunknown.threewalljumps"),"Missing incompatibility with the 3-jump mod");
   Check(!mod.MainModule.Types.SelectMany(t=>t.Methods).Any(m=>m.IsPInvokeImpl),"Native console or other native import unexpectedly present");
   using(var game=AssemblyDefinition.ReadAssembly(args[0])) {
    var body=game.MainModule.Types.Single(t=>t.Name=="Body");
    foreach(var method in mod.MainModule.Types.Single(t=>t.Name=="JumpPatch").Methods)
     foreach(var parameter in method.Parameters.Where(p=>p.Name.StartsWith("___"))) {
      var field=body.Fields.SingleOrDefault(f=>f.Name==parameter.Name.Substring(3));
      string expected=parameter.ParameterType.IsByReference?((ByReferenceType)parameter.ParameterType).ElementType.FullName:parameter.ParameterType.FullName;
      Check(field!=null && field.FieldType.FullName==expected,"Injected field mismatch: "+parameter.Name);
     }
   }
  }
  var harmony=new Harmony("local.casualtiesunknown.earlywalljump");
  plugin.GetType("EarlyWallJump.JumpPatch").GetMethod("Install",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{harmony});
  var info=Harmony.GetPatchInfo(typeof(Body).GetMethod("Jump"));
  Check(info!=null && info.Prefixes.Count==1 && info.Finalizers.Count==1,"Hook registration failed");
  object component=FormatterServices.GetUninitializedObject(pluginType);
  pluginType.GetField("harmony",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(component,harmony);
  pluginType.GetMethod("OnDestroy",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(component,null);
  info=Harmony.GetPatchInfo(typeof(Body).GetMethod("Jump"));
  Check(info!=null && info.Prefixes.Count==1 && info.Finalizers.Count==1,"Startup destruction removed hooks");

  var b=Fresh();Physics2D.RightGap=.49f;b.moveDir=new Vector2(-1,0);b.TryJump();
  Check(b.Successes==1 && b.Direction==-1,"Early right jump failed with input already reversed left");
  Check(Physics2D.LastReach==.5f && Physics2D.LastMask==1,"Default range or ground mask is wrong");
  Check(Physics2D.LastSize.y<2f,"Cast should be inset from the feet/ceiling");
  Check((float)Read(b,"timeSinceSlidLeft")==100f && (float)Read(b,"timeSinceSlidRight")==100f,"Proximity contact leaked after jumping");
  Check(!(bool)Read(b,"firstWallJump") && (bool)Read(b,"lastJumpedOnRightWall"),"Original vanilla side state was not preserved");
  b.TryJump();Check(b.Successes==1,"Second jump from the same right wall must fail");
  for(int i=0;i<8;i++) {
   bool left=i%2==0;Physics2D.LeftGap=left?.49f:float.PositiveInfinity;Physics2D.RightGap=left?float.PositiveInfinity:.49f;
   b.moveDir=new Vector2(left?1:-1,0);int prior=b.Successes;b.TryJump();
   Check(b.Successes==prior+1 && b.Direction==(left?1:-1),"Alternating early jump failed");
   b.TryJump();Check(b.Successes==prior+1,"Same-side repeat must stay blocked");
  }
  foreach(float gap in new[]{.499f,.5f,.501f}) {
   b=Fresh();Physics2D.RightGap=gap;b.TryJump();
   Check(b.Successes==(gap<=.5f?1:0),"Half-tile range boundary failed at "+gap);
  }
  b=Fresh();b.TryJump();Check(b.Successes==0,"Empty air must not permit a jump");
  b=Fresh();Physics2D.RightGap=.2f;Physics2D.FloorNormal=true;b.TryJump();Check(b.Successes==0,"Floor/ceiling hit counted as a wall");
  b=Fresh();Physics2D.RightGap=.2f;Physics2D.Trigger=true;b.TryJump();Check(b.Successes==0,"Trigger counted as a wall");
  b=Fresh();Physics2D.LeftGap=.4f;Physics2D.RightGap=.2f;b.TryJump();Check(b.Direction==-1,"Nearest wall not selected when both sides eligible");
  b.TryJump();Check(b.Successes==2 && b.Direction==1,"Vanilla alternate side should be selected in a narrow gap");
  b=Fresh();b.Contact(-1);b.TryJump();Check(b.Successes==1 && Physics2D.Queries==0,"Existing vanilla jump was changed");
  Check((float)Read(b,"timeSinceSlidLeft")==0,"Real vanilla contact timer was changed");
  b=Fresh();Set(b,"timeSinceSlidRight",.2f);b.TryJump();Check(b.Successes==1 && Physics2D.Queries==0,"Vanilla contact grace was changed");
  b=Fresh();Physics2D.RightGap=.49f;b.grounded=true;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Ground jump should not use the extension");
  b=Fresh();Physics2D.RightGap=.49f;b.timeSinceGrounded=.05f;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Ground coyote jump should stay vanilla");
  b=Fresh();Physics2D.RightGap=.49f;b.bodyAffect.wasWater=true;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Swimming should stay vanilla");
  b=Fresh();Physics2D.RightGap=.49f;b.currentClimbable=new Climbable();b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Climbing should stay vanilla");
  b=Fresh();Physics2D.RightGap=.49f;b.forceWalk=true;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Forced walk should stay blocked");
  b=Fresh();Physics2D.RightGap=.49f;b.movingAllowed=false;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Movement restriction should stay blocked");
  b=Fresh();Physics2D.RightGap=.49f;b.standing=false;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Ragdoll should stay blocked");
  b=Fresh();Physics2D.RightGap=.49f;b.jumpCooldown=.1f;b.Jump();Check(b.Successes==0 && Physics2D.Queries==0,"Cooldown should stay blocked");
  b=Fresh();Physics2D.RightGap=.49f;b.col.enabled=false;b.TryJump();Check(b.Successes==0,"Disabled collider should not grant proximity jumping");
  b=Fresh();Physics2D.RightGap=.49f;var npc=new Body();npc.TryJump();Check(npc.Successes==0 && Physics2D.Queries==0,"NPC behavior changed");
  b=Fresh();Physics2D.RightGap=.49f;PlayerCamera.main.body=new Body{NativeId=b.NativeId};b.TryJump();Check(b.Successes==1,"Unity identity check failed");
  b=Fresh();Physics2D.RightGap=.49f;b.Claws=true;for(int i=0;i<5;i++)b.TryJump();Check(b.Successes==5,"Vanilla climbing-claw benefit was lost");
  Reach(.75f);b=Fresh();Physics2D.RightGap=.7f;b.TryJump();Check(b.Successes==1,"Configured reach was ignored");
  Reach(0);b=Fresh();Physics2D.RightGap=.49f;b.TryJump();Check(b.Successes==0 && Physics2D.Queries==0,"Zero reach should disable extra checks");Reach(.5f);
  b=Fresh();Physics2D.RightGap=.49f;b.ThrowOnJump=true;bool threw=false;
  try{b.TryJump();}catch(InvalidOperationException){threw=true;}
  Check(threw,"Original game exception was swallowed");
  Check((float)Read(b,"timeSinceSlidLeft")==100f && (float)Read(b,"timeSinceSlidRight")==100f,"Exception leaked synthetic contact");
  Check(logs.Any(e=>e.Level==LogLevel.Error && e.Data.ToString().Contains("Simulated original jump failure")),"Original error not logged");
  b=Fresh();Physics2D.RightGap=.49f;Physics2D.ThrowOnCast=true;b.TryJump();Check(b.Successes==0,"Physics failure should fall back to vanilla");
  Check(logs.Any(e=>e.Level==LogLevel.Error && e.Data.ToString().Contains("Simulated physics failure")),"Physics error not logged");
  Console.WriteLine("PASS: "+checks+" Early Wall Jump checks against compiled hooks and supplied Harmony under Mono.");
  Console.WriteLine("Physics queries use a controlled stand-in, not the live Unity engine; gameplay still needs testing.");
 }
}
