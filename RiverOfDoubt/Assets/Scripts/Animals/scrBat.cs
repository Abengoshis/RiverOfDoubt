using UnityEngine;
using System.Collections;

public class scrBat : scrAnimal
{
	public GameObject GuanoPrefab;
	private GameObject boat;

	private float originalY;
	private bool flying = false;
	private Transform leftWing;
	private Transform rightWing;
	private float flapOffset;

	// Use this for initialization
	void Start ()
	{
		leftWing = this.transform.FindChild("LeftWing");
		rightWing = this.transform.FindChild("RightWing");
		flapOffset = Random.Range (0, 1000);
		boat = GameObject.Find ("Boat");
		originalY = this.transform.position.y;
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		// Do not flap when killed.
		if (Health > 0)
		{
			if (flying == true)
			{
				// Determine the point in the flap cycle for the wings.
				float flappyBat = 10 * Mathf.Sin (flapOffset + Time.time * 14.5f);

				// Flap the wings.
				leftWing.localRotation = Quaternion.Euler(-flappyBat, 0, flappyBat);
				rightWing.localRotation = Quaternion.Euler(-flappyBat, 0, -flappyBat);

				// Face the direction of travel.
				this.transform.eulerAngles = Vector3.Lerp (this.transform.eulerAngles, new Vector3(45, this.rigidbody.velocity.z > 0 ? 0 : 180, 0), 3 * Time.deltaTime);
				this.rigidbody.velocity = new Vector3(0, 0, this.rigidbody.velocity.z);
				this.transform.position = new Vector3(this.transform.position.x, Mathf.Lerp(this.transform.position.y, originalY - 4f, 0.5f * Time.deltaTime), this.transform.position.z);
			}
			else
			{
				if (this.transform.position.z < boat.transform.position.z + 30) 
				{
					flying = true;
					this.transform.rigidbody.AddForce(0, 0, -8, ForceMode.Impulse);
				}
			}
		}

		base.Update();
	}

	public override void Kill ()
	{
		// Explode into a bunch of guano going downwards.
		for (int i = 0; i < 16; i++)
		{
			Rigidbody guano = ((GameObject)Instantiate (GuanoPrefab, this.transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)))).rigidbody;
			Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range (-0.8f, 0.1f), Random.Range (-1f, 1f));
			direction.Normalize();
			guano.rigidbody.velocity = direction * Random.Range(0.1f, 2f);
			guano.transform.localScale = Vector3.one * Random.Range (0.25f, 0.5f);
			guano.AddTorque(Random.Range (-50, 51), Random.Range (-50, 51), Random.Range (-50, 51));
			guano.useGravity = true;
		}

		// Give the bat gravity and add a random torque as it falls.
		this.rigidbody.useGravity = true;
		this.rigidbody.AddTorque(Random.Range (-100, 101), Random.Range (-100, 101), Random.Range (-100, 101));

		// Collect guano.
		scrGUI3D.CollectItem(GuanoPrefab, this.transform.position, 1f);

		base.Kill ();
	}

	void OnCollisionEnter(Collision collision)
	{
		// Reverse the Z speed when a wall is hit.
		if (Health > 0 && collision.transform.root.name.Contains("Section"))
		{
			this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, this.rigidbody.velocity.y, -this.rigidbody.velocity.z);
		}
	}
}
