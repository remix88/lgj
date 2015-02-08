using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
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
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
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
		groundCheck = transform.Find("groundCheck");
		anim = body.GetComponent<Animator>();
		health = GetComponent<Mortal>();

		jumpBeacon = (GameObject)Resources.Load("Prefabs/JumpBeacon");
	}


	void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		int layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("Princess"));
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, layerMask); 

		ProcessInput();

		if(Time.time > disabledUntil) {
			disabled = false;
		}

		// Character states
		idle = h == 0;
		if(grounded && plunging) {
			plunging = false;
		}
		hurt = health.GetLastDamage() > 0 && Time.time - health.GetLastDamage() < 0.5f;
		
		// Animation states
		if(hurt) {
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
			} else if (Input.GetKeyDown(KeyCode.DownArrow) && !plunging) {
				plunge = true;
			}
		} else {
			h = 0;
		}
	}

	void FixedUpdate ()
	{		
		// Add force, but less force when the player is nearing the maximum speed
		float strength = 1- (Mathf.Sign(h) * rigidbody2D.velocity.x) / maxSpeed;
		rigidbody2D.AddForce(Vector2.right * h * moveForce * strength);
		
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
				int i = Random.Range(0, jumpClips.Length);
				AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
			}

			// Add a vertical force to the player.
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
			lastJump = Time.time;

			// Leave a beacon on which the princess will jump as well
			GameObject beacon = (GameObject)Instantiate(jumpBeacon);
			beacon.transform.position = transform.position;
		}

		if(plunge) {
			rigidbody2D.velocity = new Vector3(0, -8f);
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
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!audio.isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				audio.clip = taunts[tauntIndex];
				audio.Play();
			}
		}
	}


	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}

	void Disable(float seconds) {
		disabled = true;
		disabledUntil = Time.time + seconds;
	}

	void OnTriggerEnter2D(Collider2D collider) {
		if(!hurt) {
			if(collider.transform.tag == "Danger") {
				Danger danger = collider.gameObject.GetComponent<Danger>();
				health.Hurt(danger.DamageOnTouch);
				hurt = true;
			}
		}
	}

	void OnTriggerStay2D(Collider2D collider) {
		if(collider.transform.tag == "Danger") {
			Danger danger = collider.gameObject.GetComponent<Danger>();
			float damage = Time.deltaTime * danger.DamagePerSecond;
			health.Hurt(damage);
		}
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if(!hurt) {
			if(collision.transform.tag == "Danger") {
				Danger danger = collision.gameObject.GetComponent<Danger>();
				health.Hurt(danger.DamageOnTouch);
				hurt = true;
				if(rigidbody2D.velocity.y < 0.5f) {
					Disable(0.3f);
					rigidbody2D.AddRelativeForce(-100 * Vector2.right);
				}
			}
		}
	}

	void OnCollisionStay2D(Collision2D collision) {
		if(collision.transform.tag == "Princess") {
			Princess princess = collision.gameObject.GetComponent<Princess>();
			health.Hurt(Time.deltaTime * princess.DamagePerSecond);
		}
	}
}
