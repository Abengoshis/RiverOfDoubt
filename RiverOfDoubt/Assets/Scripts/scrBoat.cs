using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public int Health;

	private float wobble = 0;
	private float invincibleDelay = 3;
	private float invincibleTimer;

	// Use this for initialization
	void Start ()
	{
		invincibleTimer = invincibleDelay;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (invincibleTimer < invincibleDelay)
		{
			invincibleTimer += Time.deltaTime;

			this.transform.rotation = Quaternion.Euler(this.transform.eulerAngles.x,
			                                           this.transform.eulerAngles.y,
			                                           wobble * Mathf.Sin (Mathf.PI * invincibleTimer / (0.2f * invincibleDelay)));

			wobble = Mathf.Lerp (wobble, 3, invincibleTimer / invincibleDelay);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Section")
		{
			scrSection otherSection = other.GetComponent<scrSection>();
			otherSection.GenerateNextSections();

			if (otherSection.PreviousSection != null)
				otherSection.PreviousSection.DestroyRedundantSections(otherSection);
		}

		if (other.tag == "Obstacle")
		{
			if (invincibleTimer >= invincibleDelay)
			{
				if (other.name == "SmallRock(Clone)")
				{
					Health -= 1;
					wobble = 4;
				}
				else if (other.name == "MediumRock(Clone)")
				{
					Health -= 2;
					wobble = 8;
				}
				else if (other.name == "LargeRock(Clone)")
				{
					Health -= 3;
					wobble = 12;
				}

				invincibleTimer = 0;
			}
		}
	}
}
