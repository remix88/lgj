using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public float RotationOffset = 0;
	public float MinSpeedForRotation = 5;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Vector3 diff = GetComponent<Rigidbody2D>().velocity.normalized;
		float mag = GetComponent<Rigidbody2D>().velocity.magnitude;

		if(mag > MinSpeedForRotation) {
			float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
			Quaternion euler = Quaternion.Euler(0f, 0f, rot_z - 90 + RotationOffset);
			transform.rotation = euler;
		}
	}
}
