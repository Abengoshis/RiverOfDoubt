using UnityEngine;
using System.Collections;

public class scrController : MonoBehaviour
{	
	public bool PlayerIsFocus;	// Whether the player can be controlled.
	public float WalkSpeed;	// The speed to apply when walking.
	public float BoatSpeed;	// The speed of the boat.
	public float BoatTurn;	// The turn speed of the boat.
	private Transform boat { get { return this.transform.parent; } }

	private bool firePressed = false;	// Whether trying to fire.
	private int gunDamage = 1;	// DEBUG have gun prefab list.

	private bool switchPressed = false;	// Whether trying to switch.
	private Transform switchDoor = null;	// The door to interact with.

	// Switch timer variables.
	private float switchTimer = 0;
	private float switchDelay = 1;
	
	void Start ()
	{
		Screen.lockCursor = true;
	}

	void Update ()
	{
		// Run the switch timer backwards for player control and forwards for boat control.
		if (PlayerIsFocus == true)
		{
			switchTimer -= Time.deltaTime;
			if (switchTimer < 0)
				switchTimer = 0;

			Ray lookRay = new Ray(Camera.main.transform.position - Camera.main.transform.forward * 0.1f, Camera.main.transform.forward);
			RaycastHit hit;

			// Check if the player wants to shoot.
			if (Input.GetAxis("Fire") > 0)
			{
				if (firePressed == false && Physics.SphereCast (lookRay, 1, out hit, 100, 1 << LayerMask.NameToLayer("Animal")))
					hit.transform.GetComponent<scrAnimal>().Shoot(gunDamage);

				firePressed = true;
			}
			else
			{
				firePressed = false;
			}

			// Check if the player is looking at the door.
			if (Physics.Raycast(lookRay, out hit, 2, 1 << LayerMask.NameToLayer("Interactive")))
			{
				if (hit.collider.name == "LeftDoor" || hit.collider.name == "RightDoor")
				{
					// Check if the player wants to interact.
					if (switchPressed == false && Input.GetAxis("Interact") != 0)
					{	
						// Set the switchDoor to this door.
						switchDoor = hit.collider.transform;

						// Swap control to the boat.
						PlayerIsFocus = false;
						
						// Flag as pressed.
						switchPressed = true;
					}
					
					// Light up the door to show you can click it. L4D2 style borders?
					Debug.Log ("Looking at the door!");
				}

				Debug.Log (hit.transform.name);
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
		// Constantly set the boat's velocity to keep it moving.
		boat.rigidbody.velocity = boat.forward * BoatSpeed;

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
			this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + (switchDoor.name[0] == 'R' ? Vector3.right : Vector3.left) * this.transform.localScale.z;
			this.transform.rotation = switchDoor.transform.rotation;
			this.transform.Rotate (0, 180, 0);

			// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
			Camera.main.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler(new Vector3(25, 0, 0)), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));
		}

		// Smoothstep lerp the position of the camera between the player's first person view position and the boat's third person view position.
		Camera.main.transform.position = Vector3.Lerp (this.transform.position + Vector3.up * this.transform.localScale.y, boat.position + new Vector3(-18 * Vector3.forward.x, 11, -18 * Vector3.forward.z), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));

		// Set the player's velocity to the boat's velocity.
		this.rigidbody.velocity = boat.rigidbody.velocity;
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
		boat.rigidbody.AddTorque(0, Input.GetAxis ("Horizontal") * BoatTurn, 0);
	}
}
