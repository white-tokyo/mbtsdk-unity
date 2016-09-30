using System;
using UnityEngine;
using System.Collections;
using MilboxTouch;
using UniRx;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{

    public MbtController MbtController;
    public GameObject Cubes;
    public Text Text;

	// Use this for initialization
	void Start ()
	{
	    MbtController.OnTap += () =>
	    {
	        if (SampleCubeController.GazedCube != null)
	        {
	            SampleCubeController.GazedCube.Tap();
	        }
	    };
        MbtController.OnDoubleTap += () =>
        {
            if (SampleCubeController.GazedCube != null)
            {
                SampleCubeController.GazedCube.DoubleTap();
            }
        };
        MbtController.OnScroll += (degree) =>
        {
            Cubes.gameObject.transform.Rotate(Vector3.up,degree);
        };
        MbtController.OnSwipe += (swipe) =>
        {
            if (SampleCubeController.GazedCube != null)
            {
                SampleCubeController.GazedCube.Swipe(swipe);
            }
        };
	    MbtController.OnSetupProgress += () =>
	    {
            Debug.Log("aaaaaaaaa");
	        Text.text = "setup progress..\n" + Time.time;
	    };
        MbtController.OnSetupCompleted += () =>
        {
            Text.text = "setup complete !";
            Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
            {
                Text.text = "";
            });
        };
        MbtController.StartSetup();
    }
}
