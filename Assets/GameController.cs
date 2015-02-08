using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour, HealthListener {

	public PlayerControl Knight;
	public Princess Princess;
	public GameObject CameraRig;
	public GameObject RetryCanvas;

	private Mortal knightHealth;
	private Mortal princessHealth;

	float startPosition = 0;

	public float score = 0;
	public float distance = 0;

	// Use this for initialization
	void Start () {
		knightHealth = Knight.GetComponent<Mortal>();
		knightHealth.AddHealthListener(this);

		princessHealth = Princess.GetComponent<Mortal> ();
		princessHealth.AddHealthListener(this);

		startPosition = CameraRig.transform.position.x;
	}
	
	// Update is called once per frame
	void Update () {
		distance = startPosition - CameraRig.transform.position.x;
	}

	public void OnHealthChange(Mortal health, float oldValue) {
		// Knight health
		if(health.gameObject == Knight.gameObject) {
			if(health.CurrentHealth <= 0) {
				GameOver();
			}
		// Princess health
		} else if (health.gameObject == Princess.gameObject) {
			score = health.TotalHealth - health.CurrentHealth;
		}
	}

	public void StartGame() {
		Application.LoadLevel (Application.loadedLevelName);
	}

	public void GameOver() {
		float delay = 1.0f;
		Knight.Die();

		CameraScroll scroll = CameraRig.GetComponent<CameraScroll>();
		if(scroll != null) {
			scroll.Scroll = false;
		}
		Invoke("ShowScore", delay);
	}

	void ShowScore() {
		RetryCanvas.SetActive(true);
	}
}
