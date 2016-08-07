using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum SwipeDirection {
	Right,
	Left,
	Up,
	Down
}

/// <summary>
/// MBT helper.
/// 
/// </summary>
public class MBTHelper : MonoBehaviour {
	
	public delegate void OnScrollEventHandler(float degreeDelta);
	public delegate void OnSwipeEventHandler(float speed,SwipeDirection direction);
	public delegate void OnTapEventHandler(float angle);
	public delegate void OnActionEventHandler();

	//action event
	public static event OnTapEventHandler OnTap;
	public static event OnTapEventHandler OnDoubleTap;
	public static event OnScrollEventHandler OnScroll;
	public static event OnActionEventHandler OnScrollBegan;
	public static event OnActionEventHandler OnScrollEnd;
	public static event OnSwipeEventHandler OnSwipe;

	//setup event
	public static event OnActionEventHandler OnSetupProgress;
	public static event OnActionEventHandler OnSetupCompleted;

	//detect tap duration of time  
	public float tapDetectDuration = 1.0f;

	//tolerance for detect tap action
	public float tapDetectTolerance = 15.0f;

	//detect double tap duration of time  
	public float doubleTapDetectTime = 0.3f;

	//setup stage count limit.
	public int setupStageLimit = 10;

	//tolerance for progress setup stage. 
	public float setupTorrelance = 5;

	private MBTState state = null;
	private float leftLimit = -1;
	private float rightLimit = -1;

	void Start () {
		state = new MBTSetupState (this);
	}


	public void Awake(){
		OnTap = delegate { };
		OnDoubleTap = delegate { };
		OnSwipe = delegate { };
		OnScroll = delegate { };
		OnScrollBegan = delegate { };
		OnScrollEnd = delegate { };
		OnSetupCompleted = delegate {};
		OnSetupProgress = delegate {};
			
	}

	void Update () {

		MBTTouch.Update();

		if (MBTTouch.touchCount == 0) {
			return;
		}

		switch (MBTTouch.phase) {
		case TouchPhase.Began:
			state.touchBegan (MBTTouch.position.x);
			break;
		case TouchPhase.Moved:
		case TouchPhase.Stationary:
			state.touchMoved (MBTTouch.position.x, MBTTouch.deltaPosition.x);
			break;

		case TouchPhase.Ended:
		case TouchPhase.Canceled:
			state.touchEnded (MBTTouch.position.x, MBTTouch.deltaPosition.x);
			break;
		}
	}

	//for raise events. 
	private void setupComplete(){
		OnSetupCompleted ();
	}
	private void setupProgress(){
		OnSetupProgress ();
	}
	private void scroll(float delta){
		OnScroll (delta);
	}
	private void scrollBegan(){
		OnScrollBegan ();
	}
	private void scrollEnded(){
		OnScrollEnd ();
	}
	private void doubleTap(float angle){
		OnDoubleTap (angle);
	}
	private void tap(float angle){
		OnTap (angle);
	}
	private void swipe(float speed,SwipeDirection direction){
		OnSwipe(speed,direction);
	}

	private abstract class MBTState{
		protected MBTHelper _helper{ get; set;}
		protected MBTState(MBTHelper helper){
			this._helper = helper;
		}
		public abstract void touchBegan(float position);
		public abstract void touchMoved (float position, float delta);
		public abstract void touchEnded (float position, float delta);
	}
		



	private class MBTSetupState: MBTState {
		private List<float> rightLimitHistory = new List<float>();
		private List<float> leftLimitHistory = new List<float>();

		private float leftLimit = -1;
		private float rightLimit = -1;

		public MBTSetupState(MBTHelper helper):base(helper){
		}

		private bool setupCompleted {
			get{
				if(rightLimitHistory.Count != _helper.setupStageLimit){
					return false;
				}

				var r_max = rightLimitHistory.Max();
				var r_min = rightLimitHistory.Min();
				Debug.Log (string.Format ("rmax:{0} rmin{1}", r_max, r_min));
				if (r_max - r_min > this._helper.setupTorrelance){
					return false;
				}
				var l_max = rightLimitHistory.Max();
				var l_min = rightLimitHistory.Min();
				Debug.Log (string.Format ("lmax:{0} lmin{1}", l_max, l_min));
				if (l_max - l_min > this._helper.setupTorrelance){
					return false;
				}
				return true;
			}
		}

		private void appendHistory(float right, float left) {
			rightLimitHistory.Add (right);
			leftLimitHistory.Add (left);

			if (rightLimitHistory.Count > _helper.setupStageLimit) {
				rightLimitHistory.RemoveAt (0);
			}
			if (leftLimitHistory.Count > _helper.setupStageLimit) {
				leftLimitHistory.RemoveAt (0);
			}
			this._helper.setupProgress ();
		}

		#region MBTState implementation
		private float touchBeganPos;
		public override void touchBegan (float position){
			touchBeganPos = position;
		}


		public override void touchMoved (float position, float delta){
		}


		public override void touchEnded (float position, float delta){
			if(Mathf.Abs(touchBeganPos-position) < 20){
				return;
			}
			checkLimit (touchBeganPos);
			checkLimit (position);
		}
		#endregion

		private void checkLimit(float position){
			this.rightLimit = this.rightLimit < 0 ? position : Mathf.Max (position, this.rightLimit);
			this.leftLimit = this.leftLimit < 0 ? position : Mathf.Min (position, this.leftLimit);
			appendHistory (this.rightLimit, this.leftLimit);

			if(setupCompleted){
				_helper.setupComplete ();
				_helper.leftLimit = this.leftLimit;
				_helper.rightLimit = this.rightLimit;
				_helper.state = new DetectState (_helper);
			}
		}

	}

