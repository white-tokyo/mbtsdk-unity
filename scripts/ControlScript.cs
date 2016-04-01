using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ContolScript : MonoBehaviour {

	public Text DebugText;
	public bool DebugMode = false;

	// Use this for initialization
	void Start () {
		MBTHelper.OnTap += () => {
			Debug.Log ("::Tap::");
			ShowParameter("::Tap::");
			MBTOnTap ();
		};
		MBTHelper.OnSwipe += (x, y) => {
			Debug.Log ("::Swipe " + x.ToString() + ", " + y.ToString() + "::");
			ShowParameter("::Swipe " + x.ToString() + ", " + y.ToString() + "::");
			MBTOnSwipe (x, y);
		};
		MBTHelper.OnScroll += (x, y) => {
			Debug.Log ("::Scroll " + x.ToString() + ", " + y.ToString() + "::");
			ShowParameter("::Scroll " + x.ToString() + ", " + y.ToString() + "::");
			MBTOnScroll (x, y);
		};

		DebugText.text = "";
	}

	// Update is called once per frame
	void Update () {

	}

	private void MBTOnTap () {

	}

	private void MBTOnSwipe (float xx, float yy) {

	}

	private void MBTOnScroll (float xx, float yy) {

	}

	private void ShowParameter (string st) {
		if (DebugMode) {
			DebugText.text = st;
		}
	}
}
