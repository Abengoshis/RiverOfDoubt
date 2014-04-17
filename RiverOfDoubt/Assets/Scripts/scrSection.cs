using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrSection : MonoBehaviour
{
	private static scrGameManager gameManager;
	private static int untilSpecial = 3;

	public static void Initialize(scrGameManager _gameManager)
	{
		gameManager = _gameManager;
	}

	// Properties of the section.
	public int MinRocksToGenerate = 10;		
	public int MaxRocksToGenerate = 15;
	public int MinCratesToGenerate = 8;	
	public int MaxCratesToGenerate = 16;
	public int MinTreeBirdsToGenerate = 5;	
	public int MaxTreeBirdsToGenerate = 8;
	public int MinOverheadBirdsToGenerate = 10;	
	public int MaxOverheadBirdsToGenerate = 20;
	public int MinElephantsToGenerate = 2;	
	public int MaxElephantsToGenerate = 5;
	public int MinHutsOrCrocodilesToGenerate = 5;	
	public int MaxHutsOrCrocodilesToGenerate = 15;
	public Transform[] Connectors;
	public bool CanGenerateSplitters;
	public bool IsSplitterSection { get; protected set; }
	public int SectionIndex { get; protected set; }
	public scrSection PreviousSection { get; protected set; }
	private scrSection[] nextSections;
	private Transform[] rocks;
	private List<Transform> nativeAnimals = new List<Transform>();
	private bool entered = false;

	// Use this for initialization
	void Start ()
	{
		// Some sections have preset rocks and animals. Deparent them if this is the case in order for them to function properly.
		if (name == "Section_Temple_Natives(Clone)")
		{
			StartCoroutine(FreeChildPiggybackers());
		}
		else if (name == "Section_Volcano_Huts(Clone)")
		{
			StartCoroutine(FreeChildPiggybackers());

			// Very remote chance of making a flying elephant.
			if (Random.Range (0, 20) == 0)
			{
				Rigidbody elephant = ((GameObject)Instantiate (gameManager.ElephantFlyingPrefab, this.transform.position + new Vector3(Random.Range (-100, 100), 600, Random.Range(20, 50)), gameManager.ElephantFlyingPrefab.transform.rotation)).rigidbody;
				elephant.AddForce(elephant.transform.forward * 20, ForceMode.VelocityChange);
			}
		}
	}

	private IEnumerator FreeChildPiggybackers()
	{
		foreach (Transform child in GetComponentsInChildren<Transform>(true))
		{
			if (child.name.Contains("Rock"))
			{
				if (child.Find ("Graphics"))
				{
					child.parent = null;
					child.name += ("(Clone)");
					nativeAnimals.Add(child);
				}
			}
			else if (child.name.Contains("rock"))
			{
				child.gameObject.SetActive(true);
			}
			else if (child.name == "Crate")
			{
				child.name = "Crate(Clone)";
				child.parent = null;
				nativeAnimals.Add (child);
			}
			else if (child.GetComponent<scrAnimal>())
			{
				child.parent = null;
				nativeAnimals.Add(child);
			}

			yield return new WaitForSeconds(0.1f);
		}
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

			--untilSpecial;
			
			// Generate the next sections for each connector.
			for (int i = 0; i < Connectors.Length; i++)
			{
				int section = 0;

				// Find and add a random section to the connector. (If the section can generate splitters, give it a 50% chance to do so). (Don't allow sections just after splitters to craete more splitters. Doing so would create unnecessary lag and reveal the backfaces of some sections).
				if (CanGenerateSplitters == true && (PreviousSection == null || PreviousSection.name.Contains ("Split") == false) && (untilSpecial <= 0 || Random.Range (0, 2) == 0))
				{
					// If splitters can be generated, and the 50% chance has been achieved, and the previous section's previous section isn't a splitter, then give an extra 25% chance to create a special section.
					if (PreviousSection != null && (Random.Range (0, 4) == 0 || untilSpecial <= 0))
					{
						//Debug.Log (PreviousSection.name);
						nextSections[i] = ((GameObject)Instantiate(gameManager.SpecialSections[section = Random.Range(0, gameManager.SpecialSections.Length)], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						nextSections[i].CanGenerateSplitters = false;	// Prevent the next section from creating splitters and special sections.
						untilSpecial = Random.Range (4, 6);
					}
					else
					{
						nextSections[i] = ((GameObject)Instantiate(gameManager.SplitterSections[section = (SectionIndex + Random.Range(1, gameManager.SplitterSections.Length - 2)) % gameManager.SplitterSections.Length], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						nextSections[i].IsSplitterSection = true;
					}
				}
				else
				{
					if (i > 0)
					{
						if (Connectors[i - 1].position.x < Connectors[i].position.x && nextSections[i - 1].name != "Section_Left(Clone)")
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 2], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
						else if (Connectors[i - 1].position.x > Connectors[i].position.x && nextSections[i - 1].name != "Section_Right(Clone)")
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 0], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
						else
						{
							nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = 1], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
						}
					}
					else
					{
						nextSections[i] = ((GameObject)Instantiate(gameManager.Sections[section = (SectionIndex + Random.Range(1, gameManager.Sections.Length - 1)) % gameManager.Sections.Length], Connectors[i].position, Connectors[i].rotation)).GetComponent<scrSection>();
					}
				}

				nextSections[i].SectionIndex = section;
				//Debug.Log (nextSections[i].name + " generated after " + this.name);

				// Generate rocks for the next section.
				if (this.name != "Section_Start")
				{
					if (rocks == null && MaxRocksToGenerate > 0)
						StartCoroutine(GenerateRocksAndCrates(Random.Range (MinRocksToGenerate, MaxRocksToGenerate + 1), Random.Range (MinCratesToGenerate, MaxCratesToGenerate + 1)));

					if (nativeAnimals.Count == 0)
						StartCoroutine(GenerateAnimals(Random.Range (MinTreeBirdsToGenerate, MaxTreeBirdsToGenerate + 1),
						                               Random.Range (MinOverheadBirdsToGenerate, MaxOverheadBirdsToGenerate + 1),
						                               Random.Range (MinElephantsToGenerate, MaxElephantsToGenerate + 1),
						                               Random.Range (MinHutsOrCrocodilesToGenerate, MaxHutsOrCrocodilesToGenerate + 1), Random.Range (0, 3) == 0));
				}
				// Set the previous section of the next section to this section.
				nextSections[i].PreviousSection = this;
			}
		}

		if (direct == true)
		{
			for (int i = 0; i < nextSections.Length; i++)
			{
				nextSections[i].GenerateNextSections(false);
				//Debug.Log ("Generating indirect section " + i);
			}

			// Flag as entered.
			entered = true;
		}
	}

	public IEnumerator GenerateAnimals(int treeBirds, int overheadBirds, int elephants, int huts, bool crocodileHutOverride)
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

//			if (palms[i] == null)
//			{
//				palms.RemoveAt (i);
//				continue;
//			}

			Transform replacement = ((GameObject)Instantiate (gameManager.PopulatedPalmPrefab, palms[i].position, palms[i].rotation)).transform;
			replacement.parent = palms[i].parent;
			foreach (scrBirdSitting bird in replacement.GetComponentsInChildren<scrBirdSitting>())
			{
				bird.transform.parent = null;
				nativeAnimals.Add(bird.transform);
			}
			//replacement.DetachChildren();
			Destroy (palms[i].gameObject);
			palms.RemoveAt(i);
			--treeBirds;

			yield return new WaitForSeconds(0.1f);
		}
		#endregion

		#region Elephants
		while (animalHooks.Count > 0 && elephants > 0)
		{
			int i = Random.Range (0, animalHooks.Count);
			Transform replacement = ((GameObject)Instantiate (gameManager.ElephantPrefab, animalHooks[i].position + animalHooks[i].forward + Vector3.up * 0.05f, animalHooks[i].rotation)).transform;
			
			// Choose whether to put a tree in front of the elephant.
			if (Random.Range(0, 4) < 3)
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

		#region Huts or Crocodiles
		// Check for huts.
		Transform hutGroup = this.transform.Find("Huts");

		// Check for unflippable huts.
		if (hutGroup == null)
			hutGroup = this.transform.Find ("Huts_Noflip");

		if (hutGroup != null)
		{
			// 50% chance for huts to be on either side.
			if (hutGroup.name != "Huts_Noflip" && Random.Range (0, 2) == 0)
				hutGroup.localScale = new Vector3(-hutGroup.localScale.x, hutGroup.localScale.y, hutGroup.localScale.z);
			
			while (hutHooks.Count > 0 && huts > 0)
			{
				int i = Random.Range (0, hutHooks.Count);

//				// I have ABSOLUTELY NO IDEA why this happens: Even though I only remove the hut hooks that I add huts to, they still get included in the search.
//				if (hutHooks[i] == null || hutHooks[i].Find ("Hut_A(Clone)") || hutHooks[i].Find ("Hut_B(Clone)") || hutHooks[i].Find ("Raft(Clone)"))
//				{
//					hutHooks.RemoveAt (i);
//					continue;
//				}

				Transform replacement;

				// Normal huts.
				if (crocodileHutOverride == false)
				{
					if (Random.Range (0, 2) == 0)
					{
						replacement = ((GameObject)Instantiate (gameManager.HutAPrefab, hutHooks[i].position + Vector3.up * Random.Range (2f, 3f), Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;				
					}
					else
					{
						replacement = ((GameObject)Instantiate (gameManager.HutBPrefab, hutHooks[i].position + Vector3.up * Random.Range (2f, 3f), Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;	
					}
					
					// 90% chance to have a native in the hut.
					if (Random.Range (0, 10) < 9)
					{
						Transform native = ((GameObject)Instantiate (gameManager.NativePrefab, replacement.transform.position + Vector3.up * 3f, Quaternion.identity)).transform;
						native.parent = replacement;
						nativeAnimals.Add (native);
					}
				}
				else
				{
					// Spawn a crocodile instead of a hut. This varies hut sections a little more.
					replacement = ((GameObject)Instantiate (gameManager.CrocodilePrefab, hutHooks[i].position, Quaternion.Euler(0, Random.Range (0, 360), 0))).transform;	
				}

				nativeAnimals.Add(replacement);
				Destroy(hutHooks[i].gameObject);
				hutHooks.RemoveAt(i);
				--huts;
				
				yield return new WaitForSeconds(0.1f);;
			}
		}
		#endregion

		#region Overhead Birds
		for (int i = 0; i < overheadBirds; i++)
		{
			// Instantiate a flying bird at a random position after the end of the section.
			Rigidbody bird = ((GameObject)Instantiate (gameManager.BirdFlyingPrefab, this.transform.position + new Vector3(Random.Range (-100f, 100f), Random.Range (25f, 100f), 400 + Random.Range (0f, 800f)), Quaternion.Euler(0, 180, 0))).rigidbody;

			// Give the bird force to make it move in the opposite direction to the general direction of the player.
			bird.AddForce(0, 0, -600);

			yield return new WaitForSeconds(0.1f);;
		}
		#endregion
	}

	public IEnumerator GenerateRocksAndCrates(int numRocks, int numCrates)
	{
		if (numRocks == 0 && numCrates == 0) yield break;

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
				float currentX = children[i].collider.bounds.min.x;
				if (currentX < minX)
					minX = currentX;
			}
		}
	
		minX += 10f;

		// Get the large rock transform for convenience.
		Transform largeRock = gameManager.Rocks[gameManager.Rocks.Length - 1].transform;

		// Get the factor of the width and length of the water planes that corresponds to the size of a large rock.
		int rocksAcross = (int)((10 * waters[0].localScale.x * waters.Count) / largeRock.localScale.x);
		int rocksDown = (int)((10 * waters[0].localScale.z) / largeRock.localScale.z) + 1;

		// Create an array of available positions to perform the lottery operation on.
		Vector2[,] availablePositions = new Vector2[rocksAcross, rocksDown];
		for (int i = 0; i < rocksDown; i++)
			for (int j = 0; j < rocksAcross; j++)
				availablePositions[j, i] = new Vector2(j, i);

		// Make sure there can't be more rocks across and down than there are spaces for rocks!
		if (numRocks > rocksAcross * rocksDown)
			numRocks = rocksAcross * rocksDown;

		// Create an array of rocks.
		rocks = new Transform[numRocks];

		// Generate rocks.
		for (int i = 0; i < rocksDown && (numRocks > 0 || numCrates > 0); i++)
		{
			for (int j = 0; j < rocksAcross && (numRocks > 0 || numCrates > 0); j++)
			{
				// Get a random position after the current position by converting to a 1D coordinate.
				int position1D = Random.Range (i * rocksAcross + j, rocksAcross * rocksDown);

				// Convert the position to 2D coordinates.
				int positionDown = Mathf.FloorToInt (position1D / rocksAcross);
				int positionAcross =  position1D % rocksAcross;

				if (numRocks > 0)
				{
					// Instantiate the rock.
					rocks[numRocks - 1] = ((GameObject)Instantiate (gameManager.Rocks[Random.Range (0, gameManager.Rocks.Length)],
		             	new Vector3(minX, waters[0].position.y, this.transform.position.z) + new Vector3(availablePositions[positionAcross, positionDown].x, 0, availablePositions[positionAcross, positionDown].y) * largeRock.localScale.x,
		            	gameManager.Rocks[0].transform.rotation)).transform;

					rocks[numRocks - 1].FindChild ("Graphics").rotation = Quaternion.Euler(Random.Range (0, 360), Random.Range (0, 360), Random.Range (0, 360));

					// Decrease the number of rocks.
					numRocks--;
				}
				else
				{
					// Instantiate the crate.
					nativeAnimals.Add(((GameObject)Instantiate (gameManager.CratePrefab,
					                                            new Vector3(minX, waters[0].position.y, this.transform.position.z) +
					                                            	new Vector3(availablePositions[positionAcross, positionDown].x, 0, availablePositions[positionAcross, positionDown].y) * largeRock.localScale.x,
					                                            Quaternion.identity)).transform);

					// Decrease the number of crates.
					numCrates--;
				}

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
			{
				if (nativeAnimals[i].GetComponent<scrCrocodile>())
				{
					nativeAnimals[i].rigidbody.constraints = RigidbodyConstraints.None;
					nativeAnimals[i].rigidbody.useGravity = true;
					nativeAnimals[i].GetComponent<scrCrocodile>().enabled = false;
					Destroy (nativeAnimals[i].gameObject, 3);
				}

				Destroy (nativeAnimals[i].gameObject);
			}

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
