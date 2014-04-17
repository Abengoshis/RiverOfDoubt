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
	public float BoatSlowSpeed;
	public float BoatFastSpeed;
	public float BoatTurn;	// The turn speed of the boat.
	private Transform boat { get { return this.transform.root; } }

	public AudioClip[] speech;
	public AudioSource AudioShoot, AudioSpeech;
	public GameObject DynamitePrefab;
	private Vector3 lastArc;
	public Weapon[] Weapons;
	public int Gun = 0;
	private int nextGun = 0;
	private float changeGunTimer = -1, changeGunDelay = 0.5f;
	private float recoilTimer = -1, recoilDelay = 0.3f;	// The recoil of the recently fired gun.
	private float throwTimer = -1, throwDelay = 1.0f;
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
		if (Time.timeScale == 0)
			return;

		// Check for boost cheat.
		if (Input.GetKey (KeyCode.S) && Input.GetKey (KeyCode.A) && Input.GetKey (KeyCode.Y) && Input.GetKey (KeyCode.E) && Input.GetKeyDown(KeyCode.F10))
		{
			if (BoatFastSpeed == 20)
			{
				BoatFastSpeed = 200;
				AudioSpeech.PlayOneShot(speech[0], 0.3f);
			}
			else
			{
				BoatFastSpeed = 20;
				AudioSpeech.PlayOneShot(speech[1], 0.3f);
			}
		}

		// At this point, dirty code is the only way I'm going to get this finished on time. Thankfully this is game design, not software development!
		if (Weapons[1].Ammo == 0)
		{
			Weapons[1].ViewModel.gameObject.SetActive(false);
		}
		else
		{
			Weapons[1].ViewModel.gameObject.SetActive(true);
		}

		this.transform.localPosition = new Vector3(this.transform.localPosition.x, 1.6f, this.transform.localPosition.z);

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
			if (Physics.Raycast(lookRay, out hit, 2.5f, 1 << LayerMask.NameToLayer("Interactive")))
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

						// Show the trajectory.
						Weapons[1].ViewModel.GetComponent<LineRenderer>().enabled = false;
					}
					
					// Light up the door to show you can click it. L4D2 style borders?
					//Debug.Log ("Looking at the door!");
				}
				else if (hit.collider.name == "Chest")
				{
					if (Input.GetButtonDown("Interact") == true)
					{
						scrGUI3D.OpenInventory();
					}
				}

				//Debug.Log (hit.transform.name);
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
		if (PlayerIsFocus == false && Input.GetAxis ("Vertical") > 0)
		{
			// Accelerate the boat forwards.
			boat.rigidbody.velocity += boat.transform.forward * BoatFastSpeed * Time.fixedDeltaTime * 0.2f;

			// Keep the boat below the fast speed.
			boat.rigidbody.velocity = boat.transform.forward * Mathf.Min (boat.rigidbody.velocity.magnitude, BoatFastSpeed);

			
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, 70, 0.01f);
		}
		else if (PlayerIsFocus == false && Input.GetAxis ("Vertical") < 0)
		{

			// Slow the boat GRADUALLY if its going faster than it should be.
			if (boat.rigidbody.velocity.magnitude > BoatSlowSpeed)
			{
				boat.rigidbody.velocity -= boat.transform.forward * BoatSpeed * Time.fixedDeltaTime;
			}
			else
			{
				// Accelerate the boat forwards.
				boat.rigidbody.velocity += boat.transform.forward * BoatSlowSpeed * Time.fixedDeltaTime * 0.2f;
			}
			
			boat.rigidbody.velocity = boat.transform.forward * boat.rigidbody.velocity.magnitude;

			
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, 50, 0.01f);
		}
		else
		{
			// Slow the boat GRADUALLY if its going faster than it should be.
			if (boat.rigidbody.velocity.magnitude > BoatSpeed)
			{
				boat.rigidbody.velocity -= boat.transform.forward * BoatSpeed * Time.fixedDeltaTime;
			}
			else if (boat.rigidbody.velocity.magnitude < BoatSpeed)
			{
				// Accelerate the boat forwards.
				boat.rigidbody.velocity += boat.transform.forward * BoatSpeed * Time.fixedDeltaTime * 0.2f;
			}

			boat.rigidbody.velocity = boat.transform.forward * boat.rigidbody.velocity.magnitude;

			
			Camera.main.fieldOfView = Mathf.Lerp (Camera.main.fieldOfView, 60, 0.01f);
		}

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
		if (boat.eulerAngles.y <= 300 && boat.eulerAngles.y >= 180)
			boat.rigidbody.AddTorque(0, 2 * BoatTurn, 0);
		else if (boat.eulerAngles.y >= 60 && boat.eulerAngles.y <= 180)
			boat.rigidbody.AddTorque(0, 2 * -BoatTurn, 0);

		// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
		if (switchTimer >= switchDelay)
		{
			Camera.main.transform.rotation = Quaternion.Lerp (Camera.main.transform.rotation, Quaternion.Euler(boat.eulerAngles + new Vector3(20, 0, 0)), Time.fixedDeltaTime);
			Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, boat.position + new Vector3(-18 * boat.forward.x, 11, -18 * boat.forward.z), 5 * Time.fixedDeltaTime);
		}
	}

	void LateUpdate()
	{
		// Check if transiting to or from the player (not controlling the player).
		if (switchTimer != 0)
		{
			if (switchTimer >= switchDelay)
			{
				// Keep the player next to the switch door and facing the boat's front.
				this.transform.rotation = switchDoor.transform.rotation;
				this.transform.Rotate (0, 90, 0);

				if (switchDoor.name[0] == 'R')
				{
					this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + Vector3.right * this.transform.localScale.z;
				}
				else
				{
					this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + Vector3.left * this.transform.localScale.z;
				}

				// Ew ew ew quickfix.
				Camera.main.GetComponent<MouseLook>().rotationX = this.transform.eulerAngles.y;
				Camera.main.GetComponent<MouseLook>().rotationY = this.transform.eulerAngles.x;
			}
			else if (PlayerIsFocus == false)
			{
				// Keep the player next to the switch door and facing the door.
				this.transform.rotation = switchDoor.transform.rotation;
				if (switchDoor.name[0] == 'R')
				{
					this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + Vector3.right * this.transform.localScale.z;
				}
				else
				{
					this.transform.localPosition = new Vector3(switchDoor.localPosition.x, this.transform.localPosition.y, switchDoor.localPosition.z) + Vector3.left * this.transform.localScale.z;
					this.transform.Rotate (0, 180, 0);
				}
			}

			// Smoothstep lerp the rotation of the camera between the player's first person view direction and the world's forward direction.
			if (switchTimer < switchDelay)
				Camera.main.transform.rotation = Quaternion.Lerp (this.transform.rotation, Quaternion.Euler(boat.eulerAngles + new Vector3(20, 0, 0)), Mathf.SmoothStep (0, 1, switchTimer / switchDelay));
		
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

		CheckArcPlot();
	}

	void CheckArcPlot()
	{
		if (!(PlayerIsFocus == true && switchTimer == 0)) return;

		if (Input.GetButton("Alt Fire"))
		{			
			if (throwTimer == -1 && Weapons[1].Ammo != 0)
			{
				// Show the trajectory.
				Weapons[1].ViewModel.GetComponent<LineRenderer>().enabled = true;

				float step = 3;
				Vector3 position = Weapons[1].ViewModel.position;
				Vector3 direction = Vector3.Lerp (lastArc, boat.rigidbody.velocity + Camera.main.transform.forward * 20 + Vector3.up * 10, Time.deltaTime * 20);
				lastArc = direction;
				LineRenderer trajectory = Weapons[1].ViewModel.GetComponent<LineRenderer>();
				
				for (int i = 0; i < 50; ++i)
				{
					trajectory.SetPosition(i, position);
					position += direction * Time.deltaTime * step;
					direction += Physics.gravity * Time.deltaTime * step;
				}
			}
		}
	}

	void ChangeGun(int gun)
	{
		nextGun = gun;
		changeGunTimer = 0;
	}

	// Welp looks like I won't have time to use this.
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
		if (Input.GetButtonDown("Fire"))
		{
			// Don't allow the player to hold down fire, and don't allow shooting while the recoil timer is running.
			if (recoilTimer == -1 && Weapons[Gun].Ammo != 0 && AudioShoot.isPlaying == false)
			{
				RaycastHit hit;
				if (Physics.Raycast (hitscan, out hit, 10000, (1 << LayerMask.NameToLayer("Boat")) | (1 << LayerMask.NameToLayer ("Animal") |  (1 << LayerMask.NameToLayer ("Obstacle")))))
					if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Animal"))
						hit.transform.SendMessageUpwards("Shoot", Weapons[Gun].Damage);

				if (Weapons[Gun].Ammo > 0)
					--Weapons[Gun].Ammo;

				recoilTimer = 0;
				gunFlash.gameObject.SetActive(true);
				gunEffect.gameObject.SetActive(true);
				gunEffect.Rotate (0, 0, Random.Range (45f, 180f), Space.Self);

				AudioShoot.clip = Weapons[Gun].Sound;
				AudioShoot.pitch = Random.Range (0.9f, 1.1f);
				AudioShoot.Play ();
			}
		}

		if (Input.GetButtonDown("Alt Fire"))
		{
			lastArc = boat.rigidbody.velocity + Camera.main.transform.forward * 20 + Vector3.up * 10;
		}

		// Forced dynamite throwing code because I don't have time to add multiple weapons.
		if (Input.GetButtonUp("Alt Fire"))
		{
			if (PlayerIsFocus == true && switchTimer == 0)
				{
				// Hide the trajectory.
				Weapons[1].ViewModel.GetComponent<LineRenderer>().enabled = false;

				// Don't allow the player to hold down fire, and don't allow shooting while the recoil timer is running.
				if (throwTimer == -1 && Weapons[1].Ammo != 0)
				{				
					if (Weapons[1].Ammo > 0)
						--Weapons[1].Ammo;

					Rigidbody dynamite = ((GameObject)Instantiate(DynamitePrefab, Weapons[1].ViewModel.transform.position, Weapons[1].ViewModel.transform.rotation)).rigidbody;
					dynamite.velocity = this.transform.root.rigidbody.velocity;
					dynamite.AddForce(Camera.main.transform.forward * 20 + Vector3.up * 10, ForceMode.Impulse);
					Physics.IgnoreCollision(dynamite.collider, boat.root.Find("BoatHolder").Find("OuterHitMesh").collider);

					throwTimer = 0;
					AudioShoot.PlayOneShot (Weapons[1].Sound);
				}
			}
		}

		if (throwTimer != -1)
		{
			throwTimer += Time.deltaTime;
			
			if (throwTimer >= throwDelay)
			{
				throwTimer = -1;
			}
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
