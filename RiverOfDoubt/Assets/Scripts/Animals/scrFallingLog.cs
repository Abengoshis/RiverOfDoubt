using UnityEngine;
using System.Collections;

public class scrFallingLog : MonoBehaviour
{
	public GameObject BigSplashPrefab;
	private bool splooshed = false;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (splooshed == false && this.transform.eulerAngles.x >= 90 && this.transform.eulerAngles.x < 100)
		{
			Instantiate(BigSplashPrefab, this.transform.TransformPoint(new Vector3(0, 0.5f, 0)), BigSplashPrefab.transform.rotation);
			Instantiate(BigSplashPrefab, this.transform.position, BigSplashPrefab.transform.rotation);
			splooshed = true;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Obstacle" && other.name != "Whirlpool(Clone)" && other.GetComponent<Rigidbody>())
		{
			this.rigidbody.velocity = Vector3.zero;
			Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);
			
			// Make the obstacle fall.
			other.transform.root.rigidbody.useGravity = true;
			
			// If the other obstacle has a hinge, destroy the hinge.
			if (other.transform.root.hingeJoint != null)
				Destroy(other.transform.root.hingeJoint);
			
			// Destroy the obstacle after 5 seconds.
			Destroy (other.transform.root.gameObject, 5);
			
			// Instantly destroy the obstacle's collider so it doesn't collide again.
			Destroy (other);
		}
	}
}
