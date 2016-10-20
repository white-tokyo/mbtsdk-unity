using System;
using UnityEngine;
using UnityEngine.UI;

namespace MilboxTouch
{
    public class ControlScript : MonoBehaviour
    {

        public Text DebugText;
        public bool DebugMode = false;
        public MbtController MbtController;

        // Use this for initialization
        void Start()
        {
            MbtController.OnTap += () => {
                ShowParameter(string.Format("::Tap::"));
            };

            MbtController.OnDoubleTap += () => {
                ShowParameter(string.Format("::DoubleTap::"));
            };

            MbtController.OnScroll += (angle) => {
                string s = String.Format("::Scroll {0} ::", angle);
                ShowParameter(s);
            };

            MbtController.OnSwipe += (swipe) => {
                var s = String.Format("::Swipespd:{0},dir:{1} ::", swipe.Speed, swipe.Direction);
                ShowParameter(s);
            };
            MbtController.OnScrollBegan += () => {
                ShowParameter("scrollBegan!!!");
            };

            MbtController.OnScrollEnd += () => {
                ShowParameter("ScrollEnd!!!!");
            };

            MbtController.OnSetupProgress += () => {
                ShowParameter("setupProgress:" + Time.time);
            };
            MbtController.OnSetupCompleted += () => {
                ShowParameter("setupComplete!!!");
            };

            // DebugText.text = "";
        }

        private void ShowParameter(string st)
        {
            if (DebugMode)
            {
                Debug.Log(st);
                DebugText.text = st;
            }
        }
    }
}
