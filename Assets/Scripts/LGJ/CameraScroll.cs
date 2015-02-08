using UnityEngine;
using System.Collections;

public class CameraScroll : MonoBehaviour {

	public float Speed = 1f;
	public bool Scroll = false;

	BoxCollider2D boxCollider;

	// Use this for initialization
	void Start () {
		boxCollider = GetComponent<BoxCollider2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		if(Scroll) {
			Vector3 delta = Vector3.right * -1 * Speed * Time.deltaTime;
			transform.position += delta;
		}
	}

	public void StartScrolling() {
		if(boxCollider != null) {
			boxCollider.enabled = true;
		}
		Scroll = true;
	}
}
