using UnityEngine;
using System.Collections;

public class scrBoat : MonoBehaviour
{
	public int Health;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
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
	}
}
