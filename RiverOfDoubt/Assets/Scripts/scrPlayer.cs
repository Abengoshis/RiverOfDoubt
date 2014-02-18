using UnityEngine;
using System.Collections;

public class scrPlayer : MonoBehaviour
{	
	public GameObject Boat;
	public bool PlayerIsFocus;	// Whether the player can be controlled.
	public float WalkSpeed;	// The speed to apply when walking.
	public float BoatSpeed;	// The speed of the boat.
	public float BoatTurn;	// The turn speed of the boat.

	private bool switchPressed = false;	// Whether trying to switch.
	private Transform switchDoor;	// The door to interact with.

	// The boat's position before updating, stored in order to move the player with the boat.
	private Vector3 boatLastPosition;
	private Quaternion boatLastRotation;

	// Switch timer variables.
	private float switchTimer = 0;
	private float switchDelay = 1;
	
	void Start ()
	{
		Screen.lockCursor = true;

		// Find the door.
		switchDoor = Boat.transform.FindChild("Door");

		// Initialize the boat's last position.
		boatLastPosition = Boat.transform.position;
	}

	void Update ()
	{
		// Run the switch timer backwards for player control and forwards for boat control.
		if (PlayerIsFocus == true)
		{
			switchTimer -= Time.deltaTime;
			if (switchTimer < 0)
				switchTimer = 0;

			// The player is free to roam, so make the player move with the boat.
			this.transform.position += Boat.transform.position - boatLastPosition;
			
			// Roughly rotate the camera with the boat. // NOTE: Probably not needed really.
			Camera.main.transform.rotation = Quaternion.Euler(Camera.main.transform.rotation.eulerAngles + Boat.transform.rotation.eulerAngles - boatLastRotation.eulerAngles);
			
			// Set the last position and rotation to the current position and rotation for the next update.
			boatLastRotation = Boat.transform.rotation;
			boatLastPosition = Boat.transform.position;
			
			// Check if the player is looking at the door.
			Ray lookRay = new Ray(Camera.main.transform.position - Camera.main.transform.forward * 0.1f, Camera.main.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(lookRay, out hit, 2, 1 << LayerMask.NameToLayer("Interactive")))
			{
				if (hit.collider.transform == switchDoor)
				{
					// Check if the player wants to interact.
					if (switchPressed == false && Input.GetAxis("Interact") != 0)
					{	
						// Swap control to the boat.
						PlayerIsFocus = false;
						
						// Flag as pressed.
						switchPressed = true;
					}
					
					// Light up the door to show you can click it. L4D2 style borders?
					Debug.Log ("Looking at the door!");
				}
			}
		}
		else
		{
			switchTimer += Time.deltaTime;
			if (switchTimer > switchDelay)
				switchTimer = switchDelay;

			// Check if the player wants to get out of the cabin.
			if (switchPressed == false && Input.GetAxis("Interact") != 0)
			{	
				// Swap control to the player.
				PlayerIsFocus = true;
				
				// Flag as pressed.
				switchPressed = true;
			}
		}

		// If interact is not pressed, make it so it can be pressed again.
		if (Input.GetAxis("Interact") == 0)
			switchPressed = false;
	}
	
	void FixedUpdate()
	{
		// Stop the player's rigidbody from sleeping.
		this.rigidbody.WakeUp();

		// Reset the player's velocity to zero.
		this.rigidbody.velocity = Vector3.zero;

		// Constantly set the boat's velocity to keep it moving.
		Boat.rigidbody.velocity = Boat.transform.forward * BoatSpeed;

		// Control either the player or the boat.
		if (PlayerIsFocus == true)
		{
			// Only control the player when the camera reaches them.
			if (switchTimer == 0)
				ControlPlayer();
		}
		else
		{
			ControlBoat();
		}
	}

	void LateUpdate()
	{
		// Check if transiting to or from the player (not controlling the player).
		if (switchTimer != 0)
		{
			// Keep the player next to and facing towards from the switch door.
			this.transform.position = new Vector3(switchDoor.position.x, this.transform.position.y, switchDoor.position.z) + switchDoor.transform.forward * this.transform.localScale.z;
			this.transform.rotation = switchDoor.transform.rotation;
			this.transform.Rotate (0, 180, 0);

			// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
			Camera.main.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler(30, 0, 0), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));
		}

		// Smoothstep lerp the position of the camera between the player's first person view position and the boat's third person view position.
		Camera.main.transform.position = Vector3.Lerp (this.transform.position + Vector3.up * this.transform.localScale.y, Boat.transform.position + new Vector3(0, 12, -15), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));

	}
	
	void ControlPlayer()
	{
		// Enable mouselook.
		Camera.main.GetComponent<MouseLook>().enabled = true;

		// Get the direction through which the player wants to move.
		Vector3 moveDirection = Vector3.zero;
		moveDirection += this.transform.forward * Input.GetAxis("Vertical");
		moveDirection += this.transform.right * Input.GetAxis("Horizontal");
		
		// Set the horizontal components of the velocity to the move direction's horizontal components.
		this.rigidbody.velocity += new Vector3(moveDirection.x * WalkSpeed, 0, moveDirection.z * WalkSpeed);
		this.transform.rotation = Quaternion.Euler (0, Camera.main.transform.rotation.eulerAngles.y, 0);
	}

	void ControlBoat()
	{
		// Disable mouselook.
		Camera.main.GetComponent<MouseLook>().enabled = false;

		// Turn the boat.
		Boat.rigidbody.AddTorque(0, Input.GetAxis ("Horizontal") * BoatTurn, 0);
	}
}
