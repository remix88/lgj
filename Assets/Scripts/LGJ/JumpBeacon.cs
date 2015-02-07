using UnityEngine;
using System.Collections;

public class JumpBeacon : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
