using UnityEngine;
using System.Collections;

public class Princess : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the princess is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the princess should jump.

	public float DamagePerSecond = 10f;
	public float moveForce = 365f;			// Amount of force added to move the princess left and right.
	public float maxSpeed = 5f;				// The fastest the princess can travel in the x axis.
	public float LassoDistance = 10f;		// The maximum distance at which the Princess can use the lasso.
	public AudioClip[] jumpClips;			// Array of clips for when the princess jumps.
	public float jumpForce = 1000f;			// Amount of force added when the princess jumps.
	public AudioClip[] taunts;				// Array of clips for when the princess taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.

	private bool grounded = false;			// Whether or not the player is grounded.

	public float JumpChance = 20;

	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Animator anim;					// Reference to the princess's animator component.

	public PlayerControl Knight;

	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private Transform body;
	private LineRenderer line;
	private GameObject ropeAttach;
	private SpringJoint2D rope;
	private Mortal health;

	private int state = 0;
	private bool hurt = false;
	private bool lasso = false;
	private bool disabled = false;
	private float disabledUntil = 0f;

	void Start() {
		//Physics2D.IgnoreCollision(Player.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
		InvokeRepeating("Taunt", 1, 5);
		InvokeRepeating("MaybeJump", 1, 3);

		if(Knight == null) {
			Debug.LogError("Knight was not specified in Princess Inspector");
		} else {
			rope.connectedBody = Knight.gameObject.rigidbody2D;
			rope.enabled = false;
		}
	}

	void Awake()
	{
		body = transform.Find("Body");
		groundCheck = transform.Find("groundCheck");
		anim = body.gameObject.GetComponent<Animator>();
		line = GetComponent<LineRenderer>();
		ropeAttach = transform.Find("RopeAttach").gameObject;
		rope = GetComponent<SpringJoint2D>();
		health = GetComponent<Mortal>();
	}
	
	
	void Update()
	{
		if(lasso) {
			DrawLasso();
		}
		int layerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("Princess"));
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, layerMask);
		if(health.CurrentHealth < .25f * health.TotalHealth) {
			state = 3;
		} else if (health.CurrentHealth < 0.5f * health.TotalHealth) {
			state = 2;
		} else if (health.CurrentHealth < 0.75f * health.TotalHealth) {
			state = 1;
		} else {
			state = 0;
		}
		anim.SetInteger("state", state);

		hurt = health.GetLastDamage() > 0 && Time.time - health.GetLastDamage() < 0.5f;
		anim.SetBool("hurt", hurt);
		anim.SetBool("jump", !grounded);

		if(Time.time > disabledUntil) {
			disabled = false;
		}
	}

	void FixedUpdate ()
	{		
		if(Knight == null || disabled) {
			return;
		}
		// If the princess is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		float h = Mathf.Sign(Knight.PrincessFocus.transform.position.x - transform.position.x);
		// ... add a force to the princess.
		float strength = 1- (Mathf.Sign(h) * rigidbody2D.velocity.x) / maxSpeed;
		rigidbody2D.AddForce(Vector2.right * h * moveForce * strength);

		// If the input is moving the princess right and the princess is facing left...
		if(facingRight && transform.position.x > Knight.transform.position.x)
			// ... flip the princess.
			Flip();
		// Otherwise if the input is moving the princess left and the princess is facing right...
		else if(!facingRight && transform.position.x < Knight.transform.position.x)
			// ... flip the princess.
			Flip();
		
		// If the princess should jump...
		if(jump && grounded)
		{			
			// Play a random jump audio clip.
			if(jumpClips.Length > 0) {
				int i = Random.Range(0, jumpClips.Length);
				AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
			}
			
			// Add a vertical force to the princess.
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			
			// Make sure the princess can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}

		// Change rope side to keep moving
		if(transform.position.x > Knight.transform.position.x + 2) {
			Knight.SetRopeSide(-1);
		} else if(transform.position.x < Knight.transform.position.x - 2) {
			Knight.SetRopeSide(1);
		}

		// Use lasso when falling
		if(!lasso 
		   && transform.position.y < Knight.transform.position.y - 2f 
		   && rigidbody2D.velocity.y < -0.5
		   && Vector2.Distance(transform.position, Knight.transform.position) < LassoDistance) {
			ThrowLasso();
		}
		if(lasso && transform.position.y > Knight.transform.position.y - 2f) {
			StopLasso();
		}
	}

	void Disable(float seconds) {
		disabled = true;
		disabledUntil = Time.time + seconds;
	}

	void ThrowLasso() {
		lasso = true;
		rope.enabled = true;
	}
	
	void DrawLasso() {
		line.SetVertexCount(2);
		line.SetPosition(0, ropeAttach.transform.position);
		line.SetPosition(1, Knight.transform.position);
	}
	
	void StopLasso() {
		line.SetVertexCount(0);
		lasso = false;
		rope.enabled = false;
	}
	
	void Flip ()
	{
		// Switch the way the princess is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the princess's x local scale by -1.
		Vector3 theScale = body.localScale;
		theScale.x *= -1;
		body.localScale = theScale;
	}

	public void MaybeJump() {
		if(grounded && Random.Range(0f, 100f) < JumpChance) {
			Jump ();
		}
	}

	public void Jump() {
		jump = true;
	}
	
	public void Taunt()
	{
		if(taunts.Length == 0) {
			return;
		}
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{			
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

	void DangerImpact(Danger danger) {
		rigidbody2D.AddForce(new Vector2(0, danger.VerticalForce));
	}
	
	void DangerImpactContinuous(Danger danger) {
		rigidbody2D.AddForce(new Vector2(0, danger.VerticalForce * Time.deltaTime * 10));
	}
	
	void DangerEffect(Danger danger) {
		health.Hurt(danger.DamageOnTouch);
		hurt = true;
		if(rigidbody2D.velocity.y < 0.5f) {
			Disable(0.3f);
			rigidbody2D.AddRelativeForce(-100 * Vector2.right);
		}
	}
	
	void DangerEffectContinuous(Danger danger) {
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
				DangerEffect(danger);
				DangerImpact(danger);
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
	}
}
