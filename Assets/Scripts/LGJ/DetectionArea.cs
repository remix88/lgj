using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface AreaListener {
	void OnAreaEnter(DetectionArea area, Collider2D collider);
	void OnAreaExit(DetectionArea area, Collider2D collider);
}

public class DetectionArea : MonoBehaviour {

	List<AreaListener> areaListeners = new List<AreaListener>();

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddBinListener(AreaListener listener) {
		areaListeners.Add(listener);
	}

	void OnTrigger2DEnter(Collider2D collider) {
		foreach(AreaListener listener in areaListeners) {
			listener.OnAreaEnter(this, collider);
		}
	}
}
