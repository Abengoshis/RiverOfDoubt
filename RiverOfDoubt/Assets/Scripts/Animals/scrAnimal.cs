using UnityEngine;
using System.Collections;

public class scrAnimal : MonoBehaviour
{
	// public static float comboTimer; ??
	public GameObject DeathEffect;
	public Vector3 DeathEffectPosition;
	public float DeathEffectScale = 1;
	public AudioClip AudioHurt, AudioHurtAlternate, AudioDeath, AudioDeathAlternate;
	public float Health = 1;

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
	
	public virtual void Shoot(float damage)
	{
		if (Health > 0)
		{
			Health -= damage;

			if (Health <= 0)
			{
				Kill ();
			}
			else
			{
				audio.pitch = Random.Range (0.9f, 1.1f);
				if (Random.Range (0, 2) == 0)
					audio.PlayOneShot(AudioHurt);
				else
					audio.PlayOneShot(AudioHurtAlternate);
			}
		}
	}

	protected virtual void Kill()
	{
		// Explode
		DeathEffect = (GameObject)Instantiate(DeathEffect, this.transform.TransformPoint(DeathEffectPosition), this.transform.rotation);
		DeathEffect.particleSystem.startSpeed *= DeathEffectScale;
		DeathEffect.particleSystem.startSize *= DeathEffectScale;
		audio.pitch = Random.Range (0.9f, 1.1f);
		if (Random.Range (0, 2) == 0)
			audio.PlayOneShot(AudioDeath);
		else
			audio.PlayOneShot(AudioDeathAlternate);
	}
}
