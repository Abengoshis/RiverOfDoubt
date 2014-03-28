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
	private List<Transform> nativeAnimals = new List<Transform>();
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

	public void GenerateNextSections(bool direct)
	{
		// Don't generate more sections if the sections have already been generated.
		if (entered == true) return;

		if (nextSections == null)
		{
			// Set the number of next sections to number of connectors.
			nextSections = new scrSection[Connectors.Length];
			
			// Generate the next sections for each connector.
			for (int i = 0; i < Connectors.Length; i++)
			{
				// Find and add a random section to the connector. (If the section can generate splitters, give it a 50% chance to do so.
				if (CanGenerateSplitters == true && Random.Range (0, 2) == 0)
				{
					if (i > 0)
					{
						// Determine which direction to stop sections being generated in.
						if (Connectors[i].position.x > Connectors[i - 1].position.x)
						{
							if (Connectors[i - 1].name == "Section_Right(Clone)" || Connectors[i - 1].name == "Section_Line(Clone)")
								nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[Random.Range(1, gameManager.SplitterSections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
						else
						{
							if (Connectors[i - 1].name == "Section_Left(Clone)" || Connectors[i - 1].name == "Section_Line(Clone)")
								nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[Random.Range(0, gameManager.SplitterSections.Length - 1)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
					}

					nextSections[i] = ((GameObject)Instantiate(gameManager.SplitterSections[Random.Range(0, gameManager.SplitterSections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
				}
				else
				{
					nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[Random.Range(0, gameManager.Sections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
				}

				// Generate rocks for the next section.
				//nextSections[i].GenerateRocks(10);
				StartCoroutine(GenerateRocks(10));

				//nextSections[i].GenerateAnimals(20, 4, 3, 10);
				StartCoroutine(GenerateAnimals(20, 4, 3, 10));

				// Set the previous section of the next section to this section.
				nextSections[i].PreviousSection = this;
			}
		}

		if (direct == true)
		{
			for (int i = 0; i < nextSections.Length; i++)
			{
				nextSections[i].GenerateNextSections(false);
			}

			// Flag as entered.
			entered = true;
		}
	}

	public IEnumerator GenerateAnimals(int treeBirds, int overheadBirds, int elephants, int huts)
	{
		Transform[] parts = this.transform.GetComponentsInChildren<Transform>();

		List<Transform> palms = new List<Transform>();
		List<Transform> animalHooks = new List<Transform>();
		List<Transform> hutHooks = new List<Transform>();

		for (int i = 0; i < parts.Length; i++)
		{
			if (parts[i].name == "palm_trio")
				palms.Add(parts[i]);
			else if (parts[i].name == "AnimalHook")
				animalHooks.Add(parts[i]);
			else if (parts[i].name == "HutHook")
				hutHooks.Add(parts[i]);
		}

		#region Tree Birds
		while (palms.Count > 0 && treeBirds > 0)
		{
			int i = Random.Range (0, palms.Count);
			Transform replacement = ((GameObject)Instantiate (gameManager.PopulatedPalmPrefab, palms[i].position, palms[i].rotation)).transform;
			replacement.parent = palms[i].parent;
			foreach (scrBirdSitting bird in replacement.GetComponentsInChildren<scrBirdSitting>())
			{
				bird.transform.parent = null;
				nativeAnimals.Add(bird.transform);
			}
			//replacement.DetachChildren();
			Destroy(palms[i].gameObject);
			palms.RemoveAt(i);
			--treeBirds;

			yield return new WaitForSeconds(0.1f);
		}
		#endregion

		#region Overhead Birds
		for (int i = 0; i < overheadBirds; i++)
		{
			// Instantiate a flying bird at a random position after the end of the section.
			Rigidbody bird = ((GameObject)Instantiate (gameManager.BirdFlyingPrefab, this.transform.position + new Vector3(Random.Range (-60f, 60f), Random.Range (25f, 40f), 400 + Random.Range (0f, 800f)), Quaternion.Euler(0, 180, 0))).rigidbody;

			// Give the bird force to make it move in the opposite direction to the general direction of the player.
			bird.AddForce(0, 0, -600);

			yield return new WaitForSeconds(0.1f);;
		}
		#endregion

		#region Elephants
		while (animalHooks.Count > 0 && elephants > 0)
		{
			int i = Random.Range (0, animalHooks.Count);
			Transform replacement = ((GameObject)Instantiate (gameManager.ElephantPrefab, animalHooks[i].position + animalHooks[i].forward + Vector3.up * 0.05f, animalHooks[i].rotation)).transform;

			// Choose whether to put a tree in front of the elephant.
			if (Random.Range(0, 2) == 0)
			{
				Transform fallingLog = ((GameObject)Instantiate(gameManager.FallingLogPrefab, animalHooks[i].position + animalHooks[i].forward + Vector3.up * gameManager.FallingLogPrefab.transform.localScale.y * 0.52f, animalHooks[i].rotation)).transform;
				nativeAnimals.Add(fallingLog);
				replacement.Translate (0, 0, -12, Space.Self);
				replacement.GetComponent<scrElephantStanding>().TreeToPush = fallingLog;
			}
		
			nativeAnimals.Add(replacement);
			Destroy(animalHooks[i].gameObject);
			animalHooks.RemoveAt(i);
			--elephants;

			yield return new WaitForSeconds(0.1f);;
		}
		#endregion

		#region Huts
		Transform hutGroup = this.transform.FindChild("Huts");
		if (hutGroup != null)
		{
			// 50% chance for huts to be on either side.
			if (Random.Range (0, 2) == 0)
				hutGroup.localScale = new Vector3(-hutGroup.localScale.x, hutGroup.localScale.y, hutGroup.localScale.z);

			while (hutHooks.Count > 0 && huts > 0)
			{
				int i = Random.Range (0, hutHooks.Count);
				Transform replacement = ((GameObject)Instantiate ((Random.Range (0, 2) == 0 ? gameManager.HutAPrefab : gameManager.HutBPrefab), hutHooks[i].position + Vector3.up * Random.Range (3, 5), Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;				
				nativeAnimals.Add(replacement);
				Destroy(hutHooks[i].gameObject);
				hutHooks.RemoveAt(i);
				--huts;

				// 50% chance to have a native in the hut.
				if (Random.Range (0, 2) == 0)
					Instantiate (gameManager.NativePrefab, replacement.transform.position + Vector3.up * 2f, Quaternion.identity);

				yield return new WaitForSeconds(0.1f);;
			}
		}
		#endregion
	}

	public IEnumerator GenerateRocks(int numRocks)
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

				yield return new WaitForSeconds(0.1f);
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

		for (int i = nativeAnimals.Count - 1; i >= 0; i--)
		{
			if (nativeAnimals[i] != null)
				Destroy (nativeAnimals[i].gameObject);

			nativeAnimals.RemoveAt (i);
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
		{
			if (nextSections[i] != null && nextSections[i] != keepSection)
			{
				for (int j = 0; j < nextSections[i].nextSections.Length; j++)
					Destroy (nextSections[i].nextSections[j].gameObject);

				Destroy (nextSections[i].gameObject);
			}
		}

		// Destroy the previous section as it is out of view.
		if (PreviousSection != null)
			Destroy (PreviousSection.gameObject);
	}
}
