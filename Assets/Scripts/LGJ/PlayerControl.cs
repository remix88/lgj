﻿using UnityEngine;
using System.Collections;
using System;

public class PlayerControl : MonoBehaviour, GroundListener
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.
	[HideInInspector]
	public bool plunge = false;				// Condition for whether the player should plunge.
	[HideInInspector]
	public float h = 0;						// Horizontal movement

	private float lastJump = 0;

	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.

	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck1;			// A position marking where to check if the player is grounded.
    private Transform groundCheck2;         // A position marking where to check if the player is grounded.
    private Transform groundCheck3;         // A position marking where to check if the player is grounded.
    private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.

	private GameObject jumpBeacon;
	public GameObject PrincessFocus;
	private Transform body;
	private Mortal health;

	public float RopeDistance = 1;
	private int ropeSide = 0;

	private bool disabled = false;
	private float disabledUntil = 0f;
	private bool dead = false;

	private bool idle = true;
	private bool scared = false;
	private bool plunging = false;
	private bool hurt = false;

	void Start() {

	}

	void Awake()
	{
		// Setting up references.
		body = transform.Find("Body");
		groundCheck1 = transform.Find("groundCheck1");
        groundCheck2 = transform.Find("groundCheck2");
        groundCheck3 = transform.Find("groundCheck3");
        anim = body.GetComponent<Animator>();
		health = GetComponent<Mortal>();

		jumpBeacon = (GameObject)Resources.Load("Prefabs/JumpBeacon");
	}


	void Update()
	{
        ProcessInput();

		if(disabledUntil >= 0 && Time.time > disabledUntil) {
			disabled = false;
		}

		// Character states
		idle = h == 0;
		if(grounded && plunging) {
			plunging = false;
		}
		hurt = health.GetLastDamage() > 0 && Time.time - health.GetLastDamage() < 0.5f;
		
		// Animation states
		if(dead) {
			anim.SetTrigger("dead");
		} else if(hurt) {
			anim.SetTrigger("idlescared");
		} else if(plunging) {
			anim.SetTrigger("plunge");
		} else if(idle) {
			if(scared) {
				anim.SetTrigger("idlescared");
			} else {
				anim.SetTrigger("idle");
			}
		} else {
			if(scared) {
				anim.SetTrigger("walkscared");
			} else {
				anim.SetTrigger("walk");
			}
		}
	}

	public void SetRopeSide(int side) {
		ropeSide = side;
		int facing = facingRight ? 1 : -1;
		PrincessFocus.transform.localPosition = new Vector2(RopeDistance * ropeSide * facing, 0);
	}

	void ProcessInput() {	
		if(!disabled) {
			// Cache the horizontal input.
			h = Input.GetAxis("Horizontal");

			// If the jump button is pressed and the player is grounded then the player should jump.
			if(Input.GetButtonDown("Jump") && grounded && Time.time > lastJump + 0.5f) {
				jump = true;
			} else if (Input.GetAxis("Vertical") < 0 && !plunging) {
				plunge = true;
			}
		} else {
			h = 0;
		}
	}

	void FixedUpdate ()
	{
		if(disabled) {
			return;
		}
		// Add force, but less force when the player is nearing the maximum speed
		float strength = 1- (Mathf.Sign(h) * GetComponent<Rigidbody2D>().velocity.x) / maxSpeed;
		GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce * strength);

		// If the input is moving the player right and the player is facing left...
		if(h > 0 && !facingRight)
			// ... flip the player.
			Flip();
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(h < 0 && facingRight)
			// ... flip the player.
			Flip();

		// If the player should jump...
		if(jump)
		{
			// Play a random jump audio clip.
			if(jumpClips.Length > 0) {
				int i = UnityEngine.Random.Range(0, jumpClips.Length);
				AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
			}

			// Add a vertical force to the player.
			GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
			lastJump = Time.time;

			// Leave a beacon on which the princess will jump as well
			GameObject beacon = (GameObject)Instantiate(jumpBeacon);
			beacon.transform.position = transform.position;
		}

		if(plunge) {
			GetComponent<Rigidbody2D>().velocity = new Vector3(0, -8f);
			plunge = false;
			plunging = true;
		}

		if(Physics2D.OverlapCircle(transform.position, 2, 1 << LayerMask.NameToLayer("Princess"))) {
			scared = true;
		} else {
			scared = false;
		}
	}
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = body.localScale;
		theScale.x *= -1;
		body.localScale = theScale;
	}


	public IEnumerator Taunt()
	{
		// Check the random chance of taunting.
		float tauntChance = UnityEngine.Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}


	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = UnityEngine.Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}

	public void Die() {
		Disable (true);
		dead = true;
		GetComponent<Rigidbody2D>().isKinematic = true;
	}

	public void Disable(bool disable) {
		disabled = disable;
		disabledUntil = -1;
	}
	
	public void Disable(float seconds) {
		disabled = true;
		disabledUntil = Time.time + seconds;
	}

	void DangerImpact(Danger danger) {
		if(dead) {
			return;
		}
		GetComponent<Rigidbody2D>().AddForce(new Vector2(danger.HorizontalForce, danger.VerticalForce));
		if(danger.HorizontalForce != 0) {
			Disable(0.3f);
		}
	}
	
	void DangerImpactContinuous(Danger danger) {
		if(dead) {
			return;
		}
		GetComponent<Rigidbody2D>().AddForce(new Vector2(danger.HorizontalForce * Time.deltaTime * 10, danger.VerticalForce * Time.deltaTime * 10));
	}
	
	void DangerEffect(Danger danger) {
		if(dead) {
			return;
		}

        health.Hurt(danger.DamageOnTouch);
        hurt = true;
    }
	
	void DangerEffectContinuous(Danger danger) {
		if(dead) {
			return;
		}
		float damage = Time.deltaTime * danger.DamageOnStay;
		health.Hurt(damage);
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if(!hurt) {
			if(collider.transform.tag == "Danger") {
				Danger danger = collider.gameObject.GetComponent<Danger>();
				DangerEffect (danger);
				DangerImpact(danger);
			}
		}
	}
	
	void OnTriggerStay2D(Collider2D collider) {
		if(!hurt) {
			if(collider.transform.tag == "Danger") {
				if(collider.transform.tag == "Danger") {
					Danger danger = collider.gameObject.GetComponent<Danger>();
					DangerEffectContinuous (danger);
					DangerImpactContinuous(danger);
				}
			}
		}
	}
	
	void OnCollisionEnter2D(Collision2D collision) {
		if(!hurt) {
			if(collision.transform.tag == "Danger") {
				Danger danger = collision.gameObject.GetComponent<Danger>();
                DangerImpact(danger);
                DangerEffect(danger);
			}
		}
	}
	
	void OnCollisionStay2D(Collision2D collision) {
		if(!hurt) {
			if(collision.transform.tag == "Danger") {
				Danger danger = collision.gameObject.GetComponent<Danger>();
				DangerEffectContinuous(danger);
				DangerImpact(danger);
			}
		}
		if(collision.transform.tag == "Princess") {
			Princess princess = collision.gameObject.GetComponent<Princess>();
			health.Hurt(Time.deltaTime * princess.DamagePerSecond);
		}
	}

	void OnBecameInvisible() {
		if(gameObject.activeSelf) {
			health.Hurt(health.CurrentHealth);
		}
	}

    void GroundListener.OnGrounded()
    {
        grounded = true;
    }

    void GroundListener.OnAired()
    {
        grounded = false;
    }
}
