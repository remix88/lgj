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
	public AudioClip[] jumpClips;			// Array of clips for when the princess jumps.
	public float jumpForce = 1000f;			// Amount of force added when the princess jumps.
	public AudioClip[] taunts;				// Array of clips for when the princess taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.

	private bool grounded = false;			// Whether or not the player is grounded.

	public float JumpProbability = 0.2f;

	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Animator anim;					// Reference to the princess's animator component.

	public PlayerControl Player;

	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private Transform body;
	private LineRenderer line;
	private GameObject ropeAttach;
	private SpringJoint2D rope;
	private Mortal health;

	private int state = 0;
	private bool hurt = false;
	private bool lasso = false;

	void Start() {
		//Physics2D.IgnoreCollision(Player.GetComponent<BoxCollider2D>(), GetComponent<BoxCollider2D>());
		InvokeRepeating("Taunt", 1, 5);
		InvokeRepeating("MaybeJump", 1, 5);
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
	
		if(state == 3) {
			anim.SetTrigger("walkworst");
		} else if (state == 2) {
			anim.SetTrigger("walkworse");
		} else if (state == 1) {
			anim.SetTrigger("walkbad");
		} else {
			anim.SetTrigger("walk");
		}
	}

	void FixedUpdate ()
	{		
		// If the princess is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		float h = Mathf.Sign(Player.PrincessFocus.transform.position.x - transform.position.x);
		// ... add a force to the princess.
		float strength = 1- (Mathf.Sign(h) * rigidbody2D.velocity.x) / maxSpeed;
		rigidbody2D.AddForce(Vector2.right * h * moveForce * strength);

		// If the input is moving the princess right and the princess is facing left...
		if(facingRight && transform.position.x > Player.transform.position.x)
			// ... flip the princess.
			Flip();
		// Otherwise if the input is moving the princess left and the princess is facing right...
		else if(!facingRight && transform.position.x < Player.transform.position.x)
			// ... flip the princess.
			Flip();
		
		// If the princess should jump...
		if(jump)
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
		if(transform.position.x > Player.transform.position.x + 2) {
			Player.SetRopeSide(-1);
		} else if(transform.position.x < Player.transform.position.x - 2) {
			Player.SetRopeSide(1);
		}

		// Use lasso when falling
		if(!lasso && transform.position.y < Player.transform.position.y && rigidbody2D.velocity.y < -0.5) {
			ThrowLasso();
		}
		if(lasso && transform.position.y > Player.transform.position.y) {
			StopLasso();
		}
	}

	void ThrowLasso() {
		lasso = true;
		rope.enabled = true;
	}
	
	void DrawLasso() {
		line.SetVertexCount(2);
		line.SetPosition(0, ropeAttach.transform.position);
		line.SetPosition(1, Player.transform.position);
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
		if(grounded && Random.Range(0f, 100f) > JumpProbability) {
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
}
