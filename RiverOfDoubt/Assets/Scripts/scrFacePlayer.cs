using UnityEngine;
using System.Collections;

public class scrFacePlayer : MonoBehaviour
{
	private GameObject player;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find ("Player");
	}
	
	// Update is called once per frame
	void Update ()
	{
		this.transform.LookAt(player.transform.position);
		this.transform.Rotate(-this.transform.eulerAngles.x, 0, 0);
	}
}
