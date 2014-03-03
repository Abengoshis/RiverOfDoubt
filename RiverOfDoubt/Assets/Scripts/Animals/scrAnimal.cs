using UnityEngine;
using System.Collections;

public class scrAnimal : MonoBehaviour
{
	// public static float comboTimer; ??
	public int Health = 1;
	public int Points = 0;
	private float deathFadeDelay = 4;
	private float deathFadeTimer = 0;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		if (Health <= 0)
		{
			// Fade the animal over time.
			Color colour = this.renderer.material.color;
			colour.a = 1 - deathFadeTimer / deathFadeDelay;
			this.renderer.material.color = colour;

			if (deathFadeTimer >= deathFadeDelay)
			{
				Destroy (this.gameObject);
			}

			deathFadeTimer += Time.deltaTime;
		}
	}
	
	public virtual void Shoot(int damage)
	{
		if (Health > 0)
		{
			Health -= damage;

			if (Health <= 0)
			{
				Kill ();
			}
		}
	}

	protected virtual void Kill()
	{
		// Explode
	}
}
