using UnityEngine;
using System.Collections;

public class scrFacePlayer : MonoBehaviour
{
	public bool Boat = false;
	public bool YawOnly = true;
	public float Speed = 0;
	private GameObject player;
	private GameObject boat;

	// Use this for initialization
	void Start ()
	{
		player = GameObject.Find ("Player");
		boat = GameObject.Find ("Boat");
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Time.timeScale == 0)
			return;

		// Yaw to face the player.
		Vector3 originalAngles = this.transform.eulerAngles;

		if (Boat == true)
			this.transform.LookAt(boat.transform.position);
		else
			this.transform.LookAt(player.transform.position);


		if (Speed == 0)
		{
			if (YawOnly == true)
				this.transform.eulerAngles = new Vector3(originalAngles.x, this.transform.eulerAngles.y, originalAngles.z);
		}
		else
		{
			Vector3 targetEuler = this.transform.eulerAngles;
			this.transform.rotation = Quaternion.Lerp (Quaternion.Euler(originalAngles), Quaternion.RotateTowards(this.transform.rotation, Quaternion.Euler(targetEuler), Speed), Speed);

			if (YawOnly == true)
				this.transform.eulerAngles = new Vector3(originalAngles.x, this.transform.eulerAngles.y, originalAngles.z);
		}
	}
}
