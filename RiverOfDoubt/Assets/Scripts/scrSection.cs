using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSection : MonoBehaviour
{
	private static scrGameManager gameManager;

	public static void Initialize(scrGameManager _gameManager)
	{
		gameManager = _gameManager;
	}

	// Properties of the section.
	public Transform[] Connectors;
	public bool CanGenerateSplitters;
	public scrSection PreviousSection { get; protected set; }
	private scrSection[] nextSections;
	private Transform[] rocks;
	private bool entered = false;

	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
		if (gameManager.Boat.transform.position.z - this.transform.position.z > 800)
			Destroy (this.gameObject);
	}

	public void GenerateNextSections()
	{
		// Don't generate more sections if the sections have already been generated.
		if (entered == true) return;

		// Set the number of next sections to number of connectors.
		nextSections = new scrSection[Connectors.Length];
		
		// Generate the next sections for each connector.
		for (int i = 0; i < Connectors.Length; i++)
		{
			// Find and add a random section to the connector. (If the section can generate splitters, give it a 50% chance to do so.
			if (CanGenerateSplitters == true && Random.Range (0, 2) == 0)
				nextSections[i] = ((GameObject)Instantiate(gameManager.SplitterSections[Random.Range(0, gameManager.SplitterSections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
			else
				nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[Random.Range(0, gameManager.Sections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();

			// Generate rocks for the next section.
			nextSections[i].GenerateRocks(10);

			nextSections[i].GenerateAnimals(10, 20, 10, 10);

			// Set the previous section of the next section to this section.
			nextSections[i].PreviousSection = this;
		}

		// Flag as entered.
		entered = true;

		Debug.Log ("Generating next section.");
	}

	public void GenerateAnimals(int treeBirds, int overheadBirds, int elephants, int etc)
	{
		#region Overhead Birds
		for (int i = 0; i < overheadBirds; i++)
		{
			// Instantiate a flying bird at a random position after the end of the section.
			Rigidbody bird = ((GameObject)Instantiate (gameManager.BirdFlyingPrefab, this.transform.position + new Vector3(Random.Range (-60f, 60f), Random.Range (25f, 40f), 400 + Random.Range (0f, 800f)), Quaternion.Euler(0, 180, 0))).rigidbody;

			// Give the bird force to make it move in the opposite direction to the general direction of the player.
			bird.AddForce(0, 0, -600);
		}
		#endregion
	}

	public void GenerateRocks(int numRocks)
	{
		// Get all children of this object.
		Transform[] children = gameObject.GetComponentsInChildren<Transform>();
		List<Transform> waters = new List<Transform>();

		float minX = float.PositiveInfinity;

		// Filter the children to get the water planes.
		for (int i = 0; i < children.Length; i++)
		{
			if (children[i].gameObject.layer == LayerMask.NameToLayer("Water"))
			{
				waters.Add(children[i]);

				// Find the minimum furthest left water plane.
				float currentX = children[i].transform.position.x - transform.localScale.x * 10;
				if (currentX < minX)
					minX = currentX;
			}
		}

		// Get the large rock transform for convenience.
		Transform largeRock = gameManager.Rocks[gameManager.Rocks.Length - 1].transform;

		// Get the factor of the width and length of the water planes that corresponds to the size of a large rock.
		int rocksAcross = (int)((10 * waters[0].localScale.x * waters.Count) / largeRock.localScale.x);
		int rocksDown = (int)((10 * waters[0].localScale.z) / largeRock.localScale.z) + 1;

		// Create an array of available positions to perform the lottery operation on.
		Vector2[,] availablePositions = new Vector2[rocksAcross, rocksDown];
		for (int i = 0; i < rocksAcross; i++)
			for (int j = 0; j < rocksDown; j++)
				availablePositions[i, j] = new Vector2(i - 1, j);

		// Make sure there can't be more rocks across and down than there are spaces for rocks!
		if (numRocks > rocksAcross * rocksDown)
			numRocks = rocksAcross * rocksDown;

		// Create an array of rocks.
		rocks = new Transform[numRocks];

		// Generate rocks.
		for (int i = 0; i < rocksDown && numRocks > 0; i++)
		{
			for (int j = 0; j < rocksAcross && numRocks > 0; j++)
			{
				int positionDown = Random.Range (i, rocksDown);
				int positionAcross = Random.Range (j, rocksAcross);

				// Instantiate the rock.
				rocks[numRocks - 1] = ((GameObject)Instantiate (gameManager.Rocks[Random.Range (0, gameManager.Rocks.Length)],
	             	new Vector3(minX, waters[0].position.y, this.transform.position.z) + new Vector3(availablePositions[positionAcross, positionDown].x, 0, availablePositions[positionAcross, positionDown].y) * largeRock.localScale.x,
	            	gameManager.Rocks[0].transform.rotation)).transform;

				rocks[numRocks - 1].FindChild ("Graphics").rotation = Quaternion.Euler(Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360));

				// Decrease the number of rocks.
				numRocks--;

				// Swap the current available position along and the found position.
				Vector2 temp = availablePositions[j, i];
				availablePositions[j, i] = availablePositions[positionAcross, positionDown];
				availablePositions[positionAcross, positionDown] = temp;
			}
		}
	}

	void OnDestroy()
	{
		if (rocks != null)
		{
			// Destroy all instances associated with this section.
			for (int i = 0; i < rocks.Length; i++)
				if (rocks[i] != null)
					Destroy (rocks[i].gameObject);
		}

	}

	/// <summary>
	/// Destroys the sections which are out of view or have not been taken.
	/// </summary>
	/// <param name="keepSection">The connected section which won't be destroyed (the section the boat is currently in).</param>
	public void DestroyRedundantSections(scrSection keepSection)
	{
		// Destroy untaken path(s).
		for (int i = 0; i < nextSections.Length; i++)
			if (nextSections[i] != null && nextSections[i] != keepSection)
				Destroy (nextSections[i].gameObject);

		// Destroy the previous section as it is out of view.
		if (PreviousSection != null)
			Destroy (PreviousSection.gameObject);
	}
}
