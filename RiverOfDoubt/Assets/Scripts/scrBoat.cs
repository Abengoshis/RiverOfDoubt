using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public GameObject BigSplashPrefab;
	public GameObject SmallSplashPrefab;
	public int Health;

	private float wobble = 0;
	private float wobbleDelay = 3;
	private float wobbleTimer = 0;
	private Transform boatHolder;

	// Use this for initialization
	void Start ()
	{
		wobbleTimer = wobbleDelay;
		boatHolder = this.transform.FindChild("BoatHolder");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale == 0)
			return;

		if (wobbleTimer < wobbleDelay)
		{
			wobbleTimer += Time.deltaTime;

			boatHolder.rotation = Quaternion.Euler(this.transform.eulerAngles.x,
		                                           this.transform.eulerAngles.y,
		                                           wobble * Mathf.Sin (Mathf.PI * wobbleTimer / (0.25f * wobbleDelay)));

			wobble = Mathf.Lerp (wobble, 10, wobbleTimer / wobbleDelay);
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
		}

		if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
		{
			if (other.transform.root.name == "SmallRock(Clone)")
			{
				Health -= 1;
			}
			else if (other.transform.root.name == "MediumRock(Clone)")
			{
				Health -= 2;
			}
			else if (other.transform.root.name == "LargeRock(Clone)")
			{
				Health -= 3;
			}
			else if (other.transform.root.name == "Hut_A(Clone)" || other.transform.root.name == "Hut_B(Clone)")
			{
				Health -= 2;

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
			else if (other.transform.root.name == "Spear(Clone")
			{
				Health -= 1;
			}

			if (other.name != "Spear(Clone)")
				wobbleTimer = 0;

			if (other.name != "Whirlpool(Clone)")
			{
				if (other.name == "Log(Clone)")
				{
					Instantiate(BigSplashPrefab, other.transform.TransformPoint(new Vector3(0, 0.5f, 0)), BigSplashPrefab.transform.rotation);
					Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);
					this.rigidbody.velocity = Vector3.zero;
				}
				else if (other.name == "Spear(Clone)")
				{
					Instantiate(SmallSplashPrefab, other.transform.position + other.transform.forward * 4, SmallSplashPrefab.transform.rotation);
					this.rigidbody.velocity *= 0.5f;
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
				Destroy (other);
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.name == "Whirlpool")
		{
			if (wobbleTimer >= wobbleDelay)
			{
				Health -= 5;
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
