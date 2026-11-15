using System.Runtime.CompilerServices;
using UnityEngine;
public class PlayerCamera:MonoBehaviour {public static PlayerCamera main; public Body body;}
public class Climbable:MonoBehaviour { }
public class LiquidAffect:MonoBehaviour {public bool wasWater;}
public class Body:MonoBehaviour {
 public bool grounded,standing=true,forceWalk,movingAllowed=true;
 public BoxCollider2D col=new BoxCollider2D();
 public Climbable currentClimbable; public LiquidAffect bodyAffect=new LiquidAffect();
 public float jumpCooldown,timeSinceGrounded=1;
 private bool firstWallJump=true,lastJumpedOnRightWall;
 private float timeSinceSlidLeft=100,timeSinceSlidRight=100;
 public Vector2 moveDir; public int Successes,Direction; public bool Claws,ThrowOnJump;
 private bool canWalljumpLeft=>timeSinceSlidLeft<.21f;
 private bool canWalljumpRight=>timeSinceSlidRight<.21f;
 [MethodImpl(MethodImplOptions.NoInlining)] public void Jump() {
  if(!standing || !(jumpCooldown<=0) || forceWalk || !movingAllowed) return;
  if(ThrowOnJump) throw new System.InvalidOperationException("Simulated original jump failure");
  if(grounded || currentClimbable || timeSinceGrounded<=.11f || bodyAffect.wasWater) {jumpCooldown=.25f;grounded=false;return;}
  if((!canWalljumpLeft && !canWalljumpRight) || (!firstWallJump && (!canWalljumpLeft || !lastJumpedOnRightWall) && (!canWalljumpRight || lastJumpedOnRightWall)))return;
  Direction=canWalljumpLeft?1:-1;
  if(canWalljumpRight)lastJumpedOnRightWall=true;else lastJumpedOnRightWall=false;
  firstWallJump=Claws; jumpCooldown=.25f; Successes++;
 }
 public void TryJump(){jumpCooldown=0;Jump();}
 public void Contact(int side){timeSinceSlidLeft=side<0?0:100;timeSinceSlidRight=side>0?0:100;}
}
