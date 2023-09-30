//  ------ United Godot ------ 
//  teadrinker / Martin Eklund
//  License: MIT 

#if UNITY_2017_1_OR_NEWER
 
namespace Godot 
{
	public enum PropertyHint:long
	{
		None=0,Range=1,Enum=2,EnumSuggestion=3,ExpEasing=4,Link=5,Flags=6,Layers2DRender=7,Layers2DPhysics=8,Layers2DNavigation=9,
		Layers3DRender=10,Layers3DPhysics=11,Layers3DNavigation=12,File=13,Dir=14,GlobalFile=15,GlobalDir=16,ResourceType=17,
		MultilineText=18,Expression=19,PlaceholderText=20,ColorNoAlpha=21,ObjectId=22,TypeString=23,NodePathToEditedNode=24,
		ObjectTooBig=25,NodePathValidTypes=26,SaveFile=27,GlobalSaveFile=28,IntIsObjectid=29,IntIsPointer=30,ArrayType=31,
		LocaleId=32,LocalizableString=33,NodeType=34,HideQuaternionEdit=35,Password=36,LayersAvoidance=37,Max=38
	}

	[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
	public sealed class ExportAttribute : System.Attribute
	{
		public ExportAttribute(PropertyHint hint = PropertyHint.None, string hintString = "") {		
			_hint = hint;
			_hintString = hintString;
		}
		private PropertyHint _hint;
		private string _hintString;
		public PropertyHint Hint { get { return _hint; } }
		public string HintString { get { return _hintString; } }
	}

}


namespace UnitedGodot {
	public static class Global {
		public const bool isGodot = false;
	}
}

#else
 
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace UnitedGodot {
	public static class Global {
		public const bool isGodot = true;
	}
}

namespace UnityEngine
{




	partial class Resources { 
		public static string UnitedGodotResourceRoot = "res://UnitedGodotProject/Resources/"; 
		public static string UnitedGodotTextureExtension = ".jpg"; 
		public static string UnitedGodotMeshExtension = ".obj"; 
	}





	public static class Debug {
		public static void Log(string msg) { GD.Print(msg); }		
		public static void LogWarning(string msg) { GD.Print("WARNING: "+msg); }		
		public static void LogError(string msg) { GD.Print("ERROR: "+msg); }		
	}
	public class Object { }

	public interface IComponent {
		bool enabled { get; set; }
		GameObject gameObject { get; }
		void InternalSetGameObject(GameObject go);
		void Init();
		void OnComponentEnable();
		void OnComponentDisable();
	}

	public class Component : Object, IComponent {
		public bool enabled { get { return _enabled; } set { if(value != _enabled) { _enabled = value; if(_enabled) OnComponentEnable(); else OnComponentDisable(); } } }
		private bool _enabled = true;
		public GameObject gameObject { get { return _gameObject; } }
		private GameObject _gameObject;
		public void InternalSetGameObject(GameObject go) { _gameObject = go; }
		public virtual void OnComponentEnable() {}
		public virtual void OnComponentDisable() {}
		public virtual void Init() {}
	}	


  
	
	public class GameObject : Object {
		
		public Node3D godot; // note: this may be created/handled by GameObject, or just be a reference (if Node3D is sent through constructor)

		public string name = ""; //  note: any code setting "name = null" will break, Unity converts null to ""
		public Transform transform;
		public MeshRenderer meshRenderer; // can only be one per gameObject 
		public MeshFilter meshFilter; // can only be one per gameObject 		
		private Dictionary<Type, List<IComponent>> _components = new(); 

		// public GameObject(string name, params Type[] components); // todo?
		public GameObject(string _name = null, Node3D parent = null) {
			name = _name ?? "";
			CreateAndInitComponent(ref transform); // GameObjects always have transform
			if(parent == null)
				godot = new Node3D();
			else
				godot = parent;
		}

		public void ValidateMesh() {
			if(meshRenderer != null && meshFilter != null) {
				if(meshRenderer.sharedMaterial != null)
					meshRenderer.godotMeshInstance3D.MaterialOverride = meshRenderer.sharedMaterial.godot;
				meshRenderer.godotMeshInstance3D.Mesh = meshFilter.sharedMesh?.GetGodotMesh();
			}
		}

		private T CreateAndInitComponent<T>(ref T instanceVar) where T : Component, new() {
			if(instanceVar == null)
			{
				instanceVar = new T();
				instanceVar.InternalSetGameObject(this);
				instanceVar.Init();
			}
			else 
				Debug.LogWarning("Component already added! only single component allowed of type: "+typeof(T).FullName);
			return (T) instanceVar;
		}

