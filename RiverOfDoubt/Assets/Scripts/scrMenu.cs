using UnityEngine;
using System.Collections;

public class scrMenu : MonoBehaviour
{
	public AudioClip ChangeMenu;

	public Transform MainMenu;
	public Transform AboutMenu;
	public Transform CreditsMenu;
	private byte menu = 0;

	// Use this for initialization
	void Start ()
	{
		Transform button;
		Material copy;

		button = AboutMenu.Find("Back").Find ("Button");
		copy = new Material(button.renderer.material);
		button.renderer.material = copy;

		button = CreditsMenu.Find("Back").Find ("Button");
		copy = new Material(button.renderer.material);
		button.renderer.material = copy;

		AboutMenu.Find ("Info").GetComponent<TextMesh>().text = 
			@"In 1914, Roosevelt and a small crew set off on an expedition down the unexplored
Amazonian 'River of Doubt'. The crew experienced disease, pestilence and famine
throughout the journey, resulting in increasing irritation.

One day, an intoxicated Roosevelt decided he'd had enough of the whole thing and
declared a personal war on all of the Amazon rainforest!

When the crew awoke, their boat was gone, along with Roosevelt and his hunting rifle.";

		CreditsMenu.Find ("Credits").GetComponent<TextMesh>().text = 
			@"Trees, Bushes, Plants and Rocks Models - Nobiax (Hugues) [nobiax.deviantart.com]
Log Model - azlyirnizam [tf3dm.com/3d-model/fallen-log-7859.html]
Title Font - Duality (Typodermic Fonts Inc.) [dafont.com/duality.font]
Text Font - F25 Executive (Typodermic Fonts Inc.) [dafont.com/f25-executive.font]
Title Music - Crash Bandicoot: Jungle Rollers (Naughty Dog)
Ingame Music - Crash Bandicoot: Boulders (Naughty Dog)
Collection & Gun Sound Effects - SoundJay
Other Sound Effects - Spyro 3: Year of the Dragon (Insomniac Games)
Everything Else - Alexander Saye";

		foreach (Transform t in MainMenu.GetComponentsInChildren<Transform>())
		{
			if (t.name == "Button")
			{
				copy = new Material(t.renderer.material);
				t.renderer.material = copy;
			}
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Reset buttons and move camera.
		if (menu == 0)
		{
			this.transform.position = Vector3.Lerp (this.transform.position, MainMenu.Find ("CameraPoint").position, 4f * Time.deltaTime);
			
			foreach (Transform t in MainMenu.GetComponentsInChildren<Transform>())
				if (t.name == "Button")
					t.renderer.material.SetColor("_Color", new Color(t.renderer.material.color.r, t.renderer.material.color.g, t.renderer.material.color.b, 0.35f));
		}
		else if (menu == 1)
		{
			this.transform.position = Vector3.Lerp (this.transform.position, AboutMenu.Find ("CameraPoint").position, 4f * Time.deltaTime);
			
			Transform t = AboutMenu.Find("Back").Find ("Button");
			t.renderer.material.SetColor("_Color", new Color(t.renderer.material.color.r, t.renderer.material.color.g, t.renderer.material.color.b, 0.35f));
		}
		else if (menu == 2)
		{
			this.transform.position = Vector3.Lerp (this.transform.position, CreditsMenu.Find ("CameraPoint").position, 4f * Time.deltaTime);
			
			Transform t = CreditsMenu.Find("Back").Find ("Button");
			t.renderer.material.SetColor("_Color", new Color(t.renderer.material.color.r, t.renderer.material.color.g, t.renderer.material.color.b, 0.35f));
		}

		RaycastHit hit;
		if (Physics.Raycast (this.camera.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << LayerMask.NameToLayer("GUI")))
	    {
			Transform button = hit.transform;
			button.renderer.material.SetColor("_Color", new Color(button.renderer.material.color.r, button.renderer.material.color.g, button.renderer.material.color.b, 0.5f));

			if (Input.GetMouseButtonDown(0))
			{
				switch(button.parent.name)
				{
				case "Start":
					Application.LoadLevel("Loading");
					Screen.lockCursor = true;
					break;
				case "About":
					menu = 1;
					audio.PlayOneShot(ChangeMenu);
					break;
				case "Credits":
					menu = 2;
					audio.PlayOneShot(ChangeMenu);
					break;
				case "Quit":
					Application.Quit();
					break;
				case "Back":
					menu = 0;
					audio.PlayOneShot(ChangeMenu);
					break;
				}
			}
		}
	}
}
