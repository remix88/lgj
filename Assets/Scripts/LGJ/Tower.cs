using UnityEngine;
using System.Collections;

public class Tower : MonoBehaviour, AreaListener {

	DetectionArea princessBin;
	Animator princessGateAnimator;
	Animator gateToFreedomAnimator;

	void Awake() {
		princessGateAnimator = transform.FindChild("PrincessGate").GetComponent<Animator>();
		gateToFreedomAnimator = transform.FindChild("GateToFreedom").GetComponent<Animator>();
		princessBin = transform.FindChild("PrincessBin").gameObject.GetComponent<DetectionArea>();
	}

	// Use this for initialization
	void Start () {
		princessBin.AddAreaListener(this);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void ClosePrincessGate() {
		princessGateAnimator.SetTrigger("close");
	}

	void OpenPrincessGate() {
		princessGateAnimator.SetTrigger("open");
	}

	void OpenGateToFreedom() {
		gateToFreedomAnimator.SetTrigger("open");
	}

	void CloseGateToFreedom() {
		gateToFreedomAnimator.SetTrigger("close");
	}

	public void OnAreaEnter(DetectionArea area, Collider2D collider) {
		if(area == princessBin) {
			ClosePrincessGate();
			OpenGateToFreedom();
		}
	}

	public void OnAreaExit(DetectionArea area, Collider2D collider) {
		if(area == princessBin) {
			OpenPrincessGate();
			CloseGateToFreedom();
		}
	}
}