		public T[] GetComponents<T>() where T : class, IComponent, new() {
			if(     typeof(T) == typeof(MeshRenderer)) return new T[] { (T) (IComponent) meshRenderer};
			else if(typeof(T) == typeof(MeshFilter  )) return new T[] { (T) (IComponent) meshFilter};
			else if(typeof(T) == typeof(Transform   )) return new T[] { (T) (IComponent) transform};
			else {
				if(_components.TryGetValue(typeof(T), out var list)) 
					return list.Select(x => (T) x).ToArray();
			}
			return null;
		}

		public T GetComponent<T>() where T : class, IComponent, new() {
			if(     typeof(T) == typeof(MeshRenderer)) return (T) (IComponent) meshRenderer;
			else if(typeof(T) == typeof(MeshFilter  )) return (T) (IComponent) meshFilter;
			else if(typeof(T) == typeof(Transform   )) return (T) (IComponent) transform;
			else {
				if(_components.TryGetValue(typeof(T), out var list)) 
					return (T) list[0];
			}
			return null;
		}

		public T AddComponent<T>() where T : class, IComponent, new() {
			
			if     (typeof(T) == typeof(MeshRenderer)) return (T) (IComponent) CreateAndInitComponent(ref meshRenderer);
			else if(typeof(T) == typeof(MeshFilter  )) return (T) (IComponent) CreateAndInitComponent(ref meshFilter);	
			else if(typeof(T) == typeof(Transform   )) return (T) (IComponent) CreateAndInitComponent(ref transform); // this will always fail, since added in constructor, but the error msg is good...	

			var c = new T();
			c.InternalSetGameObject(this);
			c.Init();
			if(_components.TryGetValue(typeof(T), out var list)) 
				list.Add(c);				
			else 
			{
				list = new List<IComponent>() { c };
				_components.Add(typeof(T), list);
			}
			return c;
		}		
	}	





	public partial class MonoBehaviour : Node, IComponent {

		public T GetComponent<T>() where T : Component, new() { return gameObject.GetComponent<T>(); }	
		public Transform transform { get { return gameObject.transform; } }
		
		// IComponent
		public bool enabled { get { return _enabled; } set { if(value != _enabled) { _enabled = value; if(_enabled) OnComponentEnable(); else OnComponentDisable(); } } }
		private bool _enabled = true;
		public GameObject gameObject { get { return _gameObject; } }
		private GameObject _gameObject;
		public void InternalSetGameObject(GameObject go) { _gameObject = go; }
		public virtual void OnComponentEnable () { _onEnable ?.Invoke(); }
		public virtual void OnComponentDisable() { _onDisable?.Invoke(); }
		public virtual void Init() {
			//Debug.LogWarning("init " + this.GetType().FullName + " " + (GetParent() == null ? "null" : GetParent().Name));
			if(GetParent() == null)
				gameObject.godot.CallDeferred("add_child", this);
		}

		public double _localTime = 0.0;

		private System.Action _awake;
		private System.Action _start;
		private System.Action _onEnable;
		private System.Action _update;
		private System.Action _onDisable;
		private System.Action _onDestroy;
		public override void _Ready()
		{
			base._Ready(); // needed?

			var parentNode3D = GetParent() as Node3D;
			var parentGO = parentNode3D as UnitedGodot.UGGameObject;
			if(parentNode3D == null && parentGO == null) {
				Debug.LogError("MonoBehaviour "+this.GetType().FullName+" has no parent Node3D or GameObject!");
				return;
			} 
			else if(parentGO != null) 
			{
				InternalSetGameObject(parentGO.InnerGameObject);
				if(gameObject == null)
					Debug.LogError("MonoBehaviour "+this.GetType().FullName+" has no parent Node3D or GameObject (2)!");
			}
			else 
			{
				InternalSetGameObject(UnitedGodot.UGGameObject.FromNode3D(parentNode3D));
				if(gameObject == null)
					Debug.LogError("MonoBehaviour "+this.GetType().FullName+" has no parent Node3D or GameObject (3)!");
			}
			
			Type type = this.GetType();
			System.Reflection.MethodInfo[] methods = type.GetMethods(
					System.Reflection.BindingFlags.DeclaredOnly | 
					System.Reflection.BindingFlags.NonPublic | 
					System.Reflection.BindingFlags.Public | 
					System.Reflection.BindingFlags.Instance);

			foreach (System.Reflection.MethodInfo method in methods)
			{
				if     (method.Name == "Awake"    ) _awake     = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
				else if(method.Name == "Start"    ) _start     = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
				else if(method.Name == "OnEnable" ) _onEnable  = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
				else if(method.Name == "Update"   ) _update    = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
				else if(method.Name == "OnDisable") _onDisable = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
				else if(method.Name == "OnDestroy") _onDestroy = (System.Action)Delegate.CreateDelegate(typeof(System.Action), this, method);
			}

			if(_awake == null && _start == null && _onEnable == null && _update == null && _onDisable == null && _onDestroy == null)
				Debug.LogWarning("MonoBehaviour "+type.FullName+" has no methods to call!");

			_awake?.Invoke();
			_start?.Invoke();
			_onEnable?.Invoke();
		}
		public override void _ExitTree()
		{
			_onDisable?.Invoke();
			_onDestroy?.Invoke();

			base._ExitTree(); // needed?
		}
				
