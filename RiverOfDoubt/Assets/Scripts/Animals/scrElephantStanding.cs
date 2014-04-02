using UnityEngine;
using System.Collections;

public class scrElephantStanding : scrAnimal
{
	public GameObject TuskPrefab;
	public Transform TreeToPush;
	private GameObject player;
	private bool treePushed = false;
	private float rearTimer = 0, rearDelay = 2;

	void Start ()
	{
		player = GameObject.Find ("Player");
	}

	protected override void Update ()
	{
		if (Health > 0)
		{
			if (rearTimer >= 0)
			{
				rearTimer += Time.deltaTime;

				this.transform.eulerAngles = new Vector3(-20 * Mathf.Sin (Mathf.PI * 0.5f * rearTimer), this.transform.eulerAngles.y, this.transform.eulerAngles.z);
				this.rigidbody.isKinematic = true;

				if (rearTimer >= rearDelay)
					rearTimer = -1;
			}
			else
			{
				if (treePushed == false)
				{
					if (TreeToPush != null)
					{
						if (Vector3.Distance(this.transform.position, player.transform.position) < 100)
						{
							Vector3 direction = TreeToPush.position - this.transform.position;
							direction.y = 0;
							direction.Normalize();
							this.rigidbody.isKinematic = false;
							this.rigidbody.freezeRotation = true;
							this.rigidbody.velocity = direction * 5;
							Debug.Log ("Pushing Tree");
						}
					}
				}
			}
		}

		base.Update();
	}

	public override void Shoot (float damage)
	{
		if (rearTimer > rearDelay * 0.5f)
			rearTimer = rearDelay * 0.5f - (rearTimer - rearDelay * 0.5f);
		else if (rearTimer < 0)
			rearTimer = 0;

		base.Shoot (damage);
	}

	public override void Kill ()
	{
		// Give the elephant gravity and push it over.
		this.rigidbody.isKinematic = false;
		this.rigidbody.useGravity = true;
		this.rigidbody.freezeRotation = false;
		//this.rigidbody.AddTorque(0, Random.Range (0, 2) == 2 ? 10000 : -10000, Random.Range (0, 2) == 2 ? 10000 : -10000);
		Vector3 direction = this.transform.position - player.transform.position;
		direction.Normalize();
		this.rigidbody.AddForce(direction * this.rigidbody.mass * 200);

		Destroy(this.transform.FindChild("HeadPivot").GetComponent<scrFacePlayer>());

		// Collect a tusks.
		scrGUI3D.CollectItem(TuskPrefab, this.transform.position, 1f, scrGUI3D.Parts.Tusk);

		base.Kill ();
	}

	void OnCollisionEnter(Collision collision)
	{
		if (TreeToPush != null)
		{
			if (collision.gameObject.name == TreeToPush.name)
			{
				treePushed = true;
				this.rigidbody.isKinematic = true;
			}
		}
	}
}
