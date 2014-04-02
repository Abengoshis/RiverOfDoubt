using UnityEngine;
using System.Collections;

public class scrGameManager : MonoBehaviour
{
	public GameObject[] Sections;
	public GameObject[] SplitterSections;
	public GameObject[] SpecialSections;
	public GameObject Boat;
	public GameObject[] Rocks;
	public GameObject PopulatedPalmPrefab;
	public GameObject BirdFlyingPrefab;
	public GameObject ElephantPrefab, FallingLogPrefab;
	public GameObject HutAPrefab, HutBPrefab, NativePrefab;

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
