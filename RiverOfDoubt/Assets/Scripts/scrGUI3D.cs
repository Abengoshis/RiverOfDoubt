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

	private static List<Collectable> collectionItems = new List<Collectable>();
	private static Vector3 collectionPoint = new Vector3(-4, -2, 5);
	private static float collectionStayTime = 0.5f;

	private static GameObject instance;

	public Texture2D ReticleTexture;

	// Use this for initialization
	void Start ()
	{
		instance = this.gameObject;
	}
	
	// Update is called once per frame
	void Update ()
	{
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
				// Reduce size after the item has reached the collection point.
				if (collectionItems[i].TimerProgress >= 1)
				{
					collectionItems[i].transform.localScale = Vector3.Lerp (Vector3.one, Vector3.zero, Mathf.SmoothStep(0f, 1f, (collectionItems[i].TimerProgress - 1) / collectionStayTime));
				}

				// Smoothstep lerp the collectable towards the collection point.
				collectionItems[i].transform.position = Vector3.Lerp(collectionItems[i].InitialPosition, this.transform.TransformPoint(collectionPoint), Mathf.SmoothStep(0f, 1f, collectionItems[i].TimerProgress));

				// Rotate the collectable.
				collectionItems[i].transform.Rotate (0, 150 * Time.deltaTime, 0);
			}
		}
	}

	void OnGUI()
	{
		if (ReticleIsVisible == true)
			GUI.DrawTextureWithTexCoords(reticleDestination, ReticleTexture, reticleSource);
	}

	public static void CollectItem(GameObject itemPrefab, Vector3 worldPosition, float timeToCollect)
	{
		// Get the screen position of the item.
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

		// Set the new world position of the item.
		worldPosition = instance.camera.ScreenToWorldPoint(screenPosition);

		// Instantiate the item.
		Transform item = ((GameObject)Instantiate (itemPrefab, worldPosition, instance.transform.rotation)).transform;

		collectionItems.Add(new Collectable(item, timeToCollect));
	}
}
