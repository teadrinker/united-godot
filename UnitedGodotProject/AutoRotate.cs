using UnityEngine;

public partial class AutoRotate : MonoBehaviour
{
	[Godot.Export] public float rotationSpeedX = 0f;
	[Godot.Export] public float rotationSpeedY = 90f;
	[Godot.Export] public float rotationSpeedZ = 0f;
	public void Update()
	{
		transform.Rotate(new Vector3(rotationSpeedX, rotationSpeedY, rotationSpeedZ) * Time.deltaTime);
	}
}
