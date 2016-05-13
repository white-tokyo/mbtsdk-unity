using UnityEngine;
using System.Collections;

public class MBTHelper : MonoBehaviour {

	public delegate void Trigger();
	public delegate void TouchPad(float x, float y);
	public delegate void Gesture(string name);

	public const float tapDetectTime = 1.0f;

	public const float tapDetectLength = 15.0f;
	private const float tapDetectLengthSqrt = tapDetectLength*tapDetectLength;

	public float doubleTapDetectTime = 0.15f;

	public int chargeDetectTapCount = 30;

	private Vector3 currentPos = Vector3.zero;
	private bool isScroll = false;
	private Vector3 tapPos = Vector3.zero;
	private float tapTime = 0.0f;
	private float lastTapTime = 0.0f;
	private int tapCountPerTime = 0;
	private int lastTime = 0;
	private bool isTap = false;
	private bool charged = false;

	public float minSwipeDistX;
	public float minSwipeDistY;

	private float swipeDistX;
	private float swipeDistY;

	private float SignValueX;
	private float SignValueY;

	private Vector2 endPos;

	void Start () {
		if (minSwipeDistX == 0) {
			minSwipeDistX = 50;
		}

		if (minSwipeDistY == 0) {
			minSwipeDistY = 50;
		}

		// charge - 一定フレーム以内に複数の反応があった場合
		// doubleTap - 一定秒数以内に複数回のタップがあった場合
	}

	public static event Trigger OnCharge, OnTap, OnDoubleTap;
	public static event TouchPad OnScroll;
	public static event Gesture OnSwipe;

	public void Awake(){
		OnTap = delegate { };
		OnCharge = delegate { };
		OnDoubleTap = delegate { };
		OnSwipe = delegate { };
		OnScroll = delegate { };
	}

	void Update () {

		if (Input.touchCount > 0) {

			if (lastTime == (int)Time.time) {
				tapCountPerTime += 1;

				if (tapCountPerTime > chargeDetectTapCount && !charged) {
					charged = true;
					OnCharge ();
				}
			} else {
				tapCountPerTime = 0;
				lastTime = (int)Time.time;
			}

			if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
				isTap = true;

				if ((Time.time - lastTapTime) < doubleTapDetectTime) {
					OnDoubleTap ();
				} else {
					lastTapTime = Time.time;
				}

				isScroll = true;
				tapPos = Input.GetTouch(0).position;
				currentPos = Input.GetTouch(0).position;
			}
			switch (Input.GetTouch(0).phase) {
			case TouchPhase.Began:
			case TouchPhase.Moved:
			case TouchPhase.Stationary:
				tapTime += Time.deltaTime;

				Vector3 vv = Input.GetTouch(0).position;
				Vector3 vp = currentPos - vv;
				if (isScroll && vp.sqrMagnitude > tapDetectLengthSqrt) {
					if (vp.x*vp.x > vp.y*vp.y){
						OnScroll(Input.GetTouch(0).position.x, 0);
						isTap = false;
					} else {
						OnScroll(0, Input.mousePosition.y);
						isTap = false;
					}
				}
				currentPos = vv;
				break;

			case TouchPhase.Ended:

				endPos = new Vector2 (tapPos.x, tapPos.y);

				swipeDistX = (new Vector3 (endPos.x, 0, 0) - new Vector3 (currentPos.x, 0, 0)).magnitude;
				if (swipeDistX > minSwipeDistX) {
					SignValueX = Mathf.Sign (endPos.x - currentPos.x);

					if (SignValueX > 0) {
						OnSwipe ("RIGHTSwipe");
					} else if (SignValueX < 0) {
						OnSwipe ("LEFTSwipe");
					}
				}

				swipeDistY = (new Vector3 (0, endPos.y, 0) - new Vector3 (0, currentPos.y, 0)).magnitude;
				if (swipeDistY > minSwipeDistY) {
					SignValueY = Mathf.Sign (endPos.y - currentPos.y);

					if (SignValueY > 0) {
						OnSwipe ("UPSwipe");
					} else if (SignValueY < 0) {
						OnSwipe ("DownSwipe");
					}
				}

				if (isScroll) {
					vp = currentPos - tapPos;
					// タップした時間がタップ判定時間より短かった場合
					if (tapTime < tapDetectTime) {
						if (vp.sqrMagnitude < tapDetectLengthSqrt && isTap) {
							OnTap ();
						}
					} else {
						if (vp.x * vp.x < vp.y * vp.y) {
							OnScroll (0, Input.GetTouch (0).position.y);
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
