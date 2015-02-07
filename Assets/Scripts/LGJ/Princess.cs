using UnityEngine;
using System.Collections;

public class Princess : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the princess is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the princess should jump.
	
	
	public float moveForce = 365f;			// Amount of force added to move the princess left and right.
	public float maxSpeed = 5f;				// The fastest the princess can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the princess jumps.
	public float jumpForce = 1000f;			// Amount of force added when the princess jumps.
	public AudioClip[] taunts;				// Array of clips for when the princess taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.
	
	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the princess is grounded.
	private bool grounded = false;			// Whether or not the princess is grounded.
	private Animator anim;					// Reference to the princess's animator component.
	
	void Start() {

	}

	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
	}
	
	
	void Update()
	{
		// The princess is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  
	}
	
	
	void FixedUpdate ()
	{		
		// The Speed animator parameter is set to the absolute value of the horizontal input.
		anim.SetFloat("Speed", Mathf.Abs(rigidbody2D.velocity.x));
		
		// If the princess's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
			// ... set the princess's velocity to the maxSpeed in the x axis.
			rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);
		
		// If the input is moving the princess right and the princess is facing left...
		if(rigidbody2D.velocity.x > .5 && !facingRight)
			// ... flip the princess.
			Flip();
		// Otherwise if the input is moving the princess left and the princess is facing right...
		else if(rigidbody2D.velocity.x < -.5 && facingRight)
			// ... flip the princess.
			Flip();
		
		// If the princess should jump...
		if(jump)
		{
			// Set the Jump animator trigger parameter.
			anim.SetTrigger("Jump");
			
			// Play a random jump audio clip.
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);
			
			// Add a vertical force to the princess.
			rigidbody2D.AddForce(new Vector2(0f, jumpForce));
			
			// Make sure the princess can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}
	}	
	
	void Flip ()
	{
		// Switch the way the princess is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the princess's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	public void Jump() {
		jump = true;
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
}