		public override void _Process(double delta) 
		{
			base._Process(delta); // needed?

			if(_update != null && enabled) {
				Time.deltaTime = (float)delta;
				_localTime += delta;
				Time.time = (float)_localTime;
				_update?.Invoke();				
			}
		}
	}




	public class MeshFilter : Component {
		public Mesh mesh       { set { _sharedMesh = value; gameObject.ValidateMesh(); } get {return _sharedMesh;} } // incomplete: this should probably copy the mesh
		public Mesh sharedMesh { set { _sharedMesh = value; gameObject.ValidateMesh(); } get {return _sharedMesh;} }
		private Mesh _sharedMesh;
	}

	  
	public class MeshRenderer : Component {
		//public Material[] materials;
		//public Material[] sharedMaterials;
		public Material material       { set { _sharedMaterial = value; gameObject.ValidateMesh(); } get {return _sharedMaterial;} } // incomplete: this should probably copy the material
		public Material sharedMaterial { set { _sharedMaterial = value; gameObject.ValidateMesh(); } get {return _sharedMaterial;} }
		private Material _sharedMaterial;
	
		public MeshInstance3D godotMeshInstance3D;
		public override void Init() {
			//Debug.LogError("gameObject.godot t "+(gameObject.godot.GetType().FullName));
			godotMeshInstance3D = new MeshInstance3D();
			gameObject.ValidateMesh();
			//gameObject.godot.AddChild(godotMeshInstance3D);
			gameObject.godot.CallDeferred("add_child", godotMeshInstance3D); // apparently not allowed inside notifications? (process, input, ready) 
		} 
		//private void InitLater() {  }		
		public override void OnComponentDisable() {
			godotMeshInstance3D.Visible = false;
		}
		public override void OnComponentEnable() {
			godotMeshInstance3D.Visible = true;
		}
	}

	public class Texture : Object {
		public virtual Godot.Texture godotTex { get { return null; } }
	}
	public class Texture2D : Texture {
		public int width { get { return godot.GetWidth(); } }
		public int height { get { return godot.GetHeight(); } }
		public override Godot.Texture godotTex { get { return godot; } }
		public Godot.Texture2D godot;
	}

	public class Material {

		public Godot.ShaderMaterial godot;		
		
		public Material(Shader s) { shader = s; godot = new Godot.ShaderMaterial(); godot.Shader = shader.godot; }		
		public Shader shader;
		public Color color { set { godot.SetShaderParameter("_Color", value.godot); } }
		public Texture2D mainTexture { set { godot.SetShaderParameter("_MainTex", value.godotTex); } } // Getting textures don't actually work with Unity on IOS, so don't implement!
		public void SetTexture(string name, Texture2D tex) { godot.SetShaderParameter(name, tex.godot); }
		public void SetFloat  (string name, float   value) { godot.SetShaderParameter(name, value); }
		public void SetColor  (string name, Color   value) { godot.SetShaderParameter(name, value.godot); }
		public void SetVector (string name, Vector4 value) { godot.SetShaderParameter(name, value.godot); }
	}
	
	public class Mesh : Object {
		public int[] triangles {
			set {                                           _meshArrays[(int)Godot.Mesh.ArrayType.Index] = value; }
			get { return (int[])_meshArrays[(int)Godot.Mesh.ArrayType.Index]; }
		}
		public Vector3[] vertices {
			set {                                           _meshArrays[(int)Godot.Mesh.ArrayType.Vertex] = Vector3.toGodot(value); }
			get { return Vector3.fromGodot((Godot.Vector3[])_meshArrays[(int)Godot.Mesh.ArrayType.Vertex]); }
		}
		public Vector2[] uv {
			set {                                           _meshArrays[(int)Godot.Mesh.ArrayType.TexUV] = Vector2.toGodot(value); }
			get { return Vector2.fromGodot((Godot.Vector2[])_meshArrays[(int)Godot.Mesh.ArrayType.TexUV]); }
		}
		
