using UnityEngine;
using System.Collections;

public class scrGameManager : MonoBehaviour
{
	public GameObject[] Sections;
	public GameObject Boat;
	public GameObject[] Rocks;

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
