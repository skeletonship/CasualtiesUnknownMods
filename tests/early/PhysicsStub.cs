using System;
namespace UnityEngine {
 public class Collider2D:Behaviour {
  public Bounds bounds {get;set;}=new Bounds(new Vector3(0,0,0),new Vector3(.5f,2f,0));
  public bool isTrigger {get;set;}
 }
 public class BoxCollider2D:Collider2D { }
 public struct RaycastHit2D { public Collider2D collider {get;set;} public Vector2 normal {get;set;} public float distance {get;set;} }
 public static class Physics2D {
  public static float LeftGap=float.PositiveInfinity,RightGap=float.PositiveInfinity;
  public static bool FloorNormal,Trigger,ThrowOnCast;
  public static int Queries,LastMask; public static float LastReach; public static Vector2 LastSize;
  public static void Reset() {LeftGap=RightGap=float.PositiveInfinity;FloorNormal=Trigger=ThrowOnCast=false;Queries=0;}
  public static RaycastHit2D BoxCast(Vector2 origin,Vector2 size,float angle,Vector2 direction,float distance,int layerMask) {
   Queries++; LastMask=layerMask; LastReach=distance; LastSize=size;
   if(ThrowOnCast) throw new InvalidOperationException("Simulated physics failure");
   float gap=direction.x<0?LeftGap:RightGap;
   if(gap>distance || layerMask!=1) return default(RaycastHit2D);
   return new RaycastHit2D { collider=new Collider2D {isTrigger=Trigger},normal=FloorNormal?new Vector2(0,1):new Vector2(-direction.x,0),distance=gap};
  }
 }
}
