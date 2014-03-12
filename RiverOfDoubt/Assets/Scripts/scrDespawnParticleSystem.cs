using UnityEngine;
using System.Collections;

public class scrDespawnParticleSystem : MonoBehaviour
{
	ParticleSystem system;

	// Use this for initialization
	void Start ()
	{
		system = this.GetComponent<ParticleSystem>();
	}

	void Update ()
	{
		if (system.IsAlive() == false)
			Destroy (this.gameObject);
	}
}
