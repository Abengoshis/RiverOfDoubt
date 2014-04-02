using UnityEngine;
using System.Collections;

/// <summary>
/// Really stupid script which for some reason is necessary. The errors this fixes didn't happen a day before I added it. Magic.
/// </summary>
public class scrHutFudge : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter(collision.collider);
	}

	void OnTriggerEnter(Collider collider)
	{
		if (collider.name == "Hut_A(Clone)" || collider.name == "Hut_B(Clone)" ||
		    collider.transform.parent != null && collider.transform.parent.name.Contains ("Rock"))
		{
			Destroy (this.gameObject);
		}
	}
}
