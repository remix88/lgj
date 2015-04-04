using UnityEngine;
using System.Collections;

public class CleanInvisible : MonoBehaviour {

	public static float DistanceFromPlayer = 30f;

	private GameObject Knight;

	// Use this for initialization
	void Start () {

	}

	void Awake() {
		Knight = GameObject.Find("Knight");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnBecameInvisible() {
		if(gameObject.activeSelf) {
			Destroy (gameObject);
		}
	}

	void OnEnable()
	{
		if(Mathf.Abs(Knight.transform.position.x - transform.position.x) > DistanceFromPlayer) {
			Destroy(gameObject);
		}
	}
}
