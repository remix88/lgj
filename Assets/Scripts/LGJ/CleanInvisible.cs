using UnityEngine;
using System.Collections;

public class CleanInvisible : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnBecameInvisible() {
		if(gameObject.activeSelf) {
			Destroy (gameObject);
		}
	}
}
