using UnityEngine;
using System.Collections;

public class scrOverheadBird : scrAnimal
{
	private Transform leftWing;
	private Transform rightWing;
	private float flapOffset;

	// Use this for initialization
	void Start ()
	{
		leftWing = this.transform.FindChild("LeftWing");
		rightWing = this.transform.FindChild("RightWing");
		flapOffset = Random.Range (0, 1000);

		// Add forward velocity.
		this.rigidbody.AddRelativeForce(0, 0, 600);
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		// Do not flap when killed.
		if (Health > 0)
		{
			float flappyBird = 10 * Mathf.Sin (flapOffset + Time.time * 7.5f);

			leftWing.localRotation = Quaternion.Euler(-flappyBird, 0, flappyBird);
			rightWing.localRotation = Quaternion.Euler(-flappyBird, 0, -flappyBird);
		}

		base.Update();
	}

	protected override void Kill ()
	{
		// Explode into a plume of feathers.

		// Give the bird gravity and add a random torque as it falls.
		this.rigidbody.useGravity = true;
		this.rigidbody.AddTorque(Random.Range (-100, 101), Random.Range (-100, 101), Random.Range (-100, 101));

		base.Kill ();
	}
}
