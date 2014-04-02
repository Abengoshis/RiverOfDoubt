using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Rigidbody))]
public class scrFaceVelocity : MonoBehaviour
{
	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale == 0)
			return;

		this.transform.LookAt(this.transform.position + this.rigidbody.velocity);
	}
}
