using UnityEngine;
using System.Collections;

public class scrCrate : MonoBehaviour
{
	public GameObject Powerup;
	public GameObject[] AvailablePowerups;
	private int spin;

	// Use this for initialization
	void Start ()
	{
		spin = Random.Range (-50, 51);

		// Choose a powerup.
		int choice = Random.Range (0, 3);

		if (choice == 0)
		{
			Powerup = AvailablePowerups[0];
		}
		else
		{
			Powerup = AvailablePowerups[1];
		}

		// Give the crate a chance to collide normally before turning into a trigger.
		Invoke ("ChangeToTrigger", 0.1f);
	}

	void ChangeToTrigger()
	{
		collider.isTrigger = true;
	}

	// Update is called once per frame
	void Update ()
	{
		this.transform.position = new Vector3(
			this.transform.position.x,
			-0.3f + 0.25f * Mathf.Sin (2 * Time.time),
			this.transform.position.z);

		this.transform.Rotate (0, spin * Time.deltaTime, 0);
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.root.name.Contains("Section"))
		{
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.name == "Whirlpool" || other.name == "Whirlpool(Clone)")
		{
			Destroy (this.gameObject);
		}
	}

	void OnDestroy()
	{
		particleSystem.Play();
		audio.Play();

		foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
		{
			if (childRenderer.GetComponent<ParticleSystem>() == false)
				childRenderer.enabled = false;
		}

		Destroy (this.collider);
		Destroy (this.gameObject, 4);
	}
}
