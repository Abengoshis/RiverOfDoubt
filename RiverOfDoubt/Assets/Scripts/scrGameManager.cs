using UnityEngine;
using System.Collections;

public class scrGameManager : MonoBehaviour
{
	public GameObject[] Sections;
	public GameObject Boat;
	public GameObject[] Rocks;
	public GameObject BirdSittingPrefab, BirdFlyingPrefab;

	// Use this for initialization
	void Start ()
	{
		scrSection.Initialize(this);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
