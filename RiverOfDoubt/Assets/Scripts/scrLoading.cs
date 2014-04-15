using UnityEngine;
using System.Collections;

public class scrLoading : MonoBehaviour
{
	// The purpose of the loading screen is to give a transition between the main menu and the game for less powerful PCs that take longer to load the level.
	// Use this for initialization
	void Start ()
	{
		Application.LoadLevel("River");
	}
}
