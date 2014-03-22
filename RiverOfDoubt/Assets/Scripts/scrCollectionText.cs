using UnityEngine;
using System.Collections;

public class scrCollectionText : MonoBehaviour
{
	private float expiryTimer = 0;
	private Vector3 startPosition, endPosition;

	// Use this for initialization
	void Start ()
	{
		startPosition = this.transform.position;
		endPosition = this.transform.position + this.transform.up * 3;
	}
	
	// Update is called once per frame
	void Update ()
	{
		expiryTimer += Time.deltaTime;
		if (expiryTimer > 3)
			Destroy(this.gameObject);

		this.transform.position = Vector3.Lerp (startPosition, endPosition, expiryTimer / 3);
		Color temp = this.renderer.material.color;
		temp.a = 1 - expiryTimer / 3;
		this.renderer.material.color = temp;
	}
}
