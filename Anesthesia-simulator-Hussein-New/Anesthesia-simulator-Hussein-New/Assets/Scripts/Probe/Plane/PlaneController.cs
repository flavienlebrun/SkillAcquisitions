// DecompilerFi decompiler from Assembly-CSharp.dll class: PlaneController
using UnityEngine;

public class PlaneController : MonoBehaviour
{
	public GameObject cutPlane;

	private Material material;

	private Vector3 normal;

	private Vector3 position;

	private Renderer render;

	private void Start()
	{
		render = GetComponent<Renderer>();
		normal = cutPlane.transform.up;
		position = cutPlane.transform.position;
		UpdateShaderProperties();
	}

	private void Update()
	{
		UpdateShaderProperties();
	}

	private void UpdateShaderProperties()
	{
		normal = cutPlane.transform.up;
		position = cutPlane.transform.position;
		render.material.SetVector("_PlaneNormal", normal);
		render.material.SetVector("_PlanePosition", position);
	}
}
