using UnityEngine;
using System.Collections;

public class GameCanvas : MonoBehaviour {

    public float ouchTime = 1f;

    float ouch = 0f;
    GameObject ouchObject;
    Renderer ouchRenderer;

    // Use this for initialization
    void Start () {
        ouchObject = transform.Find("Ouch").gameObject;
        ouchRenderer = (Renderer)ouchObject.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if(ouch <= 0)
        {
            ouch = 0;
            ouchObject.SetActive(false);
        } else
        {
            ouch -= Time.deltaTime / ouchTime;
            Color color = ouchRenderer.material.color;
            color.a = ouch;
            ouchRenderer.material.color = color;
        }
	}

    public void Ouch(float magnitude)
    {
        ouchObject.SetActive(true);
        ouch = ouch + magnitude;
        if(ouch > 1)
        {
            ouch = 1;
        }
    }
}
