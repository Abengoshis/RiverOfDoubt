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
		if (Time.timeScale == 0)
			return;

		// Yaw to face the player.
		Vector3 originalAngles = this.transform.eulerAngles;
		this.transform.LookAt(player.transform.position);
		this.transform.eulerAngles = new Vector3(originalAngles.x, this.transform.eulerAngles.y, originalAngles.z);
	}
}
