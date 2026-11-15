using System;
namespace UnityEngine {
 public class Object {
  private static int nextId;
  public int NativeId=++nextId;
  public static bool operator ==(Object a,Object b) => ReferenceEquals(a,b) || (!ReferenceEquals(a,null) && !ReferenceEquals(b,null) && a.NativeId==b.NativeId);
  public static bool operator !=(Object a,Object b) => !(a==b);
  public static implicit operator bool(Object a) => !ReferenceEquals(a,null);
  public override bool Equals(object other) => ReferenceEquals(this,other);
  public override int GetHashCode() => base.GetHashCode();
 }
 public class Component : Object { }
 public class Behaviour : Component { }
 public class MonoBehaviour : Behaviour { }
}
