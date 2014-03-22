using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class Collectable
{
	public Transform transform { get; private set; }
	public Vector3 InitialPosition { get; private set; }
	private float collectTimer;
	private float collectDelay;

	public float TimerProgress { get { return collectTimer / collectDelay; } }


	public Collectable(Transform transform, float collectDelay)
	{
		this.transform = transform;
		this.InitialPosition = transform.position;
		this.collectDelay = collectDelay;
		this.collectTimer = 0;
	}
	
	public void IncrementTimer()
	{
		collectTimer += Time.deltaTime;
	}

	public void Destroy()
	{
		GameObject.Destroy(this.transform.gameObject);
	}
}

public class scrGUI3D : MonoBehaviour
{	
	public static bool ReticleIsVisible = true;
	private static Rect reticleDestination { get { return new Rect(Screen.width / 2 - 16, Screen.height / 2 - 16, 32, 32); } }
	private static Rect reticleSource = new Rect(0, 0, 1, 1);

	private static Transform overlay;
	private static float overlayTimer = 0;
	private static float overlayDelay = 1.5f;

	private static List<Collectable> collectionItems = new List<Collectable>();
	private static Vector3 collectionPoint = new Vector3(-3.13f, -1.42f, 4.46f);
	private static float collectionStayTime = 1.5f;
	private static bool collecting = false;
	private static Transform chestLid;
	private static float chestTimer = 0;
	private static float chestDelay = 0.5f;

	public enum Parts { Feather, Tusk, Mask }
	private static int[] collectedParts = new int[3];

	private static scrGUI3D instance;
	private static Camera gunCamera;

	public Texture2D ReticleTexture;
	public AudioClip AudioCollect;
	public GameObject CollectionTextPrefab;

	// Use this for initialization
	void Start ()
	{
		instance = this;
		gunCamera = Camera.main.transform.FindChild("Gun Camera").camera;
		overlay = this.transform.FindChild("Overlay");
		chestLid = this.transform.FindChild("Chest").FindChild("Lid");
		collectionPoint = this.transform.TransformPoint(collectionPoint);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// Whether to show the gun camera or not depends on whether the reticle is visible.
		gunCamera.enabled = ReticleIsVisible;

		// Reset collecting. It will be flagged during the collection checking loop.
		collecting = false;

		for (int i = collectionItems.Count - 1; i >= 0; i--)
		{
			// Run the collectable's collection timer.
			collectionItems[i].IncrementTimer();
			if (collectionItems[i].TimerProgress >= 1 + collectionStayTime)
			{
				Destroy (collectionItems[i].transform.gameObject);
				collectionItems.RemoveAt(i);
			}
			else
			{
				// Currently collecting an item.
				collecting = true;

				// Reduce size after the item has reached the collection point.
				if (collectionItems[i].TimerProgress >= 1)
				{
					// Create one collection text when the object just starts reducing in size.
					if (collectionItems[i].transform.localScale.x == 1)
					{
						string text = "+ " + collectionItems[i].transform.name;
						text = text.Remove (text.IndexOf('('));
						CollectionTextPrefab.GetComponent<TextMesh>().text = text;
						Instantiate (CollectionTextPrefab, this.transform.TransformPoint(new Vector3(-4f, -2f, 5.75f)), this.transform.rotation);
					}

					collectionItems[i].transform.localScale = Vector3.Lerp (Vector3.one, Vector3.zero, Mathf.SmoothStep(0f, 1f, (collectionItems[i].TimerProgress - 1) / collectionStayTime));
				}

				// Smoothstep lerp the collectable towards the collection point.
				collectionItems[i].transform.position = Vector3.Lerp(collectionItems[i].InitialPosition, collectionPoint, Mathf.SmoothStep(0f, 1f, collectionItems[i].TimerProgress));

				// Rotate the collectable.
				collectionItems[i].transform.Rotate (0, 150 * Time.deltaTime, 0);
			}
		}

		// Run the chest timer forwards or backwards depending on the collection status.
		if (collecting == true)
		{
			chestTimer += Time.deltaTime;
			if (chestTimer >= chestDelay)
				chestTimer = chestDelay;
		}
		else
		{
			chestTimer -= Time.deltaTime;
			if (chestTimer <= 0)
				chestTimer = 0;
		}

		// Rotate the chest lid with the chest timer.
		chestLid.localEulerAngles = new Vector3(-60 * Mathf.SmoothStep(chestTimer, chestDelay, chestTimer / chestDelay), 0, 0);
	}

	void OnGUI()
	{
		if (ReticleIsVisible == true)
			GUI.DrawTextureWithTexCoords(reticleDestination, ReticleTexture, reticleSource);
	}

	public static void CollectItem(GameObject itemPrefab, Vector3 worldPosition, float timeToCollect, Parts part)
	{
		// Get the screen position of the item.
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

		// Set the new world position of the item.
		worldPosition = instance.camera.ScreenToWorldPoint(screenPosition);

		// Instantiate the item.
		Transform item = ((GameObject)Instantiate (itemPrefab, worldPosition, instance.transform.rotation)).transform;
		item.gameObject.layer = LayerMask.NameToLayer("GUI");
		foreach (Transform child in item.GetComponentsInChildren<Transform>())
		{
			child.gameObject.layer = item.gameObject.layer;
		}

		collectionItems.Add(new Collectable(item, timeToCollect));
		instance.audio.PlayOneShot(instance.AudioCollect);

		++collectedParts[(int)part];
	}

	public static void TransitionOverlayIn()
	{
		overlayTimer += Time.deltaTime;
		if (overlayTimer >= overlayDelay)
			overlayTimer = overlayDelay;

		RenderSettings.fogEndDistance = Mathf.SmoothStep (400, 2, overlayTimer / overlayDelay);
		RenderSettings.fogColor = new Color(0.36f, 0.42f, 0.38f);
		Camera.main.clearFlags = CameraClearFlags.SolidColor;

		Color temp = overlay.renderer.material.color;
		temp.a = overlayTimer / overlayDelay * 0.75f;
		overlay.renderer.material.color = temp;
	}

	public static void TransitionOverlayOut()
	{
		overlayTimer -= Time.deltaTime;
		if (overlayTimer <= 0)
		{
			overlayTimer = 0;
			RenderSettings.fogColor = new Color(0.35f, 0.28f, 0.26f);
		}

		RenderSettings.fogEndDistance = Mathf.Lerp (400, 2, overlayTimer / overlayDelay);
		Camera.main.clearFlags = CameraClearFlags.Skybox;

		Color temp = overlay.renderer.material.color;
		temp.a = overlayTimer / overlayDelay * 0.75f;
		overlay.renderer.material.color = temp;
	}

	void OnPreRender()
	{
		RenderSettings.fog = false;
	}

	void OnPostRender()
	{
		RenderSettings.fog = true;
	}
}
