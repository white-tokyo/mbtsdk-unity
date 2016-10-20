using System;
using UnityEngine;
using System.Collections;
using MilboxTouch;
using UniRx;

public class SampleCubeController : MonoBehaviour
{
    public Color DefaultColor = Color.white;
    public static SampleCubeController GazedCube;

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().material.color = DefaultColor;
    }
	
	// Update is called once per frame
	void Update ()
	{
	    var time = Time.time;
	    transform.Rotate(new Vector3(Mathf.Sin(time),Mathf.Sin(time+1),Mathf.Sin(time+2)),Time.deltaTime*30);
	}
    public void SetGazedAt(bool gazedAt)
    {
        GetComponent<Renderer>().material.color = gazedAt ? Color.green : DefaultColor;
        GazedCube = gazedAt ? this : null;
    }

    public void Tap()
    {
        GetComponent<Renderer>().material.color = Color.yellow;
        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe((it) =>
        {
            GetComponent<Renderer>().material.color = DefaultColor;
        });
    }

    public void DoubleTap()
    {
        transform.localScale = new Vector3(2, 2, 2);
        Observable.Timer(TimeSpan.FromMilliseconds(2000)).Subscribe((it) =>
        {
            transform.localScale = new Vector3(1, 1, 1);
        });
    }

    public void Swipe(Swipe swipe)
    {
        GetComponent<Animation>().Play();
    }
}
