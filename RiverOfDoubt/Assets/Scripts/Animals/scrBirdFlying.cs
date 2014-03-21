using UnityEngine;
using System.Collections;

public class scrBirdFlying : scrAnimal
{
	public GameObject FeatherPrefab;

	private Transform leftWing;
	private Transform rightWing;
	private float flapOffset;

	// Use this for initialization
	void Start ()
	{
		leftWing = this.transform.FindChild("LeftWing");
		rightWing = this.transform.FindChild("RightWing");
		flapOffset = Random.Range (0, 1000);
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		// Do not flap when killed.
		if (Health > 0)
		{
			// Determine the point in the flap cycle for the wings.
			float flappyBird = 10 * Mathf.Sin (flapOffset + Time.time * 14.5f);

			// Flap the wings.
			leftWing.localRotation = Quaternion.Euler(-flappyBird, 0, flappyBird);
			rightWing.localRotation = Quaternion.Euler(-flappyBird, 0, -flappyBird);

			// Face the direction of travel.
			if (this.rigidbody.velocity != Vector3.zero)
				this.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.LookRotation(this.rigidbody.velocity), 3 * Time.deltaTime);

			// Level out over time.
			if (this.rigidbody.velocity.y != 0)
			{
				// Slowly reduce the y value of the velocity by a small percentage each update.
				Vector3 temp = this.rigidbody.velocity;
				temp.y = Mathf.Lerp (temp.y, 0, (1.0f - flapOffset / 2000) * Time.deltaTime);
				this.rigidbody.velocity = temp;
			}
		}

		base.Update();
	}

	protected override void Kill ()
	{
		// Explode into a plume of feathers.
		for (int i = 0; i < 32; i++)
		{
			Rigidbody feather = ((GameObject)Instantiate (FeatherPrefab, this.transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)))).rigidbody;
			Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
			direction.Normalize();
			feather.rigidbody.velocity = direction * Random.Range(1f, 10f);
			feather.AddTorque(Random.Range (-50, 51), Random.Range (-50, 51), Random.Range (-50, 51));
			feather.useGravity = true;
		}

		// Give the bird gravity and add a random torque as it falls.
		this.rigidbody.useGravity = true;
		this.rigidbody.AddTorque(Random.Range (-100, 101), Random.Range (-100, 101), Random.Range (-100, 101));

		// Collect a feather.
		scrGUI3D.CollectItem(FeatherPrefab, this.transform.position, 1f, scrGUI3D.Parts.Feather);

		base.Kill ();
	}

	void OnCollisionEnter(Collision collision)
	{
		// If this bird collides with another bird while neither are dead, then there has been a spawning error and one must be deleted.
		if (Health > 0)
			if (collision.gameObject.GetComponent<scrBirdFlying>() != null)
				if (collision.gameObject.GetComponent<scrBirdFlying>().Health > 0)
					Destroy(collision.gameObject);
	}
}
