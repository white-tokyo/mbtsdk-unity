using UnityEngine;
using System.Collections;

public class SampleCubeController : MonoBehaviour
{
    public Color DefaultColor = Color.white;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material.color = DefaultColor;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SetGazedAt(bool gazedAt)
    {
        GetComponent<Renderer>().material.color = gazedAt ? Color.green : DefaultColor;
    }
}
