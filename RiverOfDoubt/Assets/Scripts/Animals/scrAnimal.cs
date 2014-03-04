using UnityEngine;
using System.Collections;

public class scrAnimal : MonoBehaviour
{
	public enum Parts { Feathers, Tusk, Tail }

	// public static float comboTimer; ??
	public int Health = 1;
	protected Parts prize;	// The valuable prize part of the animal acquired when it is killed. // SHOULD BE STRING? DICTIONARY OF PARTS?
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
			//MeshRenderer[] meshRenderers = this.GetComponentsInChildren<MeshRenderer>();

			//foreach (MeshRenderer mr in meshRenderers)
			//{
			//	Color colour = mr.material.color;
			//	colour.a = 1 - deathFadeTimer / deathFadeDelay;
			//	mr.material.color = colour;
			//}
			//
			//if (deathFadeTimer >= deathFadeDelay)
			//{
			//	Destroy (this.gameObject);
			//}
			//
			//deathFadeTimer += Time.deltaTime;
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
