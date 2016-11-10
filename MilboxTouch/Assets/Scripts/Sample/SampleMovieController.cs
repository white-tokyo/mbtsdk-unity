using UnityEngine;
using MilboxTouch;
using System.Collections;
using UniRx;
using UnityEngine.UI;
using System;

public class SampleMovieController : MonoBehaviour {

    public MbtController MbtController;
    public MediaPlayerCtrl scrMedia;
    public Text Text;

    private bool _playing = false;
    private float _scrollSeek=0;

    // Use this for initialization
    void Start () {
        MbtController.OnTap += () =>
        {
            if (_playing)
            {
                scrMedia.Pause();
                Text.text = "Pause";
                Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
                {
                    Text.text = "";
                });
            }
            else
            {
                scrMedia.Play();
                Text.text = "Play";
                Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
                {
                    Text.text = "";
                });
            }
            _playing = !_playing;
        };
        MbtController.OnDoubleTap += () =>
        {
            if (_playing)
            {
                scrMedia.Stop();
                Text.text = "Stop";
                Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
                {
                    Text.text = "";
                });
            }
            else
            {
                scrMedia.Play();
                Text.text = "Play";
                Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
                {
                    Text.text = "";
                });
            }
            _playing = !_playing;
        };
        MbtController.OnScroll += (degree) =>
        {
            _scrollSeek += degree*20;
            Text.text = "seekTo:\n"+((int)_scrollSeek/1000).ToString();
        };
        MbtController.OnScrollEnd += () =>
        {
            var current = scrMedia.GetSeekPosition();
            var seekTo = current + (int) _scrollSeek;
            //Text.text = seekTo.ToString();
            Text.text = "";
            scrMedia.SeekTo(seekTo);
            _scrollSeek = 0;
        };
        MbtController.OnSwipe += (swipe) =>
        {
        };
        MbtController.OnSetupProgress += () =>
        {
            Text.text = "setup progress..\n" + Time.time;
        };
        MbtController.OnSetupCompleted += () =>
        {
            Text.text = "setup complete !";
            scrMedia.OnEnd += () =>
            {
                _playing = false;
            };
            Observable.Timer(TimeSpan.FromMilliseconds(3000)).Subscribe(it =>
            {
                Text.text = "";
                _playing = true;
                scrMedia.Play();
            });
        };
        MbtController.StartSetup();
    }
}
