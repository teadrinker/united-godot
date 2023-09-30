using UnityEngine;

public partial class UnitedGodotTest : MonoBehaviour
{
	[Godot.Export] public bool singleTriangle = false;
	/*[Godot.Export]*/ public Vector3 size = new Vector3(1, 1, 1);
	[Godot.Export][Range(-180,180)] public float rotationSpeed = 90f;
	[Godot.Export][Range(0,1)] public float scaling = 0.5f;
	/*[Godot.Export]*/ public Vector4 shaderParams = Vector4.one;

	private string _path = "UnitedGodotTest/";

	public void Start()
	{
		Texture2D tex = Resources.Load<Texture2D>(_path+"Texture");
		Shader shader = Resources.Load<Shader>(_path+"BasicShader");
		Mesh mesh = Resources.Load<Mesh>(_path+"Dodeca");
		var material = new Material(shader);
		material.SetTexture("_Texture2", tex);
		material.mainTexture = tex;		

		GenerateBoxMeshForGameObject(gameObject, Vector3.zero, size, material, singleTriangle);

		var child = new GameObject("Child");
		var handedness = UnitedGodot.Global.isGodot ? -1f : 1f;		
		child.AddComponent<AutoRotate>().rotationSpeedY = -90f * handedness;
		var childSize = Vector3.one * 0.25f;
		GenerateBoxMeshForGameObject(child, size / 2f + childSize / 2f, childSize, material, singleTriangle);
		child.transform.SetParent(transform);

		var dodecaHedron = new GameObject("dodecaHedron");
		dodecaHedron.AddComponent<MeshFilter>().sharedMesh = mesh;
		dodecaHedron.AddComponent<MeshRenderer>().sharedMaterial = material;
		dodecaHedron.transform.SetParent(transform);
		dodecaHedron.transform.localPosition = new Vector3(0, -1.3f, 0);
	}

	static void GenerateBoxMeshForGameObject(GameObject go, Vector3 pos, Vector3 size, Material material, bool singleTriangle)
	{
		var mesh = new Mesh();
		var s = size / 2f;

		Vector3[] vertices;
		int[] triangles;
		Vector2[] uv;

		if(singleTriangle) {
			vertices = new Vector3[] {
				new Vector3(   0,   0, 0),
				new Vector3(   0,  2f, 0),
				new Vector3(  2f,   0, 0),	
			};
			triangles = new int[] { 0, 1, 2 };
			uv = new Vector2[] {
				new Vector2(0, 0),
				new Vector2(0, 2),
				new Vector2(2, 0),
			};
		}
		else
		{
			vertices = new Vector3[]
			{
				new Vector3(-s.x, -s.y, -s.z),
				new Vector3( s.x, -s.y, -s.z),
				new Vector3( s.x,  s.y, -s.z),
				new Vector3(-s.x,  s.y, -s.z),
				new Vector3(-s.x, -s.y,  s.z),
				new Vector3( s.x, -s.y,  s.z),
				new Vector3( s.x,  s.y,  s.z),
				new Vector3(-s.x,  s.y,  s.z)
			};

			triangles = new int[]
			{
				0, 2, 1, // Front
				0, 3, 2,
				1, 2, 6, // Right
				1, 6, 5,
				4, 5, 6, // Back
				4, 6, 7,
				0, 7, 3, // Left
				0, 4, 7,
				0, 5, 4, // Bottom
				0, 1, 5,
				2, 3, 6, // Top
				3, 7, 6
			};
			uv = new Vector2[]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1),
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1)
			};
		}

		if(UnitedGodot.Global.isGodot)
		{
			pos.z = -pos.z; // fix handedness

			for(int i = 0; i < vertices.Length; i++)
			{
				vertices[i].z = -vertices[i].z; // fix handedness

				uv[i].y = 1f - uv[i].y; // NOTE! I don't know why this is needed, or even if this is a good fix!
			}
		}

		// Assign the mesh data
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uv;

		go.transform.localPosition = pos;		

		if(go.GetComponent<MeshFilter>() == null) go.AddComponent<MeshFilter>();
		if(go.GetComponent<MeshRenderer>() == null) go.AddComponent<MeshRenderer>();
		go.GetComponent<MeshFilter>().sharedMesh = mesh;
		go.GetComponent<MeshRenderer>().sharedMaterial = material;
	}

	public void Update()
	{
		var handedness = UnitedGodot.Global.isGodot ? -1f : 1f;
		transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed * handedness);
		var mat = GetComponent<MeshRenderer>().sharedMaterial;
		mat.color = Color.Lerp(Color.white, Color.yellow, Mathf.Sin(Time.time * 2f));
		mat.SetFloat("_Scaling", scaling + 0.2f * Mathf.Sin(Time.time));
		mat.SetVector("_ShaderParams", shaderParams);
	}
}
