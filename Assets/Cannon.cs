using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour {
	
	public float ShootingInterval = 2;
	public float Power = 20f;

	private float LastShot = 0f;
	private GameObject bullet;
	private GameObject aim;

	// Use this for initialization
	void Start () {
		bullet = transform.FindChild("Bullet").gameObject;
		aim = transform.FindChild("Aim").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if(Time.time > LastShot + ShootingInterval) {
			Shoot ();
			LastShot = Time.time;
		}
	}

	void FixedUpdate() {
		if(rigidbody2D != null) {
			rigidbody2D.AddForce(rigidbody2D.velocity * -100);
		}
	}

	public void Shoot() {
		GameObject shot = (GameObject) GameObject.Instantiate(bullet);
		shot.transform.position = bullet.transform.position;
		Physics2D.IgnoreCollision(shot.collider2D, collider2D);

		Vector2 dir = (aim.transform.position - bullet.transform.position).normalized;
		shot.SetActive(true);
		shot.rigidbody2D.velocity = dir * Power;
	}
}
