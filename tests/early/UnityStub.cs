using System;
namespace UnityEngine
{
 public class Object {
  private static int nextId; public int NativeId=++nextId;
  public static bool operator ==(Object a,Object b) => ReferenceEquals(a,b) || (!ReferenceEquals(a,null) && !ReferenceEquals(b,null) && a.NativeId==b.NativeId);
  public static bool operator !=(Object a,Object b) => !(a==b);
  public static implicit operator bool(Object a) => !ReferenceEquals(a,null);
  public override bool Equals(object other) => this==other as Object;
  public override int GetHashCode() => NativeId;
 }
 public class Component : Object { }
 public class Behaviour : Component { public bool enabled {get;set;}=true; }
 public class MonoBehaviour : Behaviour { }
 public struct Vector2 {
  public float x,y;
  public Vector2(float x,float y) {this.x=x;this.y=y;}
  public static implicit operator Vector2(Vector3 v) => new Vector2(v.x,v.y);
 }
 public struct Vector3 {
  public float x,y,z;
  public Vector3(float x,float y,float z) {this.x=x;this.y=y;this.z=z;}
 }
 public struct Bounds {
  public Vector3 center {get;set;}
  public Vector3 size {get;set;}
  public Bounds(Vector3 center,Vector3 size) {this.center=center;this.size=size;}
 }
 public struct LayerMask {
  public static int GetMask(params string[] names) => names.Length==1 && names[0]=="Ground"?1:0;
 }
}
