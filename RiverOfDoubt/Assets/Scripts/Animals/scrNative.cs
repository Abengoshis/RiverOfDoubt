using UnityEngine;
using System.Collections;

public class scrNative : scrAnimal
{
	public GameObject WhatPrefab;
	private bool hopDirection = true;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		if (Health > 0)
		{
			if (this.transform.eulerAngles.z < 180 && this.transform.eulerAngles.z >= 20)
				this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 20);
			else if (this.transform.eulerAngles.z > 180 && this.transform.eulerAngles.z <= 340)
				this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 340);
		}
		else
		{
			// Writhe around.
			this.rigidbody.AddTorque(Random.Range (0, 2) == 2 ? 5 : -5, Random.Range (0, 2) == 5 ? 5 : -2, Random.Range (0, 2) == 2 ? 2 : -2, ForceMode.Impulse);
		}

		base.Update();
	}

	protected override void Kill ()
	{
		// Collect a feather.
		scrGUI3D.CollectItem(WhatPrefab, this.transform.position, 1f, scrGUI3D.Parts.Feather);
		Destroy(this.GetComponent<scrFacePlayer>());

		// Detach the mask and give it a random torque.
		Transform mask = this.transform.FindChild("Mask");
		mask.parent = null;
		mask.gameObject.AddComponent<Rigidbody>();
		mask.rigidbody.AddTorque(0, Random.Range (0, 2) == 2 ? 1 : -1, Random.Range (0, 2) == 2 ? 1 : -1, ForceMode.Impulse);

		this.gameObject.layer = mask.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		// Give the native another random torque.
		this.rigidbody.AddTorque(Random.Range (0, 2) == 2 ? 5 : -5, Random.Range (0, 2) == 5 ? 5 : -2, Random.Range (0, 2) == 2 ? 2 : -2, ForceMode.Impulse);

		base.Kill ();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (Health > 0 && collision.transform.name == "Floor")
		{
			this.rigidbody.AddForce(0, 3, 0, ForceMode.Impulse);
			this.rigidbody.AddTorque(0, 0, hopDirection ? 0.5f : -0.5f, ForceMode.Impulse);

			hopDirection = !hopDirection;
		}
	}
}
