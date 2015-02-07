using UnityEngine;
using System.Collections;

public class JumpBeacon : MonoBehaviour {

	float start;
	float lifespan = 2f;

	// Use this for initialization
	void Start () {
		start = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > start + lifespan) {
			Destroy(gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D collider) {
		// Princess enters trigger, make her jump
		if(collider.transform.tag == "Princess") {
			Princess princess = collider.gameObject.GetComponent<Princess>();
			princess.Jump();
			Destroy(gameObject);
		}
	}
}
