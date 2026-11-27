using System;
using System.Collections.Generic;
namespace UnityEngine {
 public class RangeAttribute : Attribute { public RangeAttribute(float x, float y) {} }
 public struct Vector2 {
  public float x, y; public Vector2(float x, float y) { this.x=x; this.y=y; }
  public static Vector2 zero => new Vector2(0,0);
  public float magnitude => (float)Math.Sqrt(x*x+y*y);
  public static Vector2 operator *(Vector2 a, float b) => new Vector2(a.x*b,a.y*b);
  public static Vector2 ClampMagnitude(Vector2 a, float b) { float m=a.magnitude; return m>b?a*(b/m):a; }
  public static Vector2 Lerp(Vector2 a,Vector2 b,float t) => new Vector2(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t);
 }
 public struct Vector3 {
  public float x,y,z; public Vector3(float x,float y,float z) { this.x=x;this.y=y;this.z=z; }
  public static Vector3 zero => new Vector3(0,0,0); public static Vector3 forward => new Vector3(0,0,1);
  public float sqrMagnitude => x*x+y*y+z*z;
  public float magnitude => (float)Math.Sqrt(sqrMagnitude);
  public static Vector3 operator +(Vector3 a,Vector3 b) => new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
  public static Vector3 operator *(Vector3 a,float b) => new Vector3(a.x*b,a.y*b,a.z*b);
  public static Vector3 operator -(Vector3 a,Vector3 b) => new Vector3(a.x-b.x,a.y-b.y,a.z-b.z);
  public static Vector3 operator /(Vector3 a,float n) => new Vector3(a.x/n,a.y/n,a.z/n);
  public static Vector3 Lerp(Vector3 a,Vector3 b,float t) => new Vector3(a.x+(b.x-a.x)*t,a.y+(b.y-a.y)*t,a.z+(b.z-a.z)*t);
 }
 public static class Mathf {
  public static float Abs(float a) => Math.Abs(a);
  public static float Max(float a,float b) => Math.Max(a,b); public static int Min(int a,int b) => Math.Min(a,b);
  public static float Clamp(float a,float b,float c) => Math.Max(b,Math.Min(c,a));
  public static int Clamp(int a,int b,int c) => Math.Max(b,Math.Min(c,a));
  public static int RoundToInt(float a) => (int)Math.Round(a);
  public const float PI=(float)Math.PI;
  public static float Clamp01(float a) => Clamp(a,0,1);
  public static float Min(float a,float b) => Math.Min(a,b);
  public static float MoveTowards(float a,float b,float d) => Math.Abs(b-a)<=d?b:a+Math.Sign(b-a)*d;
  public static float Sin(float a) => (float)Math.Sin(a);
  public static float Exp(float a) => (float)Math.Exp(a);
 }
 public class RequireComponent : Attribute { public RequireComponent(Type type) {} }
 public class HeaderAttribute : Attribute { public HeaderAttribute(string text) {} }
 public class MonoBehaviour { public object component; public T GetComponent<T>() { return (T)component; } }
 public enum RigidbodyConstraints { FreezeRotation }
 public enum RigidbodyInterpolation { Interpolate }
 public enum CollisionDetectionMode { ContinuousDynamic }
 public class Rigidbody {
  public bool useGravity; public float mass,linearDamping,angularDamping;
  public RigidbodyConstraints constraints; public RigidbodyInterpolation interpolation;
  public CollisionDetectionMode collisionDetectionMode; public Vector3 linearVelocity,position;
  public void MovePosition(Vector3 p) { position=p; }
 }
 public struct Quaternion { public static Quaternion Euler(float x,float y,float z) => new Quaternion(); public static Quaternion Slerp(Quaternion a,Quaternion b,float t) => a; }
 public class Transform { public Quaternion localRotation; }
 public struct Ray { public Vector3 origin,direction; public Vector3 GetPoint(float n) => origin+direction*n; }
 public struct Plane {
  public Plane(Vector3 normal,Vector3 point) {} 
  public bool Raycast(Ray ray,out float d) { d=-ray.origin.z/ray.direction.z; return d>=0; }
 }
 public class Camera {
  public float halfHeight=7f;
  public Vector3 WorldToViewportPoint(Vector3 p) => new Vector3(0.5f+p.x/16f,0.5f+(p.y-4.5f)/(2*halfHeight),1);
  public Ray ViewportPointToRay(Vector3 p) => new Ray { origin=new Vector3(0,4.5f,-10f), direction=new Vector3((p.x-0.5f)*16f,(p.y-0.5f)*halfHeight*2,10f) };
 }
 public static class Time { public static float time, unscaledTime, deltaTime=0.02f, fixedDeltaTime=0.02f; }
}
namespace UnityEngine.InputSystem {
 public class InputDevice { public bool enabled; public bool added=true; }
 public class Vector3Control { public UnityEngine.Vector3 value; public UnityEngine.Vector3 ReadValue() => value; }
 public class GravitySensor : InputDevice { public static GravitySensor current; public Vector3Control gravity=new Vector3Control(); }
 public class Accelerometer : InputDevice { public static Accelerometer current; public Vector3Control acceleration=new Vector3Control(); }
 public class Gyroscope : InputDevice { public static Gyroscope current; public Vector3Control angularVelocity=new Vector3Control(); }
 public class InputSettings { public bool compensateForScreenOrientation; }
 public static class InputSystem {
  public static InputSettings settings=new InputSettings();
  public static void EnableDevice(InputDevice x) { x.enabled=true; }
  public static void DisableDevice(InputDevice x) { x.enabled=false; }
 }
 public class ButtonControl { public bool isPressed, wasPressedThisFrame; }
 public class Keyboard {
  public static Keyboard current;
  public ButtonControl aKey=new ButtonControl(), leftArrowKey=new ButtonControl(), dKey=new ButtonControl(), rightArrowKey=new ButtonControl(), sKey=new ButtonControl(), downArrowKey=new ButtonControl(), wKey=new ButtonControl(), upArrowKey=new ButtonControl(), spaceKey=new ButtonControl(), leftShiftKey=new ButtonControl(), rightShiftKey=new ButtonControl();
 }
 public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }
}
namespace UnityEngine.InputSystem.EnhancedTouch {
 public static class EnhancedTouchSupport {
  public static int refs;
  public static void Enable() { refs++; }
  public static void Disable() { refs--; }
 }
 public struct Touch {
  public static List<Touch> activeTouches=new List<Touch>();
  public int touchId; public UnityEngine.InputSystem.TouchPhase phase; public double time,startTime;
 }
}
namespace PigeonRooftopRush {
 public enum WindType { None, Ascending, Turbulence }
 public struct EnergyChangedEvent { public int Percent; }
 public struct FlapEvent { public float EnergyAfter; }
 public struct FlapBlockedEvent { }
 public static class EventBus { public static int last; public static void Raise(EnergyChangedEvent e) { last=e.Percent; } public static void Raise<T>(T e) {} }
 public class ManualInput : IFlightInput {
  public UnityEngine.Vector2 steer;
  public UnityEngine.Vector2 Steer => steer; public bool GlideHeld => false; public UnityEngine.Vector3 RawSensor => UnityEngine.Vector3.zero;
  public string DeviceLabel => "manual"; public bool ConsumeFlap() => false; public void Tick() {} public void Calibrate() {} public void InvertAxis(bool horizontal) {} public void SwapAxes() {}
 }
}
class Test {
 static void Check(bool b,string message) { if(!b) throw new Exception(message); }
 static void Near(float a,float b,string msg) { Check(Math.Abs(a-b)<0.001f,msg+" got "+a); }
 static void SetTouch(UnityEngine.InputSystem.TouchPhase phase,float time,double start) {
  UnityEngine.Time.time=time; UnityEngine.Time.unscaledTime=time;
  var all=UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches; all.Clear();
  all.Add(new UnityEngine.InputSystem.EnhancedTouch.Touch { touchId=7,phase=phase,time=time,startTime=start });
 }
 static void ClearTouch(float time) {
  UnityEngine.Time.time=time; UnityEngine.Time.unscaledTime=time;
  UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Clear();
 }
 static void Main() {
  var calibration=new PigeonRooftopRush.SensorCalibration(); calibration.SetNeutral(UnityEngine.Vector3.zero);
  var tiltX=calibration.ToSteer(new UnityEngine.Vector3(0.5f,0,0));
  var tiltXY=calibration.ToSteer(new UnityEngine.Vector3(0.5f,0.8f,0));
  Near(tiltX.x,tiltXY.x,"vertical tilt must not reduce horizontal input");
  Near(calibration.ToSteer(new UnityEngine.Vector3(0.08f,0,0)).x,0,"deadzone rejects small movements");
  Check(calibration.ToSteer(new UnityEngine.Vector3(0.2f,0,0)).x>0,"moderate tilt steers");
  var e=new PigeonRooftopRush.EnergySystem(); e.Reset(); e.SetWind(PigeonRooftopRush.WindType.Turbulence,true); Near(e.Current,75,"red entry");
  e.Tick(3,false); Near(e.Current,75,"red once"); e.SetWind(PigeonRooftopRush.WindType.None,false); e.SetWind(PigeonRooftopRush.WindType.Turbulence,true); Near(e.Current,50,"red reentry");
  e.SetWind(PigeonRooftopRush.WindType.None,false); e.Tick(100,false); Near(e.Current,0,"min zero"); e.SetWind(PigeonRooftopRush.WindType.Turbulence,true); Near(e.Current,0,"red clamp");
  e.SetWind(PigeonRooftopRush.WindType.Ascending,true); Near(e.Current,100,"blue refill"); Check(PigeonRooftopRush.EventBus.last==100,"HUD event");
  var gravity=new UnityEngine.InputSystem.GravitySensor(); gravity.gravity.value=new UnityEngine.Vector3(0,-9.81f,0); UnityEngine.InputSystem.GravitySensor.current=gravity;
  var acceleration=new UnityEngine.InputSystem.Accelerometer(); acceleration.acceleration.value=new UnityEngine.Vector3(0,-1,0); acceleration.enabled=true; UnityEngine.InputSystem.Accelerometer.current=acceleration;
  var gyro=new UnityEngine.InputSystem.Gyroscope(); UnityEngine.InputSystem.Gyroscope.current=gyro;
  var mobile=new PigeonRooftopRush.MobileTiltInput(); Check(gravity.enabled && gyro.enabled,"sensors enabled");
  Check(UnityEngine.InputSystem.InputSystem.settings.compensateForScreenOrientation,"screen orientation");
  SetTouch(UnityEngine.InputSystem.TouchPhase.Began,1,1); mobile.Calibrate(); Check(mobile.DeviceLabel.Contains("Gravidade"),"gravity selected");
  mobile.InvertAxis(true); mobile.InvertAxis(false); mobile.SwapAxes(); mobile.Calibrate();
  Check(mobile.Calibration.invertX && !mobile.Calibration.invertY && mobile.Calibration.swapXY,"recenter preserves corrections");
  Near(mobile.Steer.x,0,"recenter x"); Near(mobile.Steer.y,0,"recenter y");
  mobile.InvertAxis(true); mobile.InvertAxis(false); mobile.SwapAxes();
  mobile.Tick(); SetTouch(UnityEngine.InputSystem.TouchPhase.Ended,1.1f,1); mobile.Tick(); Check(!mobile.ConsumeFlap(),"start touch ignored"); ClearTouch(1.2f); mobile.Tick();
  SetTouch(UnityEngine.InputSystem.TouchPhase.Began,2,2); mobile.Tick(); SetTouch(UnityEngine.InputSystem.TouchPhase.Ended,2.1f,2); mobile.Tick(); ClearTouch(2.12f); mobile.Tick();
  Check(mobile.ConsumeFlap(),"tap queued after update"); Check(!mobile.ConsumeFlap(),"single flap");
  SetTouch(UnityEngine.InputSystem.TouchPhase.Began,3,3); mobile.Tick(); SetTouch(UnityEngine.InputSystem.TouchPhase.Stationary,3.3f,3); mobile.Tick(); Check(mobile.GlideHeld,"hold glide");
  SetTouch(UnityEngine.InputSystem.TouchPhase.Ended,3.4f,3); mobile.Tick(); Check(!mobile.ConsumeFlap(),"hold no flap");
  SetTouch(UnityEngine.InputSystem.TouchPhase.Began,4,4); mobile.Tick(); SetTouch(UnityEngine.InputSystem.TouchPhase.Canceled,4.1f,4); mobile.Tick(); Check(!mobile.ConsumeFlap(),"canceled no flap");
  ClearTouch(5); gravity.gravity.value=new UnityEngine.Vector3(0,-1,0); mobile.Calibrate(); UnityEngine.Time.time=6; gravity.gravity.value=new UnityEngine.Vector3(0.5f,-1,0); mobile.Tick(); Check(mobile.Steer.x>0,"inverted default gravity steering");
  gravity.gravity.value=UnityEngine.Vector3.zero; mobile.Calibrate(); Check(mobile.DeviceLabel.Contains("Acelerômetro"),"accelerometer fallback"); mobile.Tick(); Near(mobile.Steer.x,0,"switch no jump");
  acceleration.acceleration.value=UnityEngine.Vector3.zero; mobile.Calibrate(); Check(!mobile.Calibration.IsSet,"wait for valid sensor"); mobile.Tick(); Near(mobile.Steer.x,0,"missing sensor neutral");
  acceleration.acceleration.value=new UnityEngine.Vector3(0,-1,0); mobile.Tick(); Check(mobile.Calibration.IsSet,"late sensor calibrated"); Near(mobile.Steer.x,0,"late sensor no jump");
  mobile.Dispose(); mobile.Dispose(); Check(!gravity.enabled && !gyro.enabled && acceleration.enabled,"owned sensors only"); Check(UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.refs==0,"touch disabled once");
  var keyboard=new UnityEngine.InputSystem.Keyboard(); UnityEngine.InputSystem.Keyboard.current=keyboard; var editor=new PigeonRooftopRush.EditorInput(); keyboard.dKey.isPressed=true; keyboard.spaceKey.wasPressedThisFrame=true; keyboard.leftShiftKey.isPressed=true; editor.Tick();
  Check(editor.Steer.x>0 && editor.GlideHeld && editor.ConsumeFlap(),"editor keyboard");
  keyboard.spaceKey.wasPressedThisFrame=false; keyboard.wKey.isPressed=true; editor.Tick(); Near(editor.Steer.x,1,"diagonal keyboard preserves x"); Check(!editor.ConsumeFlap(),"editor one flap");
  UnityEngine.InputSystem.Keyboard.current=null; editor.Tick(); Near(editor.Steer.x,0,"no keyboard"); Check(!editor.GlideHeld,"no keyboard glide");
  var flight=new PigeonRooftopRush.PigeonFlightController(); var rb=new UnityEngine.Rigidbody(); rb.position=new UnityEngine.Vector3(0,6,0); flight.component=rb;
  var manual=new PigeonRooftopRush.ManualInput(); manual.steer=new UnityEngine.Vector2(0.5f,9.5f/26f); flight.InputSource=manual; flight.FlightCamera=new UnityEngine.Camera(); flight.ControlsEnabled=true;
  var awake=typeof(PigeonRooftopRush.PigeonFlightController).GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  var fixedUpdate=typeof(PigeonRooftopRush.PigeonFlightController).GetMethod("FixedUpdate",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  awake.Invoke(flight,null);
  for(int i=0;i<10;i++) fixedUpdate.Invoke(flight,null);
  float heldX=rb.position.x; Near(heldX,1f,"lateral response speed");
  for(int i=0;i<10;i++) fixedUpdate.Invoke(flight,null);
  Check(rb.position.x>heldX,"holding tilt continues moving");
  float stoppedX=rb.position.x;
  manual.steer=new UnityEngine.Vector2(0,9.5f/26f);
  for(int i=0;i<100;i++) fixedUpdate.Invoke(flight,null);
  Near(rb.position.x,stoppedX,"neutral stops without recentering");
  manual.steer=new UnityEngine.Vector2(1,9.5f/26f);
  for(int i=0;i<100;i++) fixedUpdate.Invoke(flight,null);
  Near(rb.position.x,flight.boundsX,"lateral bound");
  rb.position=new UnityEngine.Vector3(0,20,0); fixedUpdate.Invoke(flight,null); Near(rb.position.y,10.52f,"camera upper boundary");
  flight.FlightCamera.halfHeight=9; rb.position=new UnityEngine.Vector3(0,20,0); fixedUpdate.Invoke(flight,null); Near(rb.position.y,12.24f,"boundary follows field of view");
  rb.position=new UnityEngine.Vector3(0,0.601f,0); manual.steer=new UnityEngine.Vector2(0,-1); fixedUpdate.Invoke(flight,null); Check(flight.GroundTouched,"ground still crashes");
  Console.WriteLine("OK: energia, entrada e voo com stubs Unity (sem hardware).");
 }
}