		public Mesh() {
			_godot = new ArrayMesh();
			_meshArrays = new Godot.Collections.Array();
			_meshArrays.Resize((int)Godot.Mesh.ArrayType.Max);
		}
		
		public void SetGodotMesh(Godot.Mesh m) {
			if(m == null) {
				Debug.LogWarning("SetGodotMesh() mesh reference was null... ignored");
				return;
			}
			_godotMesh = m;
			Valid = true;
		}
			
		public Godot.Mesh GetGodotMesh() {
			if(!Valid) {
				_godot.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, _meshArrays);	
				_godotMesh = _godot;
				
				/*
				var vertices = new Godot.Vector3[]
				{
					new Godot.Vector3(0, 1, 0),
					new Godot.Vector3(1, 0, 0),
					new Godot.Vector3(0, 0, 1),

					new Godot.Vector3(1, 0, 0),
					new Godot.Vector3(0, 1, 0),
					new Godot.Vector3(0, 0, 1),
				};

				// Initialize the ArrayMesh.
				var arrMesh = new ArrayMesh();
				var arrays = new Godot.Collections.Array();
				arrays.Resize((int)Godot.Mesh.ArrayType.Max);
				arrays[(int)Godot.Mesh.ArrayType.Vertex] = vertices;

				// Create the Mesh.
				arrMesh.AddSurfaceFromArrays(Godot.Mesh.PrimitiveType.Triangles, arrays);
				_godot = arrMesh;
				_godotMesh = _godot;				
				*/	
								
				Valid = true;
			}
			return _godotMesh;			
		}
		public void RecalculateNormals() {
			Debug.LogError("Mesh.RecalculateNormals not implemented!");
			//_godot.GenerateNormals();
		}	
		private Godot.Collections.Array _meshArrays;
		private ArrayMesh _godot;
		private Godot.Mesh _godotMesh;
		private bool Valid = false;
	}
	
	public class Shader : Object {
		public Godot.Shader godot;
	}
	
	public static partial class Resources {
		public static T Load<T>(string name) where T: Object {
			if(typeof(T) == typeof(Mesh))
			{
				var m = new Mesh();
				m.SetGodotMesh(GD.Load<Godot.Mesh>(UnitedGodotResourceRoot + name + UnitedGodotMeshExtension));
				return (T) (Object) m;
			}
			else if(typeof(T) == typeof(Texture2D))
				return (T) (Object) new Texture2D() { godot = GD.Load<Godot.Texture2D>(UnitedGodotResourceRoot + name + UnitedGodotTextureExtension) };
			else if(typeof(T) == typeof(Shader)) {
				var shader = GD.Load<Godot.Shader>(UnitedGodotResourceRoot + name + ".gdshader");
				return (T) (Object) new Shader() { godot = shader };				
			}
			Debug.LogError("Resources.Load<"+typeof(T).FullName+"> not implemented!");
			return null;
		}
	}
	
	public static class Time {
		public static float deltaTime;
		public static float time;
	}

	public struct Vector2 {

		public Godot.Vector2 godot;

		public static Vector2 one = new Vector2(1f, 1f);		
		public static Vector2 zero = new Vector2(0f, 0f);		
		public static Vector2[] fromGodot(Godot.Vector2[] data) {
			var ret = new Vector2[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = new Vector2(data[i].X, data[i].Y);
			}
			return ret;
		}
		public static Godot.Vector2[] toGodot(Vector2[] data) {
			var ret = new Godot.Vector2[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = data[i].godot;
			}
			return ret;
		}		
		public float x { get { return godot.X; } set { godot.X = value; }  }
		public float y { get { return godot.Y; } set { godot.Y = value; }  }		
		public Vector2(float _x, float _y) {
			godot.X = _x;
			godot.Y = _y;
		}		

