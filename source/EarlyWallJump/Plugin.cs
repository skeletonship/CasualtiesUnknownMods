using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EarlyWallJump
{
    [BepInPlugin(Id, "Early Wall Jump", Version)]
    [BepInProcess("CasualtiesUnknown.exe")]
    [BepInIncompatibility("local.casualtiesunknown.threewalljumps")]
    public sealed class Plugin : BaseUnityPlugin
    {
        internal const string Id = "local.casualtiesunknown.earlywalljump";
        internal const string Version = "1.0.0";
        internal static float ReachTiles = 0.5f;
        internal static ManualLogSource Log;
        private Harmony harmony;

        private void Awake()
        {
            Log = Logger;
            ReachTiles = Config.Bind("Wall Jump", "ReachTiles", 0.5f,
                new ConfigDescription("Maximum wall gap in tiles for an early wall jump. Independent of movement input. 0 disables extra proximity checks. Restart after changing.",
                    new AcceptableValueRange<float>(0f, 2f))).Value;
            try
            {
                harmony = new Harmony(Id);
                JumpPatch.Install(harmony);
                Log.LogInfo("Early Wall Jump " + Version + " ACTIVE: reach " + ReachTiles +
                    " tiles; vanilla one jump per wall side. Jump prefix/finalizer verified.");
            }
            catch (Exception exception)
            {
                harmony?.UnpatchSelf();
                Log.LogError("Early Wall Jump DISABLED: " + exception);
            }
        }

        private void OnDestroy()
        {
            // This game destroys the loader's Unity component at startup. The
            // static hooks must remain registered until the game process exits.
            Log?.LogInfo("Loader component destroyed; Early Wall Jump hooks retained for the game session.");
        }
    }

    internal struct ContactSnapshot
    {
        internal bool Restore;
        internal float Left, Right;
    }

    internal static class JumpPatch
    {
        // WorldToBlockPos uses a one-unit grid in the supplied game assembly.
        private const float UnitsPerTile = 1f;
        private const float VanillaContactGrace = 0.21f;

        internal static void Install(Harmony harmony)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            MethodInfo original = typeof(Body).GetMethod("Jump", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo prefix = typeof(JumpPatch).GetMethod("Prefix", flags);
            MethodInfo finalizer = typeof(JumpPatch).GetMethod("Finalizer", flags);
            if (original == null) throw new MissingMethodException("Body.Jump");
            harmony.Patch(original, prefix: new HarmonyMethod(prefix), finalizer: new HarmonyMethod(finalizer));
            Patches patches = Harmony.GetPatchInfo(original);
            if (patches == null || !Contains(patches.Prefixes, prefix) || !Contains(patches.Finalizers, finalizer))
                throw new InvalidOperationException("Early wall-jump hooks did not register.");
        }

        private static bool Contains(System.Collections.Generic.IEnumerable<Patch> patches, MethodInfo method)
        {
            foreach (Patch patch in patches)
                if (patch.owner == Plugin.Id && patch.PatchMethod == method) return true;
            return false;
        }

        internal static void Prefix(Body __instance, float ___jumpCooldown, bool ___movingAllowed,
            float ___timeSinceGrounded, bool ___firstWallJump, bool ___lastJumpedOnRightWall,
            ref float ___timeSinceSlidLeft, ref float ___timeSinceSlidRight, ref ContactSnapshot __state)
        {
            __state = default(ContactSnapshot);
            try
            {
                if (PlayerCamera.main == null || __instance == null || __instance != PlayerCamera.main.body ||
                    !__instance.standing || __instance.forceWalk || !___movingAllowed || !(___jumpCooldown <= 0f) ||
                    __instance.grounded || __instance.currentClimbable || ___timeSinceGrounded <= 0.11f ||
                    (__instance.bodyAffect && __instance.bodyAffect.wasWater) || !(Plugin.ReachTiles > 0f))
                    return;

                bool vanillaLeft = ___timeSinceSlidLeft < VanillaContactGrace;
                bool vanillaRight = ___timeSinceSlidRight < VanillaContactGrace;
                bool allowLeft = ___firstWallJump || ___lastJumpedOnRightWall;
                bool allowRight = ___firstWallJump || !___lastJumpedOnRightWall;
                // If the original game already accepts this jump, leave its
                // contact grace and side choice completely alone.
                if ((vanillaLeft && allowLeft) || (vanillaRight && allowRight)) return;

                BoxCollider2D collider = __instance.col;
                if (!collider || !collider.enabled) return;
                Bounds bounds = collider.bounds;
                if (!(bounds.size.x > 0f) || !(bounds.size.y > 0f)) return;
                Vector2 size = new Vector2(bounds.size.x, Math.Max(0.01f, bounds.size.y - 0.04f));
                Vector2 origin = bounds.center;
                float reach = Plugin.ReachTiles * UnitsPerTile;
                int groundMask = LayerMask.GetMask("Ground");

                float left = allowLeft ? WallDistance(origin, size, -1, reach, groundMask) : float.PositiveInfinity;
                float right = allowRight ? WallDistance(origin, size, 1, reach, groundMask) : float.PositiveInfinity;
                if (float.IsPositiveInfinity(left) && float.IsPositiveInfinity(right)) return;
                bool selectedRight = right < left;

                __state.Left = ___timeSinceSlidLeft;
                __state.Right = ___timeSinceSlidRight;
                __state.Restore = true;
                // Present one eligible nearby wall to the original method for
                // this call only. Do not change firstWallJump or the last side.
                ___timeSinceSlidLeft = selectedRight ? 1f : 0f;
                ___timeSinceSlidRight = selectedRight ? 0f : 1f;
            }
            catch (Exception exception)
            {
                Restore(ref ___timeSinceSlidLeft, ref ___timeSinceSlidRight, __state);
                __state = default(ContactSnapshot);
                Plugin.Log?.LogError("Early wall proximity check failed: " + exception);
            }
        }

        private static float WallDistance(Vector2 origin, Vector2 size, int side, float reach, int layerMask)
        {
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, new Vector2(side, 0f), reach, layerMask);
            // Vertical inset avoids floor contact. Normal filtering also excludes
            // floors/ceilings, and trigger volumes do not count as walls.
            if (!hit.collider || hit.collider.isTrigger || hit.normal.x * side > -0.5f)
                return float.PositiveInfinity;
            return hit.distance;
        }

        internal static Exception Finalizer(Exception __exception, ref float ___timeSinceSlidLeft,
            ref float ___timeSinceSlidRight, ContactSnapshot __state)
        {
            Restore(ref ___timeSinceSlidLeft, ref ___timeSinceSlidRight, __state);
            if (__exception != null) Plugin.Log?.LogError("Body.Jump failed: " + __exception);
            return __exception;
        }

        private static void Restore(ref float left, ref float right, ContactSnapshot snapshot)
        {
            if (!snapshot.Restore) return;
            left = snapshot.Left;
            right = snapshot.Right;
        }
    }
}
