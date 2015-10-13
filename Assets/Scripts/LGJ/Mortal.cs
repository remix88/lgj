using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ShowHealthBar {
	Always, WhenDamaged, Never
}

public interface HealthListener {
	void OnHealthChange(Mortal health, float oldValue);
}

public class Mortal : MonoBehaviour {

	public float TotalHealth = 100;
	public float CurrentHealth = 100;
	public GameObject HealthBar = null;
	public ShowHealthBar ShowHealthBar = ShowHealthBar.Always;

	private float fullHealthScale = 0f;
	private float lastDamage = 0f;

	private GameObject healthBarContent;

	private List<HealthListener> listeners = new List<HealthListener>();

	// Use this for initialization
	void Start () {
		if(HealthBar != null) {
			if(ShowHealthBar != ShowHealthBar.Always) {
				HealthBar.SetActive(false);
			}
			healthBarContent = HealthBar.transform.Find("Content").gameObject;
            fullHealthScale = healthBarContent.transform.localScale.x;
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
		float oldValue = CurrentHealth;
		CurrentHealth -= damage;
		lastDamage = Time.time;
		if(HealthBar != null) {
			if(ShowHealthBar == ShowHealthBar.WhenDamaged) {
				HealthBar.SetActive(true);
			}
			healthBarContent.transform.localScale = new Vector2(CurrentHealth / TotalHealth * fullHealthScale, 1);
			if(healthBarContent.transform.localScale.x < -1) {
				healthBarContent.transform.localScale = new Vector2(0, 1);
			}
		}
		foreach(HealthListener listener in listeners) {
			listener.OnHealthChange(this, oldValue);
		}
	}

	public void AddHealthListener(HealthListener listener) {
		listeners.Add(listener);
	}
}
