using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public GameObject BigSplashPrefab;
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
			otherSection.GenerateNextSections();

			if (otherSection.PreviousSection != null)
				otherSection.PreviousSection.DestroyRedundantSections(otherSection);
		}

		if (other.tag == "Obstacle")
		{
			if (wobbleTimer >= wobbleDelay)
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
				else
				{
					Debug.Log (other.name);
				}

				wobbleTimer = 0;

				if (other.name != "Whirlpool")
				{
					this.rigidbody.velocity = Vector3.zero;
					Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);

					// Make the obstacle fall.
					other.transform.root.rigidbody.useGravity = true;

					// If the other obstacle has a hinge, destroy the hinge.
					if (other.transform.root.hingeJoint != null)
						Destroy(other.transform.root.hingeJoint);

					// Destroy the obstacle after 5 seconds.
					Destroy (other.transform.root.gameObject, 5);

					// Instantly destroy the obstacle's collider so it doesn't collide again.
					Destroy (other);
				}
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
