using UnityEngine;
using System.Collections;

public class scrGameOver : MonoBehaviour
{
	public Transform Menu;
	public static int FinalScore { get; set; }

	// Use this for initialization
	void Start ()
	{
		Menu.Find ("Score").GetComponent<TextMesh>().text = FinalScore.ToString();
	}
	
	// Update is called once per frame
	void Update ()
	{
		foreach (Transform t in Menu.GetComponentsInChildren<Transform>())
			if (t.name == "Button")
				t.renderer.material.SetColor("_Color", new Color(t.renderer.material.color.r, t.renderer.material.color.g, t.renderer.material.color.b, 0.15f));
		
		RaycastHit hit;
		if (Physics.Raycast (this.camera.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << LayerMask.NameToLayer("GUI")))
		{
			Transform button = hit.transform;
			button.renderer.material.SetColor("_Color", new Color(button.renderer.material.color.r, button.renderer.material.color.g, button.renderer.material.color.b, 0.4f));
			
			if (Input.GetMouseButtonDown(0))
			{
				switch(button.parent.name)
				{
				case "Restart":
					Time.timeScale = 1;
					Screen.lockCursor = true;
					Application.LoadLevel("Loading");
					return;
				case "Main Menu":
					Time.timeScale = 1;
					Screen.lockCursor = false;
					Application.LoadLevel("Menu");
					return;
				}
			}
		}
	}
}
