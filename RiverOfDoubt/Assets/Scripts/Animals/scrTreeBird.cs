using UnityEngine;
using System.Collections;

public class scrTreeBird : scrAnimal
{
	private int alertRadius = 10;

	void Start ()
	{
		prize = Parts.Feathers;
	}

	protected override void Update ()
	{
		base.Update();
	}

	public override void Shoot (int damage)
	{
		// Cause all tree birds in the flee radius to flee.
		scrTreeBird[] treeBirds = GameObject.FindObjectsOfType<scrTreeBird>();
		foreach (scrTreeBird bird in treeBirds)
			if (Vector3.Distance (this.transform.position, bird.transform.position) < alertRadius)
				bird.Flee (this.transform.position);

		base.Shoot (damage);
	}

	public void Flee(Vector3 centre)
	{
		Vector3 direction = this.transform.position - centre;
		this.rigidbody.AddForce (direction * 50);
		this.rigidbody.AddForce (Vector3.up * 50);
	}
}