	private class DetectState: MBTState{
		public DetectState(MBTHelper helper):base(helper){
		}

		private float tapStartAnglePosition = 0;
		private float tapStartTime = 0;

		//for detect doubleTap
		private float lastTapAnglePosition = 0;
		private float lastTapTime = 0;

		//for detect scrollBegan,scrollEnd.
		private bool scrolled = false;


		private bool checkTap(float endPos){
			bool b = Mathf.Abs (this.tapStartAnglePosition - endPos) < this._helper.tapDetectTolerance;
			bool t = Time.time - tapStartTime < this._helper.tapDetectDuration;
			return b && t;
		}
		private bool checkDoubleTap(float endPos){
			bool checkDist = Mathf.Abs (this.lastTapAnglePosition - endPos) < this._helper.tapDetectTolerance;
			bool checkTime = Time.time - lastTapTime < this._helper.doubleTapDetectTime;
			return checkDist && checkTime;
		}
		private Swipe checkSwipe(float pos){
			var angleCheck = Mathf.Abs (tapStartAnglePosition - pos) > this._helper.tapDetectTolerance;
			var timeCheck = Time.time - tapStartTime < this._helper.tapDetectDuration;
			if (angleCheck && timeCheck){
				return new Swipe (tapStartAnglePosition, pos, Time.time - tapStartTime);
			}
			return null;
		}

		//convert position to angle degree.
		private float positionToAngle(float position){
			if (position < this._helper.leftLimit){
				position = this._helper.leftLimit;
			}else if (this._helper.rightLimit < position){
				position = this._helper.rightLimit;
			}

			var dir = position - this._helper.leftLimit;
			var limitSpan = this._helper.rightLimit - this._helper.leftLimit;
			var rate = dir / limitSpan;
			float correction = 170;
			float angle = rate * 360 + correction;
			return angle >= 360 ? angle - 360 : angle;
		}
		private float deltaToAngleDelta(float delta){
			var limitSpan = this._helper.rightLimit - this._helper.leftLimit;
			var rate = delta / limitSpan;
			float angle = rate * 360;
			return angle >= 360 ? angle - 360 : angle;
		}

			
		#region MBTState implementation
		public override void touchBegan (float position)
		{
			var anglePosition = positionToAngle (position);
			this.tapStartTime = Time.time;
			this.tapStartAnglePosition = anglePosition;
		}
		public override void touchMoved (float position, float delta){
//			Debug.Log (string.Format ("move: delta:{0}", delta));
			var deltaScale = Mathf.Abs (delta);

			if (0.3 < deltaScale && deltaScale < 30){
				//detect swipe

				if(!scrolled){
					scrolled = true;
					this._helper.scrollBegan ();
				}

				_helper.scroll (delta);
			}
		}
		public override void touchEnded (float position, float delta){
			var anglePosition = positionToAngle (position);

			//check scroll has ended.
			if (scrolled){
				scrolled = false;
				this._helper.scrollEnded ();
			}

			//check tap action detected.
			if(checkTap(anglePosition)){
				//check tap is double tap.
				if(checkDoubleTap(anglePosition)){
					this._helper.doubleTap (anglePosition);
					this.lastTapAnglePosition = 0;
					this.lastTapTime = 0;
				}else{
					this._helper.tap (anglePosition);
					this.lastTapAnglePosition = anglePosition;
					this.lastTapTime = Time.time;
				}
			}else{
				//check swipe action is detected.
				var swipe = checkSwipe (anglePosition);
				if(swipe != null){
					this._helper.swipe (swipe.Speed, swipe.Direction);
				}
			} 


		}
		#endregion

	}


	private class Swipe{
		private float startAnglePosition;
		private float endAnglePosition;
		private float timeDuration;
		public Swipe(float sap,float eap,float time){
			startAnglePosition = sap;
			endAnglePosition = eap;
			timeDuration = time;
		}

		public float Speed{
			get{
				return Mathf.Abs (endAnglePosition - startAnglePosition) / timeDuration;
			}
		}
		public SwipeDirection Direction{
			get{
				var s = checkAngleBlock (startAnglePosition);
				var e = checkAngleBlock (endAnglePosition);
				switch (s) {
				case AngleBlock.UpperLeft:
					if (e == AngleBlock.UpperRight) {
						return SwipeDirection.Right;
					} else if (e == AngleBlock.LowerLeft) {
						return SwipeDirection.Down;
					}
					break;
				case AngleBlock.UpperRight:
					if (e == AngleBlock.UpperLeft) {
						return SwipeDirection.Left;
					}else if (e == AngleBlock.LowerRight) {
						return SwipeDirection.Down;
					}
					break;
				case AngleBlock.LowerLeft:
					if (e == AngleBlock.UpperLeft) {
						return SwipeDirection.Up;
					}else if (e == AngleBlock.LowerRight) {
						return SwipeDirection.Right;
					}
					break;
				case AngleBlock.LowerRight: 
					if (e == AngleBlock.UpperRight) {
						return SwipeDirection.Up;
					}else if (e == AngleBlock.LowerLeft) {
						return SwipeDirection.Left;
					}
					break;
				}
				return startAnglePosition > endAnglePosition ? SwipeDirection.Right : SwipeDirection.Left;
			}
		}

		private enum AngleBlock{
			UpperRight,
			UpperLeft,
			LowerRight,
			LowerLeft
		}
		private AngleBlock checkAngleBlock(float angle){
			if (0 <= angle && angle < 90) {
				return AngleBlock.UpperRight;
			}else if (90 <= angle && angle < 180) {
				return AngleBlock.UpperLeft;
			}else if (180 <= angle && angle < 270) {
				return AngleBlock.LowerLeft;
				}
			return AngleBlock.LowerRight;
		}



	}

}
