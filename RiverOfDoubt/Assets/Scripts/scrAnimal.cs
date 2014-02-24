using UnityEngine;
using System.Collections;

public class scrAnimal : MonoBehaviour
{
	// public static float comboTimer; ??
	public int Health = 0;
	public int Points = 0;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
	
	public void Shoot()
	{
		--Health;

		if (Health <= 0)
		{
			Kill();
		}
	}

	private void Kill()
	{

	}
}
