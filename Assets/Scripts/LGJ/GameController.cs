using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour, HealthListener, AreaListener {

	public PlayerControl Knight;
	public Princess Princess;
	public GameObject CameraRig;
	public GameObject RetryCanvas;
	public GameObject WinCanvas;
	public ComicController ComicController;
	public bool ShowIntro = true;
	public DetectionArea FinishArea;

	private Mortal knightHealth;
	private Mortal princessHealth;

	float startPosition = 0;

	float score = 0;
	float distance = 0;

	bool getReady = false;

	// Use this for initialization
	void Start () {
		knightHealth = Knight.GetComponent<Mortal>();
		knightHealth.AddHealthListener(this);

		princessHealth = Princess.GetComponent<Mortal> ();
		princessHealth.AddHealthListener(this);

		startPosition = CameraRig.transform.position.x;
	
		FinishArea.AddAreaListener(this);

		StartLevel();
	}

	public void StartLevel() {
		if(ShowIntro) {
			ComicController.StartComic();
		} else {
			GetReady();
		}
		
		Knight.Disable(true);
		Princess.Disable(true);
	}
	
	// Update is called once per frame
	void Update () {
		distance = startPosition - CameraRig.transform.position.x;
		if(getReady && Input.GetAxis("Horizontal") != 0) {
			StartGame();
		}
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

	void OnLevelWasLoaded(int level) {
		ShowIntro = false;
		GetReady ();
	}

	public void LoadGame() {
		Application.LoadLevel (Application.loadedLevelName);
	}

	public void StartGame() {
		CameraRig.GetComponent<CameraScroll>().StartScrolling();
		Knight.Disable(false);
		Princess.Disable(false);
		getReady = false;
	}

	public void GetReady() {
		getReady = true;
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
		GameObject scoreText = RetryCanvas.transform.FindChild("ScoreVar").gameObject;
		scoreText.GetComponent<Text>().text = score+"";

		RetryCanvas.SetActive(true);
	}

	void StartComic() {
		ComicController.StartComic();
		ShowIntro = false;
	}

	public void Win() {
		GameObject scoreText = WinCanvas.transform.FindChild("ScoreVar").gameObject;
		scoreText.GetComponent<Text>().text = score+"";
		
		WinCanvas.SetActive(true);
		Knight.Disable(true);
		Princess.Angry(true);
	}

	public void OnAreaEnter(DetectionArea area, Collider2D collider) {
		if(area == FinishArea && collider.gameObject == Knight.gameObject) {
			Win ();
		}
	}

	public void OnAreaExit(DetectionArea area, Collider2D collider) {

	}
}
