using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class GroundCheck : MonoBehaviour {

    public GameObject GroundListener;
    public GroundListener listener;

	// Use this for initialization
	void Start () {
        if(GroundListener == null)
        {
            Debug.LogError("No GroundListener provided.");
            return;
        }
        listener = GroundListener.GetComponent<GroundListener>();
        if(listener == null)
        {
            Debug.LogError("Given GroundListener does not implement interface GroundListener.");
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnTriggerStay2D(Collider2D collider)
    {
        if(listener != null)
        {
            listener.OnGrounded();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(listener != null)
        {
            listener.OnAired();
        }
    }
}
