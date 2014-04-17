using UnityEngine;
using System.Collections;

public class scrDynamite : MonoBehaviour
{
	public GameObject ExplosionPrefab;
	public GameObject SmallSplashPrefab;
	public GameObject BigSplashPrefab;

	void Start ()
	{
		this.rigidbody.AddTorque(Random.rotation.eulerAngles, ForceMode.Impulse);
	}

	void Update ()
	{
		if (this.transform.position.y < -0.3f)
		{
			if (audio.isPlaying == true)
			{
				Instantiate(SmallSplashPrefab, this.transform.position, SmallSplashPrefab.transform.rotation);
				audio.Stop ();
				Destroy (this.gameObject, 4);
			}
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
		{
			// Instantiate explosion.
			Instantiate(ExplosionPrefab, this.transform.position, this.transform.rotation);
			Destroy (this.gameObject);
		}
		else
		{
			OnTriggerEnter(collision.collider);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.transform.root.name == "Hut_A(Clone)" || other.transform.root.name == "Hut_B(Clone)")
		{
			// Instantiate explosion.
			GameObject explosion = (GameObject)Instantiate(ExplosionPrefab, this.transform.position, this.transform.rotation);
			Destroy (this.gameObject);
			Destroy (explosion.collider, 0.5f);
			Destroy (explosion, 5.0f);

			// Detach all children and give them rigidbodies and torque. Destroy them after 3 seconds.
			Transform[] children = other.transform.root.GetComponentsInChildren<Transform>();
			for (int i = 0; i < children.Length; i++)
			{
				children[i].parent = null;
				if (children[i].name == "Native(Clone)")
				{
					children[i].GetComponent<scrNative>().Kill();
					continue;
				}
				
				if (children[i].rigidbody == null)
				{
					children[i].gameObject.AddComponent<Rigidbody>();
				}
				children[i].rigidbody.isKinematic = false;
				children[i].rigidbody.AddTorque(Random.Range(-50, 51), Random.Range (-50, 51), Random.Range (-50, 51));
				if (children[i].collider)
				{
					foreach (Collider c in children[i].GetComponents<Collider>())
						Destroy (c);
				}
				Destroy (children[i].gameObject, 3);
			}
			return;
		}

		if (other.name == "Whirlpool")
			return;

		if (other.name != "Explosion(Clone)" && (other.gameObject.layer == LayerMask.NameToLayer("Obstacle") || other.gameObject.layer == LayerMask.NameToLayer("Animal") || other.gameObject.layer == LayerMask.NameToLayer("Boat") || other.gameObject.layer == LayerMask.NameToLayer("Interactive") || other.name != "Water" && other.name.Contains("Section") == false && other.transform.root.name.Contains ("Section")))
		{
			// Instantiate explosion.
			GameObject explosion = (GameObject)Instantiate(ExplosionPrefab, this.transform.position, this.transform.rotation);
			Destroy (this.gameObject);
			Destroy (explosion.collider, 0.5f);
			Destroy (explosion, 5.0f);

			if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
			{
				Instantiate(BigSplashPrefab, other.transform.position, BigSplashPrefab.transform.rotation);

				// Make the obstacle fall.
				if (other.transform.root.rigidbody != null)
					other.transform.root.rigidbody.useGravity = true;
				
				// If the other obstacle has a hinge, destroy the hinge.
				if (other.transform.root.hingeJoint != null)
					Destroy(other.transform.root.hingeJoint);
				
				// Destroy the obstacle after 3 seconds.
				Destroy (other.transform.root.gameObject, 3);
				
				// Instantly destroy the obstacle's collider so it doesn't collide again.
				if (other != null)
					Destroy (other);
			}
		}
	}
}
