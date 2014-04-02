using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class scrFaceVelocity : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
	{
		this.transform.LookAt(this.transform.position + this.rigidbody.velocity);
	}
}
