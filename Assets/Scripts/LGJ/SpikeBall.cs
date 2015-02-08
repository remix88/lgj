using UnityEngine;
using System.Collections;

public class SpikeBall : MonoBehaviour {

	GameObject pivot;
	GameObject ball;
	LineRenderer line;

	// Use this for initialization
	void Start () {
		pivot = transform.FindChild("Pivot").gameObject;
		ball = transform.FindChild("Ball").gameObject;
		line = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
		line.SetVertexCount(2);
		line.SetPosition(0, pivot.transform.position);
		line.SetPosition(1, ball.transform.position);
	}
}
