using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BepInEx;
using HarmonyLib;

namespace ThreeWallJumps
{
    [BepInPlugin(Id, "Three Wall Jumps", "1.0.0")]
    [BepInProcess("CasualtiesUnknown.exe")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string Id = "local.casualtiesunknown.threewalljumps";
        private Harmony harmony;

        private void Awake()
        {
            try
            {
                harmony = new Harmony(Id);
                harmony.PatchAll(typeof(Plugin).Assembly);
                Logger.LogInfo("Three Wall Jumps 1.0.0 loaded: 3 jumps per wall side; switching walls resets the budget.");
            }
            catch (Exception exception)
            {
                harmony?.UnpatchSelf();
                Logger.LogError("Three Wall Jumps could not patch this game build; patches removed. " + exception);
            }
        }

        private void OnDestroy() => harmony?.UnpatchSelf();
    }

    internal static class WallJumpHooks
    {
        private static readonly ConditionalWeakTable<Body, WallJumpBudget> Budgets =
            new ConditionalWeakTable<Body, WallJumpBudget>();
        private static readonly AccessTools.FieldRef<Body, bool> FirstWallJump =
            AccessTools.FieldRefAccess<Body, bool>("firstWallJump");
        private static readonly AccessTools.FieldRef<Body, bool> LastJumpedRight =
            AccessTools.FieldRefAccess<Body, bool>("lastJumpedOnRightWall");
        private static readonly AccessTools.FieldRef<Body, float> SinceRightContact =
            AccessTools.FieldRefAccess<Body, float>("timeSinceSlidRight");

        internal static bool IsPlayer(Body body) =>
            PlayerCamera.main != null && ReferenceEquals(PlayerCamera.main.body, body);

        private static WallJumpBudget Budget(Body body) =>
            Budgets.GetValue(body, _ => new WallJumpBudget());

        // Replaces only the firstWallJump read in the game's existing eligibility check.
        // Keeping its true value preserves the normal climbing-claw upgrade.
        internal static bool CanRepeatWallJump(Body body)
        {
            bool original = FirstWallJump(body);
            if (!IsPlayer(body) || original)
                return original;
            // Matches the game's choice when assigning lastJumpedOnRightWall.
            int side = SinceRightContact(body) < 0.21f ? 1 : -1;
            return Budget(body).CanJump(side);
        }

        // Called only after the game has accepted a wall jump and set its wall side.
        internal static void RecordWallJump(Body body)
        {
            if (IsPlayer(body))
                Budget(body).RecordJump(LastJumpedRight(body) ? 1 : -1);
        }

        internal static void UpdateContact(Body body, bool left, bool right)
        {
            if (!IsPlayer(body))
                return;
            WallJumpBudget budget = Budget(body);
            if (body.grounded || body.currentClimbable != null ||
                (body.bodyAffect != null && body.bodyAffect.wasWater))
                budget.Reset();
            else if (left != right)
                budget.Touch(right ? 1 : -1);
        }
    }

    [HarmonyPatch(typeof(Body), "Jump")]
    internal static class JumpPatch
    {
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> code = instructions.ToList();
            const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo first = typeof(Body).GetField("firstWallJump", privateInstance);
            FieldInfo lastRight = typeof(Body).GetField("lastJumpedOnRightWall", privateInstance);
            int reads = code.Count(i => i.opcode == OpCodes.Ldfld && Equals(i.operand, first));
            int writes = code.Count(i => i.opcode == OpCodes.Stfld && Equals(i.operand, lastRight));
            if (reads != 1 || writes != 2)
                throw new InvalidOperationException("Unexpected Body.Jump wall-jump code (expected 1 eligibility read and 2 side writes).");

            const BindingFlags internalStatic = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo allow = typeof(WallJumpHooks).GetMethod(nameof(WallJumpHooks.CanRepeatWallJump), internalStatic);
            MethodInfo record = typeof(WallJumpHooks).GetMethod(nameof(WallJumpHooks.RecordWallJump), internalStatic);
            foreach (CodeInstruction instruction in code)
            {
                if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, first))
                {
                    // The existing Body instance remains on the stack. Retain labels/blocks.
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = allow;
                }
                yield return instruction;
                if (instruction.opcode == OpCodes.Stfld && Equals(instruction.operand, lastRight))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, record);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Body), "HandleGroundedState")]
    internal static class ContactPatch
    {
        private static void Postfix(Body __instance, bool ___slidingLeft, bool ___slidingRight) =>
            WallJumpHooks.UpdateContact(__instance, ___slidingLeft, ___slidingRight);
    }
}
