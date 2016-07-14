using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ControlScript : MonoBehaviour {

	public Text DebugText;
	public bool DebugMode = false;

	// Use this for initialization
	void Start () {
		MBTHelper.OnTap += (angle) => {
			ShowParameter(string.Format("::Tap angle:{0}::",angle));
		};

		MBTHelper.OnDoubleTap += (angle) => {
			ShowParameter(string.Format("::DoubleTap {0}::",angle));
		};

		MBTHelper.OnScroll += (angle) => {
			string s = String.Format("::Scroll {0} ::",angle);
			ShowParameter(s);
		};

		MBTHelper.OnSwipe += (speed,direction) => {
			var s = String.Format("::Swipespd:{0},dir:{1} ::",speed,direction);
			ShowParameter(s);
		};
		MBTHelper.OnScrollBegan += () => {
			ShowParameter("scrollBegan!!!");
		};

		MBTHelper.OnScrollEnd += () => {
			ShowParameter("ScrollEnd!!!!");
		};

		MBTHelper.OnSetupProgress += () => {
			ShowParameter("setupProgress:"+Time.time);
		};
		MBTHelper.OnSetupCompleted += () => {
			ShowParameter("setupComplete!!!");
		};

		// DebugText.text = "";
	}
		
	private void ShowParameter (string st) {
		Debug.Log (st);
		if (DebugMode) {
			DebugText.text = st;
		}
	}
}
