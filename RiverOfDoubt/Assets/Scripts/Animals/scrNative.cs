using UnityEngine;
using System.Collections;

public class scrNative : scrAnimal
{
	public GameObject WhatPrefab;
	public GameObject SpearPrefab;
	private bool hopDirection = true;
	private GameObject player;
	private float throwSpearTimer = 0;
	private float throwSpearDelay = 3;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find ("Player");
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		if (Health > 0)
		{
			this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, this.transform.eulerAngles.z);

			if (this.transform.eulerAngles.z < 180 && this.transform.eulerAngles.z >= 20)
				this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 20);
			else if (this.transform.eulerAngles.z > 180 && this.transform.eulerAngles.z <= 340)
				this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 340);

			if (Vector3.Distance(this.transform.position, player.transform.position) < 100)
			{
				throwSpearTimer += Time.deltaTime;
				if (throwSpearTimer >= throwSpearDelay)
				{
					Vector3 direction = (player.transform.position + player.transform.root.rigidbody.velocity) - this.transform.position;
					direction.Normalize();
					direction.y = 1.3f;

					GameObject spear = (GameObject)Instantiate(SpearPrefab, this.transform.TransformPoint(1.2f, 1.0f, 0.0f), this.transform.rotation);
					spear.rigidbody.velocity = direction * 10;

					throwSpearTimer = 0;
					Debug.Log ("Throwing Spear");
				}
			}
		}
		else
		{
			// Writhe around.
			this.rigidbody.AddTorque(Random.Range (0, 2) == 2 ? 5 : -5, Random.Range (0, 2) == 5 ? 5 : -2, Random.Range (0, 2) == 2 ? 2 : -2, ForceMode.Impulse);
		}

		base.Update();
	}

	public override void Kill ()
	{
		// Collect a feather.
		scrGUI3D.CollectItem(WhatPrefab, this.transform.position, 1f, scrGUI3D.Parts.Feather);
		Destroy(this.GetComponent<scrFacePlayer>());

		// Detach the mask and give it a random torque.
		Transform mask = this.transform.FindChild("Mask");
		if (mask != null)
		{
			mask.parent = null;
			mask.gameObject.AddComponent<Rigidbody>();
			mask.rigidbody.AddTorque(0, Random.Range (0, 2) == 2 ? 1 : -1, Random.Range (0, 2) == 2 ? 1 : -1, ForceMode.Impulse);
			
			mask.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		}

		this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

		// Give the native another random torque.
		this.rigidbody.AddTorque(Random.Range (0, 2) == 2 ? 5 : -5, Random.Range (0, 2) == 5 ? 5 : -2, Random.Range (0, 2) == 2 ? 2 : -2, ForceMode.Impulse);

		// Remove constraints.
		this.rigidbody.freezeRotation = false;

		this.transform.parent = null;

		base.Kill ();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (Health > 0 && collision.collider.name == "Floor")
		{
			this.rigidbody.AddForce(0, 3, 0, ForceMode.Impulse);
			this.rigidbody.AddTorque(0, 0, hopDirection ? 0.5f : -0.5f, ForceMode.Impulse);

			hopDirection = !hopDirection;
		}
	}
}
