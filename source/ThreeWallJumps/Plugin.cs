using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;

namespace ThreeWallJumps
{
    [BepInPlugin(Id, "Three Wall Jumps", Version)]
    [BepInProcess("CasualtiesUnknown.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string Id = "local.casualtiesunknown.threewalljumps";
        internal const string Version = "1.0.3";
        private Harmony harmony;

        private void Awake()
        {
            bool logAttempts = Config.Bind("Diagnostics", "LogJumpAttempts", false,
                "Log changes in jump eligibility, accepted wall jumps, and wall resets.").Value;
            ModLog.Initialize(Logger, logAttempts);
            ModLog.Info("Three Wall Jumps " + Version + " starting. Waiting for player...");
            try
            {
                harmony = new Harmony(Id);
                WallJumpPatches.Install(harmony);
                ModLog.Info("ACTIVE: verified Jump prefix/postfix/finalizer and wall-contact postfix.");
                ModLog.Info("3 RIGHT -> 3 LEFT -> 3 RIGHT. Logging through BepInEx; no extra console window.");
            }
            catch (Exception exception)
            {
                harmony?.UnpatchSelf();
                ModLog.Error("DISABLED: failed to install/verify patches. " + exception);
            }
        }

        private void OnDestroy()
        {
            // This game destroys the loader's Unity component during startup,
            // before a run begins. Harmony hooks and static logging state must
            // outlive that component for the entire game process.
            ModLog.Info("Loader component destroyed; gameplay patches retained for the game session.");
        }
    }

    internal static class WallJumpPatches
    {
        internal static void Install(Harmony harmony)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo jump = typeof(Body).GetMethod("Jump", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo contact = typeof(Body).GetMethod("HandleGroundedState", BindingFlags.Instance | BindingFlags.NonPublic);
            if (jump == null || contact == null)
                throw new MissingMethodException("Expected Body.Jump and Body.HandleGroundedState.");
            MethodInfo prefix = typeof(JumpPatch).GetMethod("Prefix", flags);
            MethodInfo postfix = typeof(JumpPatch).GetMethod("Postfix", flags);
            MethodInfo finalizer = typeof(JumpPatch).GetMethod("Finalizer", flags);
            MethodInfo contactPostfix = typeof(ContactPatch).GetMethod("Postfix", flags);
            harmony.Patch(jump, prefix: new HarmonyMethod(prefix), postfix: new HarmonyMethod(postfix),
                finalizer: new HarmonyMethod(finalizer));
            harmony.Patch(contact, postfix: new HarmonyMethod(contactPostfix));
            Patches jumpInfo = Harmony.GetPatchInfo(jump);
            Patches contactInfo = Harmony.GetPatchInfo(contact);
            if (jumpInfo == null || contactInfo == null ||
                !HasPatch(jumpInfo.Prefixes, prefix) || !HasPatch(jumpInfo.Postfixes, postfix) ||
                !HasPatch(jumpInfo.Finalizers, finalizer) || !HasPatch(contactInfo.Postfixes, contactPostfix))
                throw new InvalidOperationException("Harmony did not register all four expected hooks.");
        }

        private static bool HasPatch(System.Collections.Generic.IEnumerable<Patch> patches, MethodInfo method)
        {
            foreach (Patch patch in patches)
                if (patch.owner == Plugin.Id && patch.PatchMethod == method) return true;
            return false;
        }
    }

    internal sealed class PlayerJumpState
    {
        internal readonly WallJumpBudget Budget = new WallJumpBudget();
        internal string LastReason;
    }

    internal static class WallJumpHooks
    {
        private static readonly ConditionalWeakTable<Body, PlayerJumpState> States =
            new ConditionalWeakTable<Body, PlayerJumpState>();
        private static Body observedPlayer;
        internal static int TotalWallJumps { get; private set; }

        internal static bool IsPlayer(Body body) =>
            PlayerCamera.main != null && body != null && body == PlayerCamera.main.body;

        internal static PlayerJumpState State(Body body)
        {
            if (!ReferenceEquals(observedPlayer, body))
            {
                observedPlayer = body;
                ModLog.Info("PLAYER DETECTED: wall-jump hooks are receiving the current player body.");
            }
            return States.GetValue(body, _ => new PlayerJumpState());
        }

        internal static void ReportReason(PlayerJumpState state, string reason)
        {
            if (state.LastReason == reason) return;
            state.LastReason = reason;
            ModLog.Event(reason);
        }

        internal static string SideName(int side) => side > 0 ? "RIGHT" : side < 0 ? "LEFT" : "NONE";

        internal static void RecordWallJump(Body body, bool right)
        {
            PlayerJumpState state = State(body);
            state.Budget.RecordJump(right ? 1 : -1);
            if (TotalWallJumps < int.MaxValue) TotalWallJumps++;
            state.LastReason = null;
            ModLog.Event("REGISTERED: " + SideName(state.Budget.Side) + " " + state.Budget.Used + "/3 | total wall jumps: " + TotalWallJumps);
        }

        internal static void UpdateContact(Body body, bool left, bool right)
        {
            if (!IsPlayer(body)) return;
            PlayerJumpState state = State(body);
            if (body.grounded || body.currentClimbable || (body.bodyAffect && body.bodyAffect.wasWater))
            {
                if (state.Budget.Side != 0)
                    ModLog.Event("RESET: grounded, climbing, or in water; 3 jumps available.");
                state.Budget.Reset();
            }
            else if (left != right)
            {
                int side = right ? 1 : -1;
                if (state.Budget.Side != side)
                {
                    state.Budget.Touch(side);
                    state.LastReason = null;
                    ModLog.Event("CONTACT: " + SideName(side) + " | reset to 0/3.");
                }
            }
        }
    }

    internal struct JumpAttempt
    {
        internal bool Player, WallBranch, TemporarilyAllowed, OriginalFirst;
        internal float CooldownBefore;
    }

    internal static class JumpPatch
    {
        // Open the original eligibility gate for jumps 2 and 3 for this call.
        internal static void Prefix(Body __instance, ref bool ___firstWallJump,
            float ___timeSinceSlidLeft, float ___timeSinceSlidRight, float ___timeSinceGrounded,
            float ___jumpCooldown, bool ___movingAllowed, ref JumpAttempt __state)
        {
            __state = default(JumpAttempt);
            try
            {
                if (!WallJumpHooks.IsPlayer(__instance)) return;
                __state.Player = true;
                __state.CooldownBefore = ___jumpCooldown;
                __state.OriginalFirst = ___firstWallJump;
                PlayerJumpState state = WallJumpHooks.State(__instance);
                __state.WallBranch = !__instance.grounded && !__instance.currentClimbable &&
                    ___timeSinceGrounded > 0.11f && !(__instance.bodyAffect && __instance.bodyAffect.wasWater);
                if (!__instance.standing || __instance.forceWalk || !___movingAllowed)
                {
                    WallJumpHooks.ReportReason(state, "WAITING: movement/standing restrictions prevent jumping.");
                    return;
                }
                if (___jumpCooldown > 0f || !__state.WallBranch) return;
                bool left = ___timeSinceSlidLeft < 0.21f;
                bool right = ___timeSinceSlidRight < 0.21f;
                if (!left && !right)
                {
                    WallJumpHooks.ReportReason(state, "WAITING: no recent wall contact. Move toward the wall before jumping again.");
                    return;
                }
                int side = right ? 1 : -1;
                if (state.Budget.CanJump(side))
                {
                    if (!___firstWallJump)
                    {
                        ___firstWallJump = true;
                        __state.TemporarilyAllowed = true;
                    }
                }
                else if (!___firstWallJump)
                    WallJumpHooks.ReportReason(state, "LIMIT: " + WallJumpHooks.SideName(side) + " 3/3. Reach the opposite wall to reset.");
            }
            catch (Exception exception)
            {
                if (__state.TemporarilyAllowed) ___firstWallJump = __state.OriginalFirst;
                __state = default(JumpAttempt);
                ModLog.Error("Jump prefix failed: " + exception);
            }
        }

        internal static void Postfix(Body __instance, ref bool ___firstWallJump,
            bool ___lastJumpedOnRightWall, float ___jumpCooldown, JumpAttempt __state)
        {
            if (!__state.Player) return;
            try
            {
                bool accepted = __state.CooldownBefore <= 0f && ___jumpCooldown > 0f;
                if (accepted && __state.WallBranch)
                    WallJumpHooks.RecordWallJump(__instance, ___lastJumpedOnRightWall);
                else if (__state.TemporarilyAllowed)
                    ___firstWallJump = __state.OriginalFirst;
                // A successful original jump writes its own flag, including claws.
            }
            catch (Exception exception)
            {
                if (__state.TemporarilyAllowed) ___firstWallJump = __state.OriginalFirst;
                ModLog.Error("Jump postfix failed: " + exception);
            }
        }

        internal static Exception Finalizer(Exception __exception, ref bool ___firstWallJump, JumpAttempt __state)
        {
            if (__exception != null)
            {
                if (__state.TemporarilyAllowed) ___firstWallJump = __state.OriginalFirst;
                ModLog.Error("Body.Jump threw an exception: " + __exception);
            }
            return __exception;
        }
    }

    internal static class ContactPatch
    {
        internal static void Postfix(Body __instance, bool ___slidingLeft, bool ___slidingRight)
        {
            try { WallJumpHooks.UpdateContact(__instance, ___slidingLeft, ___slidingRight); }
            catch (Exception exception) { ModLog.Error("Wall-contact hook failed: " + exception); }
        }
    }
}
