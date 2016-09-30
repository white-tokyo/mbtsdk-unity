using System;
using UnityEngine;
using UnityEngine.UI;

namespace MilboxTouch
{
    public class ControlScript : MonoBehaviour
    {

        public Text DebugText;
        public bool DebugMode = false;

        // Use this for initialization
        void Start()
        {
            MBTHelper.OnTap += () => {
                ShowParameter(string.Format("::Tap::"));
            };

            MBTHelper.OnDoubleTap += () => {
                ShowParameter(string.Format("::DoubleTap::"));
            };

            MBTHelper.OnScroll += (angle) => {
                string s = String.Format("::Scroll {0} ::", angle);
                ShowParameter(s);
            };

            MBTHelper.OnSwipe += (swipe) => {
                var s = String.Format("::Swipespd:{0},dir:{1} ::", swipe.Speed, swipe.Direction);
                ShowParameter(s);
            };
            MBTHelper.OnScrollBegan += () => {
                ShowParameter("scrollBegan!!!");
            };

            MBTHelper.OnScrollEnd += () => {
                ShowParameter("ScrollEnd!!!!");
            };

            MBTHelper.OnSetupProgress += () => {
                ShowParameter("setupProgress:" + Time.time);
            };
            MBTHelper.OnSetupCompleted += () => {
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
