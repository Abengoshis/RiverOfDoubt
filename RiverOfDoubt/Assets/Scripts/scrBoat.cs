using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public GameObject BigSplashPrefab;
	public int Health;

	private float wobble = 0;
	private float wobbleDelay = 3;
	private float wobbleTimer = 0;

	// Use this for initialization
	void Start ()
	{
		wobbleTimer = wobbleDelay;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (wobbleTimer < wobbleDelay)
		{
			wobbleTimer += Time.deltaTime;

			this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x,
			                                           this.transform.eulerAngles.y,
			                                           wobble * Mathf.Sin (Mathf.PI * wobbleTimer / (0.2f * wobbleDelay)));

			wobble = Mathf.Lerp (wobble, 3, wobbleTimer / wobbleDelay);
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
				if (other.transform.parent.name == "SmallRock(Clone)")
				{
					Health -= 1;
				}
				else if (other.transform.parent.name == "MediumRock(Clone)")
				{
					Health -= 2;
				}
				else if (other.transform.parent.name == "LargeRock(Clone)")
				{
					Health -= 3;
				}
				else
				{
					Debug.Log (other.name);
				}

				this.rigidbody.velocity = Vector3.zero;
				Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);

				// Make the obstacle fall.
				other.transform.parent.rigidbody.useGravity = true;

				// Destroy the obstacle after 5 seconds.
				Destroy (other.transform.parent.gameObject, 5);

				// Instantly destroy the obstacle's collider so it doesn't collide again.
				Destroy (other);

				wobbleTimer = 0;
			}
		}
	}
}
