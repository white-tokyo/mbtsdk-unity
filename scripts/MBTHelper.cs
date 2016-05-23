using UnityEngine;
using System.Collections;

public class MBTHelper : MonoBehaviour {

	public delegate void Trigger();
	public delegate void TouchPad(float x, float y);
	public delegate void Gesture(string name);
	public delegate void Clockwise(bool flg);

	//タップの許容時間
	public const float tapDetectTime = 1.0f;

	//タップの許容移動距離
	public const float tapDetectLength = 15.0f;

	//ダブルタップの許容時間
	public float doubleTapDetectTime = 0.3f;

	//チャージに必要なフレーム数
	public int chargeDetectTapCount = 30;

	public float minSwipeDistX;
	public float minSwipeDistY;

	private Vector2 tapBeganPos = Vector2.zero;
	private float touchTime = 0.0f;
	private float lastTapTime = 0.0f;
	private int tapCountPerTime = 0;
	private int lastTime = 0;
	private bool charged = false;


	private	const float	CLOCK_PARAM_MAX			= 20.0f;
	private	const float	CLOCK_PARAM_THRESHOLD 	= 5.0f;
	private	const float	CLOCK_PARAM_ADD 		= 1.0f;
	private	const float	CLOCK_PARAM_GAIN		= 0.9f;
	private	float		clockParam				= 0f;


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
	public static event Clockwise OnClockwise;

	public void Awake(){
		OnTap = delegate { };
		OnCharge = delegate { };
		OnDoubleTap = delegate { };
		OnSwipe = delegate { };
		OnScroll = delegate { };
		OnClockwise = delegate { };
	}

	void Update () {

		MBTTouch.Update();

		if (MBTTouch.touchCount == 0) {
			return;
		}

		//check charge
		if(!charged){
			if (lastTime == (int)Time.time) {
				tapCountPerTime += 1;

				if (tapCountPerTime > chargeDetectTapCount) {
					charged = true;
					OnCharge ();
				}
			} else {
				tapCountPerTime = 0;
				lastTime = (int)Time.time;
			}
		}

		switch (MBTTouch.phase) {
		case TouchPhase.Began:
			tapBeganPos = MBTTouch.position;
			break;
		case TouchPhase.Moved:
		case TouchPhase.Stationary:
			touchTime += Time.deltaTime;

			Vector3 vp = MBTTouch.deltaPosition;
			if (vp.sqrMagnitude > tapDetectLength*tapDetectLength) {
				if (vp.x*vp.x > vp.y*vp.y){
					OnScroll(MBTTouch.position.x, 0);
				} else {
					OnScroll(0, MBTTouch.position.y);
				}
			}

			Vector2 td = nowTouch.deltaPosition;

			clockParam *= CLOCK_PARAM_GAIN;

			if (0<td.x)
			{
				if( -CLOCK_PARAM_MAX < clockParam ){ clockParam -= CLOCK_PARAM_ADD; }
			}
			else if (td.x<0)
			{
				if( clockParam < CLOCK_PARAM_MAX ){ clockParam += CLOCK_PARAM_ADD; }
			}
			
			if( CLOCK_PARAM_THRESHOLD < clockParam )
			{
				OnClockwise(true);
			}
			else if( clockParam < -CLOCK_PARAM_THRESHOLD )
			{
				OnClockwise(false);
			}

			break;

		case TouchPhase.Ended:
		case TouchPhase.Canceled:

			var endPos = MBTTouch.position;

			//check tap and double tap.
			if (touchTime < tapDetectTime) {
				if ((endPos - tapBeganPos).sqrMagnitude < tapDetectLength*tapDetectLength) {
					if ((Time.time - lastTapTime) < doubleTapDetectTime) {
						OnDoubleTap ();
						lastTapTime = 0f;
					} else {
						OnTap ();
						lastTapTime = Time.time;
					}
					touchTime = 0.0f;
					return;
				}
			}

			if (Mathf.Abs (endPos.x - tapBeganPos.x) > minSwipeDistX) {

				if (endPos.x > tapBeganPos.x) {
					OnSwipe ("RIGHTSwipe");
				} else {
					OnSwipe ("LEFTSwipe");
				}
			}else if (Mathf.Abs (endPos.y - tapBeganPos.y) > minSwipeDistY) {

				if (endPos.y > tapBeganPos.y) {
					OnSwipe ("UPSwipe");
				} else{
					OnSwipe ("DownSwipe");
				}
			}
			touchTime = 0.0f;
			break;
		}

	}
}
