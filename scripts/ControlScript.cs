using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ControlScript : MonoBehaviour {

	public Text DebugText;
	public bool DebugMode = false;

	// Use this for initialization
	void Start () {
		MBTHelper.OnTap += () => {
			Debug.Log ("::Tap::");
			ShowParameter("::Tap::");
			MBTOnTap ();
		};

		MBTHelper.OnDoubleTap += () => {
			Debug.Log ("::DoubleTap::");
			ShowParameter("::DoubleTap::");
			MBTOnDoubleTap ();
		};

		MBTHelper.OnScroll += (x, y) => {
			Debug.Log ("::Scroll " + x.ToString() + ", " + y.ToString() + "::");
			ShowParameter("::Scroll " + x.ToString() + ", " + y.ToString() + "::");
			MBTOnScroll (x, y);
		};

		MBTHelper.OnSwipe += (gesture) => {
			Debug.Log ("::Swipe " + gesture.ToString() + "::");
			ShowParameter("::Swipe " + gesture.ToString() + "::");
			MBTOnSwipe (gesture);
		};

		MBTHelper.OnCharge += () => {
			Debug.Log ("::Charge::");
			ShowParameter("::Charge::");
			MBTOnCharge ();
		};

		MBTHelper.OnClockwise += (flg) => {
			Debug.Log ("::Clock " + flg.ToString() + "::");
			ShowParameter("::Clock " + flg.ToString() + "::");
			MBTOnClock (flg);
		};

		// DebugText.text = "";
	}

	// Update is called once per frame
	void Update () {
	}

	private void MBTOnTap () {

	}

	private void MBTOnDoubleTap () {

	}

	private void MBTOnScroll (float xx, float yy) {

	}

	private void MBTOnSwipe (string gesture) {

	}

	private void MBTOnCharge () {

	}

	private void MBTOnClock (bool flg) {

	}


	private void ShowParameter (string st) {
		if (DebugMode) {
			DebugText.text = st;
		}
	}
}
