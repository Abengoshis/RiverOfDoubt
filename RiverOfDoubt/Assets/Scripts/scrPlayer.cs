using UnityEngine;
using System.Collections;

public class scrPlayer : MonoBehaviour
{	
	public GameObject Boat;
	public bool PlayerControl;	// Whether the player can be controlled.
	public float WalkSpeed;	// The speed to apply when walking.
	public float BoatSpeed;	// The speed of the boat.
	public float BoatTurn;	// The turn speed of the boat.

	private bool prevInteract = false;
	private Vector3 boatLastPosition;
	private Quaternion boatLastRotation;
	
	void Start ()
	{
		Screen.lockCursor = true;
		boatLastPosition = Boat.transform.position;
	}

	void Update ()
	{
		// Move the player with the boat.
		this.transform.position += Boat.transform.position - boatLastPosition;
		boatLastPosition = Boat.transform.position;

		Camera.main.transform.rotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles + Boat.transform.rotation.eulerAngles - boatLastRotation.eulerAngles);
		boatLastRotation = Boat.transform.rotation;

		if (Input.GetAxis("Interact") != 0)	// Check if in cabin // DEBUG
		{
			// Check for presses.
			if (prevInteract == false)
			{
				PlayerControl = !PlayerControl;
				if (PlayerControl)
					Camera.main.GetComponent<MouseLook>().enabled = true;
				else
					Camera.main.GetComponent<MouseLook>().enabled = false;
			}

			prevInteract = true;
		}
		else
		{
			prevInteract = false;
		}
	}
	
	void FixedUpdate()
	{
		// Stop the rigidbody from sleeping.
		this.rigidbody.WakeUp();

		// Reset the velocity.
		this.rigidbody.velocity = Vector3.zero;

		// Set the boat's velocity.
		Boat.rigidbody.velocity = Boat.transform.forward * BoatSpeed;

		if (PlayerControl)
		{
			Vector3 eyePosition = this.transform.position + Vector3.up * this.transform.localScale.y;
			if (Vector3.Distance (Camera.main.transform.position, eyePosition) > 0.5f)
			{
				Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, eyePosition, 0.2f);
				Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, this.transform.rotation, 0.1f);
			}
			else
			{
				Camera.main.transform.position = eyePosition;
			}

			HandlePlayer();
		}
		else
		{
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, Boat.transform.position + new Vector3(0, 12, -15), 0.1f);
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, Quaternion.Euler(30, 0, 0), 0.1f);
			HandleBoat();
		}
	}
	
	void HandlePlayer()
	{
		// Get the direction through which the player wants to move.
		Vector3 moveDirection = Vector3.zero;
		moveDirection += this.transform.forward * Input.GetAxis("Vertical");
		moveDirection += this.transform.right * Input.GetAxis("Horizontal");
		
		// Set the horizontal components of the velocity to the move direction's horizontal components.
		this.rigidbody.velocity += new Vector3(moveDirection.x * WalkSpeed, 0, moveDirection.z * WalkSpeed);
		this.transform.rotation = Quaternion.Euler (0, Camera.main.transform.rotation.eulerAngles.y, 0);
	}

	void HandleBoat()
	{
		Boat.rigidbody.AddTorque(0, Input.GetAxis ("Horizontal") * BoatTurn, 0);
	}
}
