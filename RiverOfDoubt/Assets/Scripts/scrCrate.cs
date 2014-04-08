using UnityEngine;
using System.Collections;

public class scrCrate : MonoBehaviour
{
	public enum Powerup { Something };
	public Powerup powerup;
	private int spin;

	// Use this for initialization
	void Start ()
	{
		spin = Random.Range (-50, 51);
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
		if (collision.transform.root.name == "Boat")
		{
			Destroy (this.gameObject);
		}
	}
}
