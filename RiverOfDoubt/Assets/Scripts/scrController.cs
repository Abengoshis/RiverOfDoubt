using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scrController : MonoBehaviour
{	
	[System.Serializable]
	public class Weapon
	{
		public Transform ViewModel;
		public float Recoil;
		public float Damage;
		public int Ammo;
		public AudioClip Sound;
	}

	public bool PlayerIsFocus;	// Whether the player can be controlled.
	public float WalkSpeed;	// The speed to apply when walking.
	public float BoatSpeed;	// The speed of the boat.
	public float BoatTurn;	// The turn speed of the boat.
	private Transform boat { get { return this.transform.root; } }

	public AudioSource AudioShoot, AudioSpeech;
	public Weapon[] Weapons;
	public int Gun = 0;
	private int nextGun = 0;
	private float changeGunTimer = -1, changeGunDelay = 0.5f;
	private bool firePressed = false;	// Whether trying to fire.
	private float recoilTimer = -1, recoilDelay = 0.3f;	// The recoil of the recently fired gun.
	private Vector3 gunStandardRotation = new Vector3(4.285588f, 2.832306f, 0.2118239f);
	private Transform gunWielder;
	private Transform gunEffect;
	private Light gunFlash;

	private bool switchPressed = false;	// Whether trying to switch.
	private Transform switchDoor = null;	// The door to interact with.
	private float switchTimer = 0, switchDelay = 1;	// Switch timer variables.

	void Start ()
	{
		Screen.lockCursor = true;
		gunWielder = Camera.main.transform.FindChild("Gun Camera").FindChild("Gun Wielder").transform;
		gunFlash = gunWielder.FindChild("Flash").light;
		MakeNextGunActive();
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

			CheckGunFire(lookRay);

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

		// Check if the player is underwater.
		if (this.transform.position.y < -0.3f)
			scrGUI3D.TransitionOverlayIn();
		else
			scrGUI3D.TransitionOverlayOut();
	}
	
	void FixedUpdate()
	{
		// Accelerate the boat forwards.
		boat.rigidbody.velocity += boat.transform.forward * BoatSpeed * Time.fixedDeltaTime * 0.2f;
		boat.rigidbody.velocity = boat.transform.forward * Mathf.Min (boat.rigidbody.velocity.magnitude, BoatSpeed);

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

		// Make sure the boat's rotation doesn't go past a certain amount of degrees to stop the player from going backwards.
		if (boat.eulerAngles.y <= 315 && boat.eulerAngles.y >= 180)
			boat.rigidbody.AddTorque(0, 2 * BoatTurn, 0);
		else if (boat.eulerAngles.y >= 45 && boat.eulerAngles.y <= 180)
			boat.rigidbody.AddTorque(0, 2 * -BoatTurn, 0);

		// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
		if (switchTimer >= switchDelay)
		{
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, Quaternion.Euler(boat.eulerAngles + new Vector3(25, 0, 0)), Time.fixedDeltaTime);
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, boat.position + new Vector3(-18 * boat.forward.x, 11, -18 * boat.forward.z), 5 * Time.fixedDeltaTime);
		}
	}

	void LateUpdate()
	{
		// Check if transiting to or from the player (not controlling the player).
		if (switchTimer != 0)
		{
			// Keep the player next to the switch door and facing the boat's front.
			this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + (switchDoor.name[0] == 'R' ? Vector3.right : Vector3.left) * this.transform.localScale.z;
			this.transform.rotation = switchDoor.transform.rotation;
			this.transform.Rotate (0, 180, 0);

			// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
			if (switchTimer < switchDelay)
				Camera.main.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler(boat.eulerAngles + new Vector3(25, 0, 0)), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));
		
			// Disable the reticle.
			scrGUI3D.ReticleIsVisible = false;
		}
		else
		{
			// Enable the reticle when the player has been reached.
			scrGUI3D.ReticleIsVisible = true;
		}
		
		// Smoothstep lerp the position of the camera between the player's first person view position and the boat's third person view position.
		if (switchTimer < switchDelay)
			Camera.main.transform.position = Vector3.Lerp (this.transform.position + Vector3.up * this.transform.localScale.y, boat.position + new Vector3(-18 * boat.forward.x, 11, -18 * boat.forward.z), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));

		// Set the player's velocity to the boat's velocity.
		this.rigidbody.velocity = boat.rigidbody.velocity;
	}

	void ChangeGun(int gun)
	{
		nextGun = gun;
		changeGunTimer = 0;
	}

	void MakeNextGunActive()
	{
		Weapons[Gun].ViewModel.gameObject.SetActive(false);

		// Swap the guns so that they can be alternated between.
		int temp = Gun;
		Gun = nextGun;
		nextGun = temp;

		AudioShoot.clip = Weapons[Gun].Sound;
		Weapons[Gun].ViewModel.gameObject.SetActive(true);
		gunEffect = Weapons[Gun].ViewModel.FindChild("MuzzleEffect");
	}

	void CheckGunFire(Ray hitscan)
	{
		// Check if the player wants to shoot.	// HAVE A LIST OF WEAPONS WITH RECOIL VALUES, DAMAGE, MODEL ETC.
		if (Input.GetAxis("Fire") > 0)
		{
			// Don't allow the player to hold down fire, and don't allow shooting while the recoil timer is running.
			if (firePressed == false && recoilTimer == -1 && Weapons[Gun].Ammo != 0 && AudioShoot.isPlaying == false)
			{
				RaycastHit hit;
				if (Physics.Raycast (hitscan, out hit, 10000, (1 << LayerMask.NameToLayer("Boat")) | (1 << LayerMask.NameToLayer ("Animal") |  (1 << LayerMask.NameToLayer ("Obstacle")))))
					if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Animal"))
						hit.transform.root.GetComponent<scrAnimal>().Shoot(Weapons[Gun].Damage);

				if (Weapons[Gun].Ammo > 0)
					--Weapons[Gun].Ammo;

				recoilTimer = 0;
				firePressed = true;
				gunFlash.gameObject.SetActive(true);
				gunEffect.gameObject.SetActive(true);
				gunEffect.Rotate (0, 0, Random.Range (45f, 180f), Space.Self);

				AudioShoot.clip = Weapons[Gun].Sound;
				AudioShoot.pitch = Random.Range (0.9f, 1.1f);
				AudioShoot.Play ();;
			}
		}
		else
		{
			firePressed = false;
		}

		if (recoilTimer != -1)
		{
			// Run the recoil timer.
			recoilTimer += Time.deltaTime;

			// Cause the camera to shift upwards with recoil.
			Camera.main.GetComponent<MouseLook>().rotationY += Weapons[Gun].Recoil * Mathf.Sin (Mathf.PI * 2 * recoilTimer / recoilDelay);

			// Cause the gun to shift backwards with recoil.
			gunWielder.localPosition = Weapons[Gun].Recoil * Vector3.back * Mathf.Sin (Mathf.PI * recoilTimer / recoilDelay);
			Weapons[Gun].ViewModel.localEulerAngles = gunStandardRotation + new Vector3(-10 * Weapons[Gun].Recoil * Mathf.Sin (Mathf.PI * recoilTimer / recoilDelay), 0, 0);

			float effectFactor = Mathf.Min (recoilTimer / (recoilDelay * 0.4f), 1);
			gunFlash.intensity = 8 * (1 - effectFactor);
			gunEffect.renderer.material.SetColor("_TintColor", new Color(1 - effectFactor, 1 - effectFactor, 1 - effectFactor, 1f));
			gunEffect.localScale = Vector3.one * (0.11f + 0.05f * Mathf.Sin (Mathf.PI * effectFactor));

			if (recoilTimer >= recoilDelay)
			{
				// Stop the recoil timer.
				recoilTimer = -1;
				gunFlash.gameObject.SetActive(false);
				gunEffect.gameObject.SetActive(false);
			}
		}
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
