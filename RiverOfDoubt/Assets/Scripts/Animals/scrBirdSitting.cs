using UnityEngine;
using System.Collections;

public class scrBirdSitting : scrAnimal
{
	public GameObject BirdFlyingPrefab;
	public GameObject FeatherPrefab;

	private int alertRadius = 15;

	void Start ()
	{
		Health = 1;
		prize = Parts.Feathers;
	}

	protected override void Update ()
	{
		base.Update();
	}

	public override void Shoot (int damage)
	{
		// Cause all tree birds in the flee radius to flee.
		scrBirdSitting[] sittingBirds = GameObject.FindObjectsOfType<scrBirdSitting>();
		foreach (scrBirdSitting bird in sittingBirds)
			if (bird != this && Vector3.Distance (this.transform.position, bird.transform.position) < alertRadius)
				bird.Flee (this.transform.position);

		base.Shoot (damage);
	}

	public void Flee(Vector3 centre)
	{
		// Get the direction away from the flee point.
		Vector3 direction = this.transform.position - centre;

		// Make sure the direction always goes up.
		if (direction.y < 0)
			direction.y = -direction.y;

		// Shift the direction further upwards.
		direction.y += 10f;

		// Normalize the direction.
		direction.Normalize();

		// Replace this sitting bird with a flying bird, facing the direction it will be fleeing.
		Destroy (this.gameObject);
		GameObject replacement = (GameObject)Instantiate(BirdFlyingPrefab, this.transform.position, this.transform.rotation);

		// Make the new flying bird fly away from the flee point.
		replacement.rigidbody.velocity = direction * 20;
	}

	protected override void Kill ()
	{
		// Explode into a plume of feathers.
		// Explode into a plume of feathers.
		for (int i = 0; i < 32; i++)
		{
			Rigidbody feather = ((GameObject)Instantiate (FeatherPrefab, this.transform.position, Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)))).rigidbody;
			Vector3 direction = new Vector3(Random.Range(-1f, 1f), Random.Range (-1f, 1f), Random.Range (-1f, 1f));
			direction.Normalize();
			feather.rigidbody.velocity = direction * Random.Range(1f, 5f);
			feather.AddTorque(Random.Range (-50, 51), Random.Range (-50, 51), Random.Range (-50, 51));
			feather.useGravity = true;
		}

		// Give the bird gravity and add a random torque as it falls.
		this.rigidbody.useGravity = true;
		this.rigidbody.AddTorque(Random.Range (-100, 101), Random.Range (-100, 101), Random.Range (-100, 101));

		// Collect a feather.
		scrGUI3D.CollectItem(FeatherPrefab, this.transform.position, 1f);

		base.Kill ();
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red * 0.7f;
		Gizmos.DrawSphere(this.transform.position, alertRadius);

		Gizmos.color = Color.white;
		scrBirdSitting[] sittingBirds = GameObject.FindObjectsOfType<scrBirdSitting>();
		foreach (scrBirdSitting bird in sittingBirds)
			if (bird != this && Vector3.Distance (this.transform.position, bird.transform.position) < alertRadius)
				Gizmos.DrawLine(this.transform.position, bird.transform.position);
	}
}
