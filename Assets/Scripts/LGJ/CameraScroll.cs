using UnityEngine;
using System.Collections;

public class CameraScroll : MonoBehaviour {

	public float Speed = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 delta = Vector3.right * -1 * Speed * Time.deltaTime;
		transform.position += delta;
	}
}
