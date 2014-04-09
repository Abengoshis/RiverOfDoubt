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
		int choice = Random.Range (0, 2);

		if (choice < 1)
		{
			Powerup = AvailablePowerups[0];
		}
		else if (choice < 2)
		{
			Powerup = AvailablePowerups[1];
		}
//		else if (choice < 3)
//		{
//			Powerup = AvailablePowerups[2];
//		}
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

	void OnDestroy()
	{
		particleSystem.Play();

		foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
		{
			if (childRenderer.GetComponent<ParticleSystem>() == false)
				childRenderer.enabled = false;
		}

		Destroy (this.collider);
		Destroy (this.gameObject, 4);
	}
}
