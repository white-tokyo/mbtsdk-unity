using UnityEngine;
using System.Collections;

public class MBTHelper : MonoBehaviour {


	public delegate void Trigger();
	public delegate void TouchPad(float x, float y);
	public delegate void Gesture(string name);

	public const float tapDetectTime = 1.0f;

	public const float tapDetectLength = 15.0f;
	private const float tapDetectLengthSqrt = tapDetectLength*tapDetectLength;

	private Vector3 currentPos = Vector3.zero;
	private bool isScroll = false;
	private Vector3 tapPos = Vector3.zero;
	private float tapTime = 0.0f;
	private bool isTap = false;

	public float minSwipeDistX;
	public float minSwipeDistY;

	private float swipeDistX;
	private float swipeDistY;

	float SignValueX;
	float SignValueY;

	private Vector2 startPos;
	private Vector2 endPos;

	void Start ()
	{
		if (minSwipeDistX == 0) {
			minSwipeDistX = 50;
		}
		if (minSwipeDistY == 0) {
			minSwipeDistY = 50;
		}
	}

	public static event Trigger OnTap;
	public static event TouchPad OnSwipe, OnScroll;
	public static event Gesture OnSwipeGesture;

	public void Awake(){
		OnTap = delegate { };
		OnSwipeGesture = delegate { };
		OnSwipe = delegate { };
		OnScroll = delegate { };
	}

	void Update () {
		if (Input.touchCount > 0) {
			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
				isTap = true;
				isScroll = true;
				tapPos = Input.GetTouch(0).position;
				currentPos = Input.GetTouch(0).position;
			}
			switch (Input.GetTouch(0).phase) {
			case TouchPhase.Began:
				//タッチを取得
				Touch touch = Input.touches [0];
				startPos = touch.position;
				break;
			case TouchPhase.Moved:
			case TouchPhase.Stationary:
				tapTime += Time.deltaTime;
				Vector3 vv = Input.GetTouch(0).position;
				Vector3 vp = currentPos - vv;
				Debug.Log (vp.sqrMagnitude);
				if (isScroll && vp.sqrMagnitude > tapDetectLengthSqrt) {
					if (vp.x*vp.x > vp.y*vp.y){
						OnScroll(Input.GetTouch(0).position.x, 0);
						isTap = false;
					} else {
						OnSwipe(0, Input.mousePosition.y);
						isTap = false;
					}
				}
				currentPos = vv;
				break;

			case TouchPhase.Ended:

				endPos = new Vector2 (touch.position.x, touch.position.y);

				swipeDistX = (new Vector3 (endPos.x, 0, 0) - new Vector3 (startPos.x, 0, 0)).magnitude;
				print ("X" + swipeDistX.ToString ());
				if (swipeDistX > minSwipeDistX) {
					SignValueX = Mathf.Sign (endPos.x - startPos.x);

					if (SignValueX > 0) {
						OnSwipeGesture("RIGHTSwipe");
					} else if (SignValueX < 0) {
						OnSwipeGesture("LEFTSwipe");
					}
				}

				swipeDistY = (new Vector3 (0, endPos.y, 0) - new Vector3 (0, startPos.y, 0)).magnitude;
				if (swipeDistY > minSwipeDistY) {
					SignValueY = Mathf.Sign (endPos.y - startPos.y);

					if (SignValueY > 0) {
						OnSwipeGesture("UPSwipe");
					} else if (SignValueY < 0) {
						OnSwipeGesture("DownSwipe");
					}
				}

				if (isScroll) {
					vp = currentPos - tapPos;
					if (tapTime < tapDetectTime){
						if (vp.sqrMagnitude < tapDetectLengthSqrt && isTap) {
							OnTap();
						}
					} else {
						if (vp.x*vp.x < vp.y*vp.y) {
							OnSwipe(0, Input.GetTouch(0).position.y);
						}
					}
				}

				isTap = false;
				isScroll = false;
				tapTime = 0.0f;
				break;
			}
		}
	}
}
