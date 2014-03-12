using UnityEngine;
using System.Collections;

/// <summary>
/// Despawns an object when it is under the world.
/// </summary>
public class scrDespawn : MonoBehaviour
{
	public bool DespawnWhenFarAway = false;
	public float FarAwayDistance = 500f;
	private GameObject Player;

	void Start()
	{
		Player = GameObject.Find ("Player");
	}

	// Update is called once per frame
	void Update ()
	{
		if (this.transform.position.y < -10)
			Destroy (this.gameObject);
		else
		{
			if (DespawnWhenFarAway == true)
			{
				if (Vector3.Distance (this.transform.position, Player.transform.position) > FarAwayDistance)
				{
					Destroy (this.gameObject);
				}
			}
		}

	}
}
