using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour, AreaListener {

	DetectionArea princessBin;
	Animator princessGateAnimator;

	void Awake() {
		princessGateAnimator = GetComponent<Animator>();
		princessBin = transform.FindChild("PrincessBin").gameObject.GetComponent<DetectionArea>();
	}

	// Use this for initialization
	void Start () {
		princessBin.AddBinListener(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void ClosePrincessGate() {
		princessGateAnimator.SetTrigger("close");
	}

	public void OnAreaEnter(DetectionArea area, Collider2D collider) {
		Debug.Log (area);
		if(area == princessBin) {
			ClosePrincessGate();
		}
	}

	public void OnAreaExit(DetectionArea area, Collider2D collider) {

	}
}
