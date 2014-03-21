using UnityEngine;
using System.Collections;

public class scrRandomiseAudioOnStart : MonoBehaviour
{
	public float lowPitch = 1, highPitch = 1, lowVol = 1, highVol = 1;

	// Use this for initialization
	void Start ()
	{
		audio.pitch = Random.Range (lowPitch, highPitch);
		audio.volume = Random.Range (lowVol, highVol);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
