using UnityEngine;
using System.Collections;

public enum ShowHealthBar {
	Always, WhenDamaged, Never
}

public class Mortal : MonoBehaviour {

	public float TotalHealth = 100;
	public float CurrentHealth = 100;
	public GameObject HealthBar = null;
	public ShowHealthBar ShowHealthBar = ShowHealthBar.Always;

	private float fullHealthScale = 0f;
	private float lastDamage = 0f;

	private GameObject healthBarContent;

	// Use this for initialization
	void Start () {
		if(HealthBar != null) {
			fullHealthScale = HealthBar.transform.localScale.x;
			if(ShowHealthBar != ShowHealthBar.Always) {
				HealthBar.SetActive(false);
			}
			healthBarContent = HealthBar.transform.Find("Content").gameObject;
		}
		CurrentHealth = TotalHealth;
	}
	
	// Update is called once per frame
	void Update () {
		if(ShowHealthBar == ShowHealthBar.WhenDamaged && Time.time > lastDamage + 1f) {
			HealthBar.SetActive(false);
		}
	}

	public float GetLastDamage() {
		return lastDamage;
	}

	public void Hurt(float damage) {
		CurrentHealth -= damage;
		lastDamage = Time.time;
		if(HealthBar != null) {
			HealthBar.SetActive(true);
			healthBarContent.transform.localScale = new Vector2(CurrentHealth / TotalHealth * fullHealthScale, 1);
		}
	}
}
