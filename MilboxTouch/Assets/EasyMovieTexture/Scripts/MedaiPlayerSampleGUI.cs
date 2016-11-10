using UnityEngine;
using System.Collections;

public class MedaiPlayerSampleGUI : MonoBehaviour {


	public MediaPlayerCtrl scrMedia;
	
	public bool m_bFinish = false;
	// Use this for initialization
	void Start () {
		scrMedia.OnEnd += OnEnd;

	}

	
	// Update is called once per frame
	void Update () {


	
	}
	#if !UNITY_WEBGL
	void OnGUI() {
		
	
		if( GUI.Button(new Rect(50,50,50,50),"Load"))
		{
			scrMedia.Load("EasyMovieTexture.mp4");
			m_bFinish = false;
		}
		
		if( GUI.Button(new Rect(50,100,50,50),"Play"))
		{
			scrMedia.Play();
			m_bFinish = false;
		}
	 	
		if( GUI.Button(new Rect(50,150,50,50),"stop"))
		{
			scrMedia.Stop();
		}
		
		if( GUI.Button(new Rect(50,200,50,50),"pause"))
		{
			scrMedia.Pause();
		}
		
		if( GUI.Button(new Rect(50,250,50,50),"Unload"))
		{
			scrMedia.UnLoad();
		}
		
		if( GUI.Button(new Rect(50,0,50,50), " " + m_bFinish))
		{
		
		}
		
		if( GUI.Button(new Rect(200,0,50,50),"SeekTo"))
		{
			scrMedia.SeekTo(10000);
		}


		if( scrMedia.GetCurrentState() == MediaPlayerCtrl.MEDIAPLAYER_STATE.PLAYING)
		{
			if( GUI.Button(new Rect(200,50,50,50),scrMedia.GetSeekPosition().ToString()))
			{
				scrMedia.SetSpeed(2.0f);
			}
			
			if( GUI.Button(new Rect(200,100,50,50),scrMedia.GetDuration().ToString()))
			{
				scrMedia.SetSpeed(1.0f);
			}

			if( GUI.Button(new Rect(200,150,50,50),scrMedia.GetVideoWidth().ToString()))
			{
				
			}

			if( GUI.Button(new Rect(200,200,50,50),scrMedia.GetVideoHeight().ToString()))
			{
				
			}
		}

		if( GUI.Button(new Rect(200,250,50,50),scrMedia.GetCurrentSeekPercent().ToString()))
		{
			
		}
	

	}
	#endif


	
	void OnEnd()
	{
		m_bFinish = true;
	}
}
