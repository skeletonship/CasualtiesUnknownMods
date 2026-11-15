using System.Runtime.CompilerServices;
using UnityEngine;
public class PlayerCamera : MonoBehaviour { public static PlayerCamera main; public Body body; }
public class Climbable : MonoBehaviour { }
public class LiquidAffect : MonoBehaviour { public bool wasWater; }
public class Body : MonoBehaviour {
 public bool grounded, standing=true, forceWalk, movingAllowed=true;
 public Climbable currentClimbable;
 public LiquidAffect bodyAffect=new LiquidAffect();
 public float jumpCooldown, timeSinceGrounded=1;
 private bool firstWallJump=true, lastJumpedOnRightWall, slidingLeft, slidingRight;
 private float timeSinceSlidRight=100, timeSinceSlidLeft=100;
 private bool canWalljumpLeft => timeSinceSlidLeft < .21f;
 private bool canWalljumpRight => timeSinceSlidRight < .21f;
 public int Successes; public bool Claws, ThrowOnJump;
 [MethodImpl(MethodImplOptions.NoInlining)]
 public void Jump() {
  if(!standing || !(jumpCooldown<=0) || forceWalk || !movingAllowed) return;
  if(ThrowOnJump) throw new System.InvalidOperationException("Simulated original game jump error");
  if(grounded || currentClimbable || timeSinceGrounded<=.11f || bodyAffect.wasWater) { jumpCooldown=.25f; grounded=false; return; }
  if((!canWalljumpLeft && !canWalljumpRight) || (!firstWallJump && (!canWalljumpLeft || !lastJumpedOnRightWall) && (!canWalljumpRight || lastJumpedOnRightWall))) return;
  if(canWalljumpRight) lastJumpedOnRightWall=true; else lastJumpedOnRightWall=false;
  firstWallJump=Claws;
  jumpCooldown=.25f;
  Successes++;
  slidingLeft=false; slidingRight=false;
 }
 [MethodImpl(MethodImplOptions.NoInlining)]
 private void HandleGroundedState() { }
 public void Contact(int side) {
  slidingLeft=side<0; slidingRight=side>0;
  timeSinceSlidLeft=side<0?0:100; timeSinceSlidRight=side>0?0:100;
  HandleGroundedState();
 }
 public void TryJump() { jumpCooldown=0; Jump(); }
}
