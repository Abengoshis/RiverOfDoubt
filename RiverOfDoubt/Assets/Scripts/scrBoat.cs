using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public static float Health { get; private set; }
	public static ulong Distance { get; private set; }
	public const int HEALTH_MAX = 100;

	public GameObject ScrapeSplashPrefab;
	public GameObject BigSplashPrefab;
	public GameObject SmallSplashPrefab;

	private float wobble = 0;
	private float wobbleDelay = 3;
	private float wobbleTimer = 0;
	private Transform boatHolder;
	private float timeAtLowSpeed = 0;
	private scrController controller;

	// Use this for initialization
	void Start ()
	{
		wobbleTimer = wobbleDelay;
		boatHolder = this.transform.FindChild("BoatHolder");
		controller = GetComponentInChildren<scrController>();
		Health = HEALTH_MAX;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale == 0)
		{
			audio.volume = 0.075f;
			return;
		}
		else
		{
			audio.volume = 0.2f;
		}

		if (rigidbody.velocity.magnitude > 50f)
			Health = HEALTH_MAX;

		if (Health < 0)
			Health = 0;

		if (this.rigidbody.velocity.magnitude < 3)
		{
			timeAtLowSpeed += Time.deltaTime;
		}
		else
		{
			timeAtLowSpeed = 0;
		}

		if (wobbleTimer < wobbleDelay)
		{
			wobbleTimer += Time.deltaTime;

			boatHolder.rotation = Quaternion.Euler(this.transform.eulerAngles.x,
		                                           this.transform.eulerAngles.y,
		                                           wobble * Mathf.Sin (Mathf.PI * wobbleTimer / (0.25f * wobbleDelay)));

			wobble = Mathf.Lerp (wobble, 10, wobbleTimer / wobbleDelay);
		}

		audio.pitch = 0.8f + this.rigidbody.velocity.magnitude * 0.01f;

		Distance = (ulong)(this.transform.position.z + 225) / 2;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == LayerMask.NameToLayer("Animal"))
		{
			if (collision.transform.root.name == "Crocodile(Clone)")
			{
				Instantiate(SmallSplashPrefab, collision.transform.Find ("Croc").Find ("Head").position, SmallSplashPrefab.transform.rotation);
				Health -= 2;
			}
		}

		// Check whether the boat has collided with a part of the section.
		if (collision.transform.root.name.Contains ("Section"))
		{
			Instantiate (BigSplashPrefab, collision.contacts[0].point, Quaternion.identity);

			// Find how much damage to do by comparing the angle between the forward vector of the boat and the direction to the contact point.
			float damage = 180 - Vector3.Angle(this.rigidbody.velocity, collision.contacts[0].point - this.transform.position);

			Health -= damage * 0.1f;

			
			OnCollisionStay(collision);
		}
	}

	void OnCollisionStay(Collision collision)
	{
		if (Time.timeScale == 0) return;

		// Check whether the boat has collided with a part of the section.
		if (collision.transform.root.name.Contains ("Section"))
		{
			float damage = 0;

			// Deal constant damage if stuck.
			if (timeAtLowSpeed > 5)
			{
				Health -= timeAtLowSpeed * Time.deltaTime;

				// Find how much damage to do by comparing the angle between the forward vector of the boat and the direction to the contact point.
				damage = 180 - Vector3.Angle(this.rigidbody.velocity * timeAtLowSpeed, collision.contacts[0].point - this.transform.position);
			}
			else
			{
				// Find how much damage to do by comparing the angle between the forward vector of the boat and the direction to the contact point.
				damage = 180 - Vector3.Angle(this.rigidbody.velocity, collision.contacts[0].point - this.transform.position);
			}

			Instantiate (ScrapeSplashPrefab, collision.contacts[0].point, Quaternion.identity);

			// Deal damage over time.
			Health -= damage * 0.5f * Mathf.Min(2, this.rigidbody.velocity.magnitude) * 0.05f * Time.deltaTime;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Section")
		{
			scrSection otherSection = other.GetComponent<scrSection>();

			if (otherSection.PreviousSection != null)
				otherSection.PreviousSection.DestroyRedundantSections(otherSection);

			otherSection.GenerateNextSections(true);

			return;
		}

		if (rigidbody.velocity.magnitude > 50f)
			return;

		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") || other.name == "Explosion(Clone)")
		{
			if (other.transform.root.name == "SmallRock(Clone)")
			{
				Health -= 5;
			}
			else if (other.transform.root.name == "MediumRock(Clone)")
			{
				Health -= 10;
			}
			else if (other.transform.root.name == "LargeRock(Clone)")
			{
				Health -= 15;
			}
			else if (other.transform.root.name == "Hut_A(Clone)" || other.transform.root.name == "Hut_B(Clone)")
			{
				Health -= 10;

				// Detach all children and give them rigidbodies and torque. Destroy them after 3 seconds.
				Transform[] children = other.transform.root.GetComponentsInChildren<Transform>();
				for (int i = 0; i < children.Length; i++)
				{
					children[i].parent = null;
					if (children[i].name == "Native(Clone)")
					{
						children[i].GetComponent<scrNative>().Kill();
						continue;
					}
				
					if (children[i].rigidbody == null)
					{
						children[i].gameObject.AddComponent<Rigidbody>();
					}
					children[i].rigidbody.isKinematic = false;
					children[i].rigidbody.AddTorque(Random.Range(-50, 51), Random.Range (-50, 51), Random.Range (-50, 51));
					if (children[i].collider)
					{
						foreach (Collider c in children[i].GetComponents<Collider>())
							Destroy (c);
					}
					Destroy (children[i].gameObject, 3);
				}
			}

			if (other.name != "Spear(Clone)")
				wobbleTimer = 0;

			if (other.name != "Whirlpool")
			{
				if (other.name == "Log(Clone)")
				{
					Instantiate(BigSplashPrefab, other.transform.TransformPoint(new Vector3(0, 0.5f, 0)), BigSplashPrefab.transform.rotation);
					Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);
					this.rigidbody.velocity = Vector3.zero;

					foreach (Collider c in other.transform.root.GetComponentsInChildren<Collider>())
					{
						Destroy (c);
					}

					Health -= 15;
				}
				else if (other.name == "Spear(Clone)")
				{
					Instantiate(SmallSplashPrefab, other.transform.position + other.transform.forward * 4, SmallSplashPrefab.transform.rotation);
					this.rigidbody.velocity *= 0.5f;
					Health -= 2;
				}
				else if (other.name == "Explosion(Clone)")
				{
					Health -= 5;	
					Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);
					this.rigidbody.velocity = Vector3.zero;
					wobbleTimer += Time.deltaTime;
				}
				else
				{
					Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);
					this.rigidbody.velocity = Vector3.zero;
				}
				// Make the obstacle fall.
				if (other.transform.root.rigidbody != null)
					other.transform.root.rigidbody.useGravity = true;

				// If the other obstacle has a hinge, destroy the hinge.
				if (other.transform.root.hingeJoint != null)
					Destroy(other.transform.root.hingeJoint);

				// Destroy the obstacle after 3 seconds.
				Destroy (other.transform.root.gameObject, 3);

				// Instantly destroy the obstacle's collider so it doesn't collide again.
				if (other != null)
					Destroy (other);

				return;
			}
		}

		if (other.name == "Crate(Clone)")
		{
			scrCrate crate = other.GetComponent<scrCrate>();
			scrGUI3D.CollectItem(crate.Powerup, crate.transform.position, 1f);
			switch(crate.Powerup.name)
			{
			case "Health":
				Health += 5;
				if (Health > HEALTH_MAX)
					Health = HEALTH_MAX;
				break;
			case "Dynamite":
				controller.Weapons[1].Ammo++;
				break;
			}

			Destroy(crate);
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.name == "Whirlpool")
		{
			if (wobbleTimer >= wobbleDelay)
			{
				Health -= 10;
				wobbleTimer = 0;
			}

			float distanceFactor = Vector3.Distance (
				new Vector3(this.transform.position.x, 0, this.transform.position.z), 
				new Vector3(other.transform.position.x, 0, other.transform.position.z)) / (other.transform.localScale.x * 30.25f);

			this.transform.position = new Vector3(this.transform.position.x, Mathf.Lerp (-15, 0, distanceFactor), this.transform.position.z);
			this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.LookRotation(new Vector3(other.transform.position.x, this.transform.position.y, this.transform.position.z + 5) - this.transform.position), 0.001f * (1 - distanceFactor));
		}
	}

	void OnTriggerExit(Collider other)
	{
	}
}
