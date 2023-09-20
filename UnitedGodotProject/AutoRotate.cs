using UnityEngine;

public partial class AutoRotate : MonoBehaviour
{
    [Godot.Export] public float rotationSpeed = 90f;
    public void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }
}
