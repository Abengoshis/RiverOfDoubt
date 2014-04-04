using UnityEngine;
using System.Collections;

public class scrCrocodile : scrAnimal
{
	 

	private enum SinkState { Sink, Rise, Stable };
	private SinkState sink = SinkState.Stable;
	private const float SINK_DEPTH = -2.0f;
	private const float SINK_SPEED = -3.0f;

	private GameObject boat;
	private bool chasing = false;
	private const float CHASE_SPEED = 20.0f;

	private Transform[] tail = new Transform[3];
	private const float WIGGLE_FREQUENCY = 4.0f;
	private const float WIGGLE_AMPLITUDE = 10.0f;
	private float wiggleOffset;

	private float rollOverTimer = 0;
	private const float ROLL_OVER_DELAY = 4f;
	private bool rollLeft;

	// Use this for initialization
	void Start ()
	{
		boat = GameObject.Find ("Boat");

		// Each tail affects the other. Instead of using a physics hinge I wiggle the tail manually.
		tail[0] = this.transform.Find ("Croc").Find ("Tail_0");
		tail[1] = tail[0].Find ("Tail_1");
		tail[2] = tail[1].Find ("Tail_2");

		rollLeft = Random.Range (0, 2) == 0;
		wiggleOffset = Random.Range (0.0f, 360.0f);
	}
	
	// Update is called once per frame
	protected override void Update ()
	{
		if (Time.timeScale == 0)
			return;

		if (Health <= 0)
		{
			if (rollOverTimer < ROLL_OVER_DELAY)
			{
				rollOverTimer += Time.deltaTime;
				if (rollOverTimer > ROLL_OVER_DELAY)
					rollOverTimer = ROLL_OVER_DELAY;

				// Roll onto back.
				this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, Mathf.SmoothStep (0, rollLeft ? 180 : -180, rollOverTimer / ROLL_OVER_DELAY));
			}

			this.rigidbody.drag = 20;
			this.rigidbody.useGravity = true;

			return;
		}

		if (chasing == false)
		{
			// Check if the player has passed the crocodile.
			if (boat.transform.position.z > this.transform.position.z)
			{
				// Trigger the crocodile's attack.
				GetAggro();
			}
		}
		else
		{
			// Chase the player.
			if (this.rigidbody.velocity.magnitude < CHASE_SPEED)
			{
				Vector3 direction = boat.transform.position - new Vector3(0, 0, boat.transform.localScale.z) - this.transform.position;
				direction.Normalize();

				this.rigidbody.AddForce(direction * CHASE_SPEED, ForceMode.Force);
			}
		}

		if (sink == SinkState.Sink)
		{
			// Sink below the water.
			if (this.transform.position.y > SINK_DEPTH)
			{
				this.transform.Translate (0, SINK_SPEED * Time.deltaTime, 0);
			}

			sink = SinkState.Rise;
		}
		else if (sink == SinkState.Rise)
		{
			// Rise above the water until at y = 0.
			if (this.transform.position.y < 0)
			{
				this.transform.Translate (0, -SINK_SPEED * Time.deltaTime, 0);
			}
			else
			{
				sink = SinkState.Stable;
			}
		}
		else
		{
			// Slowly bob up and down.
			this.transform.position = new Vector3(this.transform.position.x,
			                                      0.1f * Mathf.Sin (Time.time),
			                                      this.transform.position.z);
		}

		for (int i = 0; i < tail.Length; ++i)
		{
			tail[i].eulerAngles = this.transform.eulerAngles + new Vector3(0, Mathf.Sin (wiggleOffset + Time.time * WIGGLE_FREQUENCY - i) * WIGGLE_AMPLITUDE, 0);
		}

		base.Update();
	}

	public override void Shoot (float damage)
	{
		this.rigidbody.AddForce(this.transform.forward * -10, ForceMode.Impulse);

		// Force the crocodile to attack.
		if (chasing == false)
			GetAggro();
		
		base.Shoot (damage);
	}
	
	public override void Kill ()
	{
		// Stop facing the player.
		foreach (scrFacePlayer enabledFaceScript in this.GetComponentsInChildren<scrFacePlayer>())
			enabledFaceScript.enabled = false;

		foreach (Collider collider in this.GetComponentsInChildren<Collider>())
			Destroy (collider);

		base.Kill ();
	}

	private void GetAggro()
	{
		// Start facing the player.
		foreach (scrFacePlayer disabledFaceScript in this.GetComponentsInChildren<scrFacePlayer>())
			disabledFaceScript.enabled = true;
		
		// Start chasing the player.
		chasing = true;
	}

	void OnCollisionEnter(Collision collision)
	{
		if (Health > 0 && collision.transform.root.name == "Boat") 
		{
			this.rigidbody.AddForce(this.transform.forward * -10, ForceMode.Impulse);

			// Force the crocodile to attack.
			if (chasing == false)
				GetAggro();
		}
	}

	void OnTriggerStay(Collider other)
	{
		if (other.transform.root.name.Contains ("Rock"))
		{
			sink = SinkState.Sink;
		}
	}
}
