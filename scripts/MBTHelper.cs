using UnityEngine;
using System.Collections;

public class MBTHelper : MonoBehaviour {

	public delegate void Trigger();
	public delegate void TouchPad(float x, float y);
	public delegate void Gesture(string name);
	public delegate void Clockwise(bool flg, float avr);

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

	//回転は数フレーム分のアベレージを取る
	private	const int	CLOCK_AVR_NUM			= 5;
	private	const float	CLOCK_AVR_THRESHOLD 	= 1.0f;		//TODO 端末によって解像度が違うので何かしらの方法で最大近辺最小近辺を取得して適宜な値に動的に調整が好ましい
	public	float		clockAvr;

//	private	const float	CLOCK_PARAM_MAX			= 20.0f;
//	private	const float	CLOCK_PARAM_THRESHOLD 	= 5.0f;
//	private	const float	CLOCK_PARAM_ADD 		= 1.0f;
//	private	const float	CLOCK_PARAM_GAIN		= 0.9f;
//	public	float		clockParam				= 0f;


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
		CarcAvarage();

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

			if( CLOCK_AVR_THRESHOLD < clockAvr )
			{
				OnClockwise(false, clockAvr);		//TODO ここで機種依存の値、clockAvrを渡しているが機種依存しない形に修正したい
			}
			else if( clockAvr < -CLOCK_AVR_THRESHOLD )
			{
				OnClockwise(true, clockAvr);		//TODO 上記に同じ
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

	private void CarcAvarage()
	{
		float dx = 0f;
		if( 0 < MBTTouch.touchCount )
		{
			dx = MBTTouch.deltaPosition.x;
		}
		
		clockAvr = (clockAvr*(CLOCK_AVR_NUM-1) + dx) / CLOCK_AVR_NUM;
		
	}



}