		// mostly untested! (autocompleted by github copilot)		
		public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x+b.x, a.y+b.y); }
		public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x-b.x, a.y-b.y); }
		public static Vector2 operator *(Vector2 a, Vector2 b) { return new Vector2(a.x*b.x, a.y*b.y); }
		public static Vector2 operator /(Vector2 a, Vector2 b) { return new Vector2(a.x/b.x, a.y/b.y); }
		public static Vector2 operator *(Vector2 a, float b) { return new Vector2(a.x*b, a.y*b); }
		public static Vector2 operator /(Vector2 a, float b) { return new Vector2(a.x/b, a.y/b); }
		public static Vector2 operator *(float b, Vector2 a) { return new Vector2(a.x*b, a.y*b); }
		public static Vector2 operator /(float b, Vector2 a) { return new Vector2(a.x/b, a.y/b); }
		public static Vector2 Lerp(Vector2 a, Vector2 b, float t) { return new Vector2(
			Mathf.Lerp(a.x, b.x, t),
			Mathf.Lerp(a.y, b.y, t) );
		 }
		public static float Dot(Vector2 a, Vector2 b) { return a.x*b.x + a.y*b.y; }
		public static float Distance(Vector2 a, Vector2 b) { return Mathf.Sqrt((a.x-b.x)*(a.x-b.x) + (a.y-b.y)*(a.y-b.y)); }
		public static Vector2 Normalize(Vector2 a) { return a / a.magnitude; }
		public float magnitude { get { return Mathf.Sqrt(x*x + y*y); } }
		public float sqrMagnitude { get { return x*x + y*y; } }
		public static Vector2 Scale(Vector2 a, Vector2 b) { return new Vector2(a.x*b.x, a.y*b.y); }
		public static Vector2 Min(Vector2 a, Vector2 b) { return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y)); }
		public static Vector2 Max(Vector2 a, Vector2 b) { return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y)); }
		public static Vector2 Project(Vector2 a, Vector2 b) { return b * (Dot(a, b) / Dot(b, b)); }
		public static Vector2 ProjectOnPlane(Vector2 a, Vector2 b) { return a - Project(a, b); }
		public static Vector2 Reflect(Vector2 a, Vector2 b) { return a - 2f * Project(a, b); }
		public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t) { return new Vector2(
			Mathf.LerpUnclamped(a.x, b.x, t),
			Mathf.LerpUnclamped(a.y, b.y, t) );
		 }
	}

	public struct Vector3 {

		public Godot.Vector3 godot;

		public static Vector3 one = new Vector3(1f, 1f, 1f);		
		public static Vector3 zero = new Vector3(0f, 0f, 0f);		
		public static Vector3 back = new Vector3(0f, 0f, -1f);		
		public static Vector3 forward = new Vector3(0f, 0f, 1f);		
		public static Vector3 up = new Vector3(0f, 1f, 0f);
		public static Vector3 down = new Vector3(0f, -1f, 0f);
		public static Vector3 left = new Vector3(-1f, 0f, 0f);		
		public static Vector3 right = new Vector3(1f, 0f, 0f);		
		public static Vector3[] fromGodot(Godot.Vector3[] data) {
			var ret = new Vector3[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = new Vector3(data[i].X, data[i].Y, data[i].Z);
			}
			return ret;
		}
		public static Godot.Vector3[] toGodot(Vector3[] data) {
			var ret = new Godot.Vector3[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = data[i].godot;
			}
			return ret;
		}
		public float x { get { return godot.X; } set { godot.X = value; }  }
		public float y { get { return godot.Y; } set { godot.Y = value; }  }
		public float z { get { return godot.Z; } set { godot.Z = value; }  }
		public Vector3(float _x, float _y, float _z) {
			godot.X = _x;
			godot.Y = _y;
			godot.Z = _z;
		}

		// mostly untested! (autocompleted by github copilot)
		public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x+b.x, a.y+b.y, a.z+b.z); }
		public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x-b.x, a.y-b.y, a.z-b.z); }
		public static Vector3 operator *(Vector3 a, Vector3 b) { return new Vector3(a.x*b.x, a.y*b.y, a.z*b.z); }
		public static Vector3 operator /(Vector3 a, Vector3 b) { return new Vector3(a.x/b.x, a.y/b.y, a.z/b.z); }
		public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x*b, a.y*b, a.z*b); }
		public static Vector3 operator /(Vector3 a, float b) { return new Vector3(a.x/b, a.y/b, a.z/b); }
		public static Vector3 operator *(float b, Vector3 a) { return new Vector3(a.x*b, a.y*b, a.z*b); }
		public static Vector3 operator /(float b, Vector3 a) { return new Vector3(a.x/b, a.y/b, a.z/b); }
		public static Vector3 Lerp(Vector3 a, Vector3 b, float t) { return new Vector3(
			Mathf.Lerp(a.x, b.x, t),
			Mathf.Lerp(a.y, b.y, t),
			Mathf.Lerp(a.z, b.z, t) );
		 }
		public static Vector3 Cross(Vector3 a, Vector3 b) { return new Vector3(
			a.y*b.z - a.z*b.y,
			a.z*b.x - a.x*b.z,
			a.x*b.y - a.y*b.x );
		 }
		public static float Dot(Vector3 a, Vector3 b) { return a.x*b.x + a.y*b.y + a.z*b.z; }
		public static float Distance(Vector3 a, Vector3 b) { return Mathf.Sqrt((a.x-b.x)*(a.x-b.x) + (a.y-b.y)*(a.y-b.y) + (a.z-b.z)*(a.z-b.z)); }
		public static Vector3 Normalize(Vector3 a) { return a / a.magnitude; }
		public float magnitude { get { return Mathf.Sqrt(x*x + y*y + z*z); } }
		public float sqrMagnitude { get { return x*x + y*y + z*z; } }
		public static Vector3 Scale(Vector3 a, Vector3 b) { return new Vector3(a.x*b.x, a.y*b.y, a.z*b.z); }
		public static Vector3 Min(Vector3 a, Vector3 b) { return new Vector3(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z)); }
		public static Vector3 Max(Vector3 a, Vector3 b) { return new Vector3(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z)); }
		public static Vector3 Project(Vector3 a, Vector3 b) { return b * (Dot(a, b) / Dot(b, b)); }
		public static Vector3 ProjectOnPlane(Vector3 a, Vector3 b) { return a - Project(a, b); }
		public static Vector3 Reflect(Vector3 a, Vector3 b) { return a - 2f * Project(a, b); }
		public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t) { return new Vector3(
			Mathf.LerpUnclamped(a.x, b.x, t),
			Mathf.LerpUnclamped(a.y, b.y, t),
			Mathf.LerpUnclamped(a.z, b.z, t) );
		 }
	}

	public struct Vector4 {

		public Godot.Vector4 godot;

		public static Vector4 one = new Vector4(1f, 1f, 1f, 1f);		
		public static Vector4 zero = new Vector4(0f, 0f, 0f, 0f);	
		public static Vector4[] fromGodot(Godot.Vector4[] data) {
			var ret = new Vector4[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = new Vector4(data[i].X, data[i].Y, data[i].Z, data[i].W);
			}
			return ret;
		}
		public static Godot.Vector4[] toGodot(Vector4[] data) {
			var ret = new Godot.Vector4[data.Length];
			for(int i = 0; i < data.Length; i++) {
				ret[i] = data[i].godot;
			}
			return ret;
		}
		public float x { get { return godot.X; } set { godot.X = value; }  }
		public float y { get { return godot.Y; } set { godot.Y = value; }  }
		public float z { get { return godot.Z; } set { godot.Z = value; }  }
		public float w { get { return godot.W; } set { godot.W = value; }  }
		public Vector4(float _x, float _y, float _z, float _w) {
			godot.X = _x;
			godot.Y = _y;
			godot.Z = _z;
			godot.W = _w;
		}

		// mostly untested! (autocompleted by github copilot)
		public static Vector4 operator + (Vector4 a, Vector4 b) { return new Vector4(a.x+b.x, a.y+b.y, a.z+b.z, a.w+b.w); }
		public static Vector4 operator - (Vector4 a, Vector4 b) { return new Vector4(a.x-b.x, a.y-b.y, a.z-b.z, a.w-b.w); }
		public static Vector4 operator * (Vector4 a, Vector4 b) { return new Vector4(a.x*b.x, a.y*b.y, a.z*b.z, a.w*b.w); }
		public static Vector4 operator / (Vector4 a, Vector4 b) { return new Vector4(a.x/b.x, a.y/b.y, a.z/b.z, a.w/b.w); }
		public static Vector4 operator * (Vector4 a, float b) { return new Vector4(a.x*b, a.y*b, a.z*b, a.w*b); }
		public static Vector4 operator / (Vector4 a, float b) { return new Vector4(a.x/b, a.y/b, a.z/b, a.w/b); }
		public static Vector4 operator * (float b, Vector4 a) { return new Vector4(a.x*b, a.y*b, a.z*b, a.w*b); }
		public static Vector4 operator / (float b, Vector4 a) { return new Vector4(a.x/b, a.y/b, a.z/b, a.w/b); }
		public static Vector4 Lerp(Vector4 a, Vector4 b, float t) { return new Vector4(
			Mathf.Lerp(a.x, b.x, t),
			Mathf.Lerp(a.y, b.y, t),
			Mathf.Lerp(a.z, b.z, t),
			Mathf.Lerp(a.w, b.w, t) );
		 }
		public static float Dot(Vector4 a, Vector4 b) { return a.x*b.x + a.y*b.y + a.z*b.z + a.w*b.w; }
		public static float Distance(Vector4 a, Vector4 b) { return Mathf.Sqrt((a.x-b.x)*(a.x-b.x) + (a.y-b.y)*(a.y-b.y) + (a.z-b.z)*(a.z-b.z) + (a.w-b.w)*(a.w-b.w)); }
		public static Vector4 Normalize(Vector4 a) { return a / a.magnitude; }
		public float magnitude { get { return Mathf.Sqrt(x*x + y*y + z*z + w*w); } }
		public float sqrMagnitude { get { return x*x + y*y + z*z + w*w; } }
		public static Vector4 Scale(Vector4 a, Vector4 b) { return new Vector4(a.x*b.x, a.y*b.y, a.z*b.z, a.w*b.w); }
		public static Vector4 Min(Vector4 a, Vector4 b) { return new Vector4(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z), Mathf.Min(a.w, b.w)); }
		public static Vector4 Max(Vector4 a, Vector4 b) { return new Vector4(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z), Mathf.Max(a.w, b.w)); }
		public static Vector4 Project(Vector4 a, Vector4 b) { return b * (Dot(a, b) / Dot(b, b)); }
		public static Vector4 ProjectOnPlane(Vector4 a, Vector4 b) { return a - Project(a, b); }
		public static Vector4 Reflect(Vector4 a, Vector4 b) { return a - 2f * Project(a, b); }
		public static Vector4 LerpUnclamped(Vector4 a, Vector4 b, float t) { return new Vector4(
			Mathf.LerpUnclamped(a.x, b.x, t),
			Mathf.LerpUnclamped(a.y, b.y, t),
			Mathf.LerpUnclamped(a.z, b.z, t),
			Mathf.LerpUnclamped(a.w, b.w, t) );
		 }
	}


	public struct Color {
		public float r { get { return godot.R; } set { godot.R = value; }  }
		public float g { get { return godot.G; } set { godot.G = value; }  }
		public float b { get { return godot.B; } set { godot.B = value; }  }
		public float a { get { return godot.A; } set { godot.A = value; }  }
		public Color(float _r, float _g, float _b, float _a) { godot = new Godot.Color(_r, _g, _b, _a); }
		public Godot.Color godot;
		public static Color white   = new Color(1f,1f,1f,1f); 
		public static Color black   = new Color(0f,0f,0f,1f); 
		public static Color red     = new Color(1f,0f,0f,1f); 
		public static Color green   = new Color(0f,1f,0f,1f); 
		public static Color blue    = new Color(0f,0f,1f,1f); 
		public static Color yellow  = new Color(1f,1f,0f,1f); 
		public static Color cyan    = new Color(0f,1f,1f,1f); 
		public static Color magenta = new Color(1f,0f,1f,1f); 
		public static Color Lerp(Color a, Color b, float t) { return new Color(
			Mathf.Lerp(a.r, b.r, t),
			Mathf.Lerp(a.g, b.g, t),
			Mathf.Lerp(a.b, b.b, t),
			Mathf.Lerp(a.a, b.a, t) );
		 }
	}
	public struct Quaternion {
		public Godot.Quaternion godot;
		public static Quaternion identity = new Quaternion() { godot = Godot.Quaternion.Identity };
		public Quaternion(float x, float y, float z, float w) { godot = new Godot.Quaternion(x, y, z, w); }
		public static Quaternion Euler(float x, float y, float z) { return new Quaternion() { godot = Godot.Quaternion.FromEuler(new Godot.Vector3(x, y, z)) }; }
		public static Quaternion Euler(Vector3 v) { return new Quaternion() { godot = Godot.Quaternion.FromEuler(v.godot) }; }
		public static Quaternion Slerp(Quaternion a, Quaternion b, float t) { return new Quaternion() { godot = a.godot.Slerp(b.godot, t) }; }
		public static Quaternion Inverse(Quaternion a) { return new Quaternion() { godot = a.godot.Inverse() }; }
		public static Quaternion operator *(Quaternion a, Quaternion b) { return new Quaternion() { godot = a.godot * b.godot }; }
		public static Vector3 operator *(Quaternion a, Vector3 b) { return new Vector3() { godot = a.godot *  b.godot }; }
		public static Quaternion AngleAxis(float angle, Vector3 axis) { return new Quaternion() { godot = new Godot.Quaternion(axis.godot, angle) }; }
		
		// todo
	}
	public class Transform : Component {
 
		private Transform _parent;
		public Transform parent { get { return _parent; } set { _parent = value; SetParent(value); } } 


		public void SetParent(Transform newparent) {
			var parentNode = gameObject.godot.GetParent();
			if(parentNode != null) 
				parentNode.CallDeferred("remove_child", gameObject.godot); // apparently not allowed inside notifications? (process, input, ready) 
			newparent.gameObject.godot.CallDeferred("add_child", gameObject.godot); // apparently not allowed inside notifications? (process, input, ready) 
			// note, this is bad! until deferred calls are made, _parent does not actually represent the parent!
			_parent = newparent;	

			//_parent = newparent;		
			//gameObject.godot.CallDeferred(SetParentLater); // no way to defer System.Action??

			//var parentNode = gameObject.godot.GetParent();
			//if(parentNode != null) 
			//	parentNode.RemoveChild(gameObject.godot);
			//newparent.gameObject.godot.AddChild(gameObject.godot);
			//_parent = newparent;
		}

		public void Rotate(Vector3 eulerDegrees) {
			var q = gameObject.godot.Quaternion;
			q *= Godot.Quaternion.FromEuler(eulerDegrees.godot * Mathf.Deg2Rad);
			gameObject.godot.Quaternion = q;
		}

		public Quaternion localRotation { get { return new Quaternion() { godot = gameObject.godot.Quaternion}; } set { gameObject.godot.Quaternion = value.godot; } }
		public Vector3 localPosition { get { return new Vector3() { godot = gameObject.godot.Position}; } set { gameObject.godot.Position = value.godot; } }
		public Vector3 localScale { get { return new Vector3() { godot = gameObject.godot.Scale}; } set { gameObject.godot.Scale = value.godot; } }

		public Quaternion rotation { get { return new Quaternion() { godot = gameObject.godot.GlobalTransform.Basis.GetRotationQuaternion()}; } set { Debug.LogError("set global rotation : not implemented"); } }
		public Vector3 position { get { return new Vector3() { godot = gameObject.godot.GlobalPosition}; } set { gameObject.godot.GlobalPosition = value.godot; } }
		public Vector3 lossyScale { get { return new Vector3() { godot = gameObject.godot.GlobalTransform.Basis.Scale}; } }
	}



	public partial struct Mathf
	{
		public static float Sin(float f) { return (float)Math.Sin(f); }
		public static float Cos(float f) { return (float)Math.Cos(f); }
		public static float Tan(float f) { return (float)Math.Tan(f); }
		public static float Asin(float f) { return (float)Math.Asin(f); }
		public static float Acos(float f) { return (float)Math.Acos(f); }
		public static float Atan(float f) { return (float)Math.Atan(f); }
		public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }
		public static float Sqrt(float f) { return (float)Math.Sqrt(f); }
		public static float Abs(float f) { return (float)Math.Abs(f); }
		public static int Abs(int value) { return Math.Abs(value); }
		public static float Min(float a, float b) { return a < b ? a : b; }
		public static int Min(int a, int b) { return a < b ? a : b; }
		public static float Max(float a, float b) { return a > b ? a : b; }
		public static int Max(int a, int b) { return a > b ? a : b; }
		public static float Pow(float f, float p) { return (float)Math.Pow(f, p); }
		public static float Exp(float power) { return (float)Math.Exp(power); }
		public static float Log(float f, float p) { return (float)Math.Log(f, p); }
		public static float Log(float f) { return (float)Math.Log(f); }
		public static float Log10(float f) { return (float)Math.Log10(f); }
		public static float Ceil(float f) { return (float)Math.Ceiling(f); }
		public static float Floor(float f) { return (float)Math.Floor(f); }
		public static float Round(float f) { return (float)Math.Round(f); }

		public static int CeilToInt(float f) { return (int)Math.Ceiling(f); }

		public static int FloorToInt(float f) { return (int)Math.Floor(f); }
		public static int RoundToInt(float f) { return (int)Math.Round(f); }
		public static float Sign(float f) { return f >= 0F ? 1F : -1F; }
		public const float PI = (float)Math.PI;
		public const float Infinity = Single.PositiveInfinity;
		public const float NegativeInfinity = Single.NegativeInfinity;
		public const float Deg2Rad = PI * 2F / 360F;
		public const float Rad2Deg = 1F / Deg2Rad;

		public static float Clamp(float value, float min, float max) { return value < min ? min : value > max ? max : value; }
		public static int Clamp(int value, int min, int max) { return value < min ? min : value > max ? max : value; }
		public static float Clamp01(float value) { return value < 0F ? 0F : value > 1F ? 1F : value; }
		public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
		public static float LerpUnclamped(float a, float b, float t) { return a + t * (b - a); }
	}


	public class RangeAttribute : Attribute
	{
		private readonly float parameter1;
		private readonly float parameter2;
		public float Parameter1 { get { return parameter1; } }
		public float Parameter2 { get { return parameter2; } }

		public RangeAttribute(float param1, float param2)
		{
			this.parameter1 = param1;
			this.parameter2 = param2;
		}
	}

}
#endif
