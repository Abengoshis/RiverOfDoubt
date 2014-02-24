using UnityEngine;
using System.Collections;

public class scrWaves : MonoBehaviour
{
	public float WaveSpeed;
	public float WaveHeight;

	private Mesh mesh;

	// Use this for initialization
	void Start ()
	{
		mesh = this.GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Copy the vertices to a standard array.
		Vector3[] vertices = mesh.vertices;

		// Undulate each vertex by an amount dependant on their x and z position (making the waves horizontal).
		for (int i = 0; i < vertices.Length; i++)
		{
			float xWave = Mathf.Sin (vertices[i].x / mesh.bounds.size.x * Mathf.PI * 2);
			float zWave = Mathf.Sin (vertices[i].z / mesh.bounds.size.z * Mathf.PI * 2);

			vertices[i].y = WaveHeight * Mathf.Sin (Time.time * WaveSpeed * xWave + zWave);
		}

		// Set the vertices back to the mesh and recalculate the normals.
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
}
