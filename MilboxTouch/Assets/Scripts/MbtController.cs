using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace MilboxTouch
{

    /// <summary>
    /// MbtController
    /// </summary>
    public class MbtController : MonoBehaviour
    {

        public delegate void OnScrollEventHandler(float degreeDelta);
        public delegate void OnSwipeEventHandler(Swipe swipe);
        public delegate void OnActionEventHandler();

        //action event
        public event OnActionEventHandler OnTap;
        public event OnActionEventHandler OnDoubleTap;
        public event OnScrollEventHandler OnScroll;
        public event OnActionEventHandler OnScrollBegan;
        public event OnActionEventHandler OnScrollEnd;
        public event OnSwipeEventHandler OnSwipe;

        //setup event
        public event OnActionEventHandler OnSetupProgress;
        public event OnActionEventHandler OnSetupCompleted;

        //detect tap duration of time  
        public float tapDetectDuration = 250;

        //tolerance for detect tap action
        public float tapDetectTolerance = 15.0f;

        //detect double tap duration of time  
        public float doubleTapDetectTime = 0.3f;

        //setup stage count limit.
        public int setupStageLimit = 10;

        //tolerance for progress setup stage. 
        public float setupTorrelance = 5;

        private Subject<float> _touchBeganSubject = new Subject<float>();
        private Subject<float> _touchMovedSubject = new Subject<float>();
        private Subject<float> _touchEndedSubject = new Subject<float>();

        private readonly Subject<string> _onSetupProgressSubject = new Subject<string>(); 
        private readonly Subject<string> _onSetupCompleteSubject = new Subject<string>();
        private readonly Subject<string> _onTapSubject = new Subject<string>();
        private readonly Subject<string> _onDoubleTapSubject = new Subject<string>();
        private readonly Subject<string> _onScrollBeganSubject = new Subject<string>(); 
        private readonly Subject<float> _onScrollSubject = new Subject<float>(); 
        private readonly Subject<string> _onScrollEndSubject = new Subject<string>();
        private readonly Subject<Swipe> _onSwipeSubject = new Subject<Swipe>();

        /// <summary>
        /// Start setup
        /// </summary>
        public void StartSetup()
        {
            var angleFactory = new Angle.Factory(410, 555);
            var upOrDown =
                Observable.Merge(_touchBeganSubject, _touchEndedSubject).Throttle(TimeSpan.FromMilliseconds(250));

            var moveOverBounds = _touchMovedSubject.Pre().Where((tuple) =>
            {
                return Mathf.Abs(tuple.Item2 - tuple.Item1) > 100;
            }).SelectMany((tuple) =>
            {
                return Observable.Return(tuple.Item1).Concat(Observable.Return(tuple.Item2));
            });
            var setupSignal = Observable.Merge(upOrDown, moveOverBounds);
            var average = setupSignal.List().Select((list) => list.Average());
            var left = Observable.Zip(setupSignal, average).Where(it => it[0] < it[1]).Select(it => it.First());
            var right = Observable.Zip(setupSignal, average).Where(it => it[0] >= it[1]).Select(it => it.First());

            var leftFin = left.List()
                .Select(it => it.TakeLast(10))
                .Do(it => _onSetupProgressSubject.OnNext(null))
                .Where(it => it.Count() == 10)
                .Where(
                    list =>
                    {
                        var newList = new List<float>(list);
                        newList.Remove(newList.Max());
                        newList.Remove(newList.Min());
                        var diff = newList.Max() - newList.Min();
                        return diff < 50;
                    });
            var rightFin = right.List()
                .Select(it => it.TakeLast(10))
                .Do(it => _onSetupProgressSubject.OnNext(null))
                .Where(it => it.Count() == 10)
                .Where(
                    list =>
                    {
                        var newList = new List<float>(list);
                        newList.Remove(newList.Max());
                        newList.Remove(newList.Min());
                        var diff = newList.Max() - newList.Min();
                        return diff < 50;
                    });
            Observable.CombineLatest(leftFin.Timestamp(), rightFin.Timestamp())
                .Where(it => Mathf.Abs(it[0].Timestamp.Millisecond - it[1].Timestamp.Millisecond) < 500)
                .First()
                .Subscribe(
                    it =>
                    {
                        angleFactory.LeftBounds = it[0].Value.Average();
                        angleFactory.RightBounds = it[1].Value.Average();
                        Debug.Log(string.Format("FINISH: left: {0} right: {1}", angleFactory.LeftBounds, angleFactory.RightBounds));
                        _onSetupCompleteSubject.OnNext(null);
                    });


            var detectAngles =
                Observable.Merge(_touchBeganSubject.Select(it => Tuple.Create("began", it)),
                    _touchMovedSubject.Select(it => Tuple.Create("moved", it)),
                    _touchEndedSubject.Select(it => Tuple.Create("ended", it)))
                    .SkipUntil(_onSetupCompleteSubject)
                    .Select(it => Tuple.Create(it.Item1, angleFactory.ToAngle(it.Item2))).Publish();
            var sequence =
                detectAngles.TakeUntil(
                    detectAngles.Throttle(TimeSpan.FromMilliseconds(50)).Where(it => it.Item1 == "ended"));
            detectAngles.Connect().AddTo(gameObject);
            //sequence.Repeat().Subscribe(it =>
            //{
            //    Debug.Log("sequence:" + it.Item1 + " ::" + it.Item2.Value);
            //});

            var seqBegan =
                sequence.Select(it => it.Item2)
                    .Pre()
                    .Where(it => it.Item1 == null)
                    .Select(it => it.Item2)
                    .Repeat()
                    .Publish();
            var seqMoved = sequence.Skip(1).Select(it => it.Item2).Repeat().Publish();
            var seqEnded = sequence.Last().Select(it => it.Item2).Repeat().Publish();
            seqBegan.Connect().AddTo(gameObject);
            seqMoved.Connect().AddTo(gameObject);
            seqEnded.Connect().AddTo(gameObject);


            //seqBegan.Subscribe(it =>
            //{
            //    Debug.Log("seqBegan!: " + it.Value);
            //});
            //seqMoved.Subscribe(it =>
            //{
            //    Debug.Log("seqMOVED!: " + it.Value);
            //});
            //seqEnded.Subscribe(it =>
            //{
            //    Debug.Log("seqENDED!: " + it.Value);
            //});

            var tap =
                Observable.Zip(seqBegan.Timestamp(), seqEnded.Timestamp()).Select(it => Tuple.Create(it[0], it[1]))
                    //.Do(it =>
                    //{
                    //    var a = (it.Item2.Timestamp - it.Item1.Timestamp).TotalMilliseconds;
                    //    Debug.Log("duration::" + a);
                    //    Debug.Log("tor::" + (it.Item2.Value - it.Item1.Value).Abs);
                    //})
                    .Where(it => (it.Item2.Timestamp - it.Item1.Timestamp).TotalMilliseconds < tapDetectDuration)
                    .Where(it => (it.Item2.Value - it.Item1.Value).Abs < tapDetectTolerance)
                    .Select(it => it.Item2).Publish();
            tap.Pre()
                .Select(
                    it =>
                    {
                        if (it.Item1 == default(Timestamped<Angle>)) return "tap";
                        return (it.Item2.Timestamp - it.Item1.Timestamp).TotalMilliseconds < 500
                            ? "doubleTap"
                            : "tap";
                    })
                .Subscribe(
                    it =>
                    {
                        if (it == "tap")
                        {
                            //Debug.Log("TAP!!!!!");
                            _onTapSubject.OnNext(null);
                        }
                        else
                        {
                            //Debug.Log("DOUBLE_TAP!!!!!");
                            _onDoubleTapSubject.OnNext(null);
                        }
                    });
            tap.Connect().AddTo(gameObject);

            //scroll
            var moveDelta = seqMoved.Pre().Select(it => Tuple.Create(it.Item1, it.Item2 - (it.Item1 ?? it.Item2)));
            var scrollStroke =
                moveDelta.Select(it => it.Item2.Value180)
                    .Where(deltaScale =>
                    {
                        var abs = Mathf.Abs(deltaScale);
                        return 0.3 < abs && abs < 30;
                    }).TakeUntil(seqEnded);
            scrollStroke.Take(1).Do(it =>
            {
                _onScrollBeganSubject.OnNext(null);
            }).Zip(scrollStroke.ToList(), (a, b) => Tuple.Create(a, b)).Repeat().Subscribe(tuple =>
            {
                if (tuple.Item2.Count != 0)
                {
                    _onScrollEndSubject.OnNext(null);
                }
            }).AddTo(gameObject);
            scrollStroke.Repeat().Subscribe(it =>
            {
                //Debug.Log("SCR:"+it);
                _onScrollSubject.OnNext(it);
            }).AddTo(gameObject);

            //swipe
            sequence.Timestamp().ToList().Repeat().Where(it => it.Count >= 2).Subscribe(it =>
            {
                var startAngle = it.First().Value.Item2;
                var endAngle = it.Last().Value.Item2;
                var angleDelta = (endAngle - startAngle).Abs;
                var timeDelta = (it.Last().Timestamp - it.First().Timestamp).TotalMilliseconds;
                //Debug.Log("angleDelta: "+ angleDelta);
                //Debug.Log("timeduration:"+timeDelta);

                var positionCondition = angleDelta > 30;
                var timeCondition = timeDelta < tapDetectDuration;
                if (positionCondition && timeCondition)
                {
                    var swipe = new Swipe(startAngle, endAngle, (float)timeDelta);
                    _onSwipeSubject.OnNext(swipe);
                }
            }).AddTo(gameObject);

            //onSetupCompleteSubject.OnNext(null);

        }

        public void Reset()
        {
            _touchBeganSubject.Dispose();
            _touchBeganSubject = new Subject<float>();
            _touchMovedSubject.Dispose();
            _touchMovedSubject = new Subject<float>();
            _touchEndedSubject.Dispose();
            _touchEndedSubject = new Subject<float>();
        }

        private void Start()
        {
            _onSetupCompleteSubject.Subscribe(it =>
            {
                OnSetupCompleted();
            }).AddTo(gameObject);
            _onSetupProgressSubject.Subscribe(it =>
            {
                OnSetupProgress();
            }).AddTo(gameObject);
            _onTapSubject.Subscribe(it =>
            {
                OnTap();
            }).AddTo(gameObject);
            _onDoubleTapSubject.Subscribe(it =>
            {
                OnDoubleTap();
            }).AddTo(gameObject);
            _onScrollBeganSubject.Subscribe(it =>
            {
                OnScrollBegan();
            }).AddTo(gameObject);
            _onScrollSubject.Subscribe(it =>
            {
                OnScroll(it);
            }).AddTo(gameObject);
            _onScrollEndSubject.Subscribe(it =>
            {
                OnScrollEnd();
            }).AddTo(gameObject);
            _onSwipeSubject.Subscribe(it =>
            {
                OnSwipe(it);
            }).AddTo(gameObject);
        }


        private void Awake()
        {
            OnTap = delegate { };
            OnDoubleTap = delegate { };
            OnSwipe = delegate { };
            OnScroll = delegate { };
            OnScrollBegan = delegate { };
            OnScrollEnd = delegate { };
            OnSetupCompleted = delegate { };
            OnSetupProgress = delegate { };
        }

        private void Update()
        {

            MBTTouch.Update();

            if (MBTTouch.touchCount == 0)
            {
                return;
            }

            switch (MBTTouch.phase)
            {
                case TouchPhase.Began:
                    _touchBeganSubject.OnNext(MBTTouch.position.x);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    _touchMovedSubject.OnNext(MBTTouch.position.x);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    _touchEndedSubject.OnNext(MBTTouch.position.x);
                    break;
            }
        }

        //private void swipe(float speed, SwipeDirection direction)
        //{
        //    OnSwipe(speed, direction);
        //}

        //private abstract class MBTState
        //{
        //    protected MBTHelper _helper { get; set; }

        //    protected MBTState(MBTHelper helper)
        //    {
        //        this._helper = helper;
        //    }

        //    public abstract void touchBegan(float position);
        //    public abstract void touchMoved(float position, float delta);
        //    public abstract void touchEnded(float position, float delta);
        //}




        //private class MBTSetupState : MBTState
        //{
        //    private List<float> rightLimitHistory = new List<float>();
        //    private List<float> leftLimitHistory = new List<float>();

        //    private float leftLimit = -1;
        //    private float rightLimit = -1;

        //    public MBTSetupState(MBTHelper helper) : base(helper)
        //    {
        //    }

        //    private bool setupCompleted
        //    {
        //        get
        //        {
        //            if (rightLimitHistory.Count != _helper.setupStageLimit)
        //            {
        //                return false;
        //            }

        //            var r_max = rightLimitHistory.Max();
        //            var r_min = rightLimitHistory.Min();
        //            Debug.Log(string.Format("rmax:{0} rmin{1}", r_max, r_min));
        //            if (r_max - r_min > this._helper.setupTorrelance)
        //            {
        //                return false;
        //            }
        //            var l_max = rightLimitHistory.Max();
        //            var l_min = rightLimitHistory.Min();
        //            Debug.Log(string.Format("lmax:{0} lmin{1}", l_max, l_min));
        //            if (l_max - l_min > this._helper.setupTorrelance)
        //            {
        //                return false;
        //            }
        //            return true;
        //        }
        //    }

        //    private void appendHistory(float right, float left)
        //    {
        //        rightLimitHistory.Add(right);
        //        leftLimitHistory.Add(left);

        //        if (rightLimitHistory.Count > _helper.setupStageLimit)
        //        {
        //            rightLimitHistory.RemoveAt(0);
        //        }
        //        if (leftLimitHistory.Count > _helper.setupStageLimit)
        //        {
        //            leftLimitHistory.RemoveAt(0);
        //        }
        //        this._helper.setupProgress();
        //    }

        //    #region MBTState implementation

        //    private float touchBeganPos;

        //    public override void touchBegan(float position)
        //    {
        //        touchBeganPos = position;
        //    }


        //    public override void touchMoved(float position, float delta)
        //    {
        //    }


        //    public override void touchEnded(float position, float delta)
        //    {
        //        if (Mathf.Abs(touchBeganPos - position) < 20)
        //        {
        //            return;
        //        }
        //        checkLimit(touchBeganPos);
        //        checkLimit(position);
        //    }

        //    #endregion

        //    private void checkLimit(float position)
        //    {
        //        this.rightLimit = this.rightLimit < 0 ? position : Mathf.Max(position, this.rightLimit);
        //        this.leftLimit = this.leftLimit < 0 ? position : Mathf.Min(position, this.leftLimit);
        //        appendHistory(this.rightLimit, this.leftLimit);

        //        if (setupCompleted)
        //        {
        //            _helper.setupComplete();
        //            _helper.leftLimit = this.leftLimit;
        //            _helper.rightLimit = this.rightLimit;
        //            _helper.state = new DetectState(_helper);
        //        }
        //    }

        //}

//        private class DetectState : MBTState
//        {
//            public DetectState(MBTHelper helper) : base(helper)
//            {
//            }

//            private float tapStartAnglePosition = 0;
//            private float tapStartTime = 0;

//            //for detect doubleTap
//            private float lastTapAnglePosition = 0;
//            private float lastTapTime = 0;

//            //for detect scrollBegan,scrollEnd.
//            private bool scrolled = false;


//            private bool checkTap(float endPos)
//            {
//                bool b = Mathf.Abs(this.tapStartAnglePosition - endPos) < this._helper.tapDetectTolerance;
//                bool t = Time.time - tapStartTime < this._helper.tapDetectDuration;
//                return b && t;
//            }

//            private bool checkDoubleTap(float endPos)
//            {
//                bool checkDist = Mathf.Abs(this.lastTapAnglePosition - endPos) < this._helper.tapDetectTolerance;
//                bool checkTime = Time.time - lastTapTime < this._helper.doubleTapDetectTime;
//                return checkDist && checkTime;
//            }

//            //private Swipe checkSwipe(float pos)
//            //{
//            //    var angleCheck = Mathf.Abs(tapStartAnglePosition - pos) > this._helper.tapDetectTolerance;
//            //    var timeCheck = Time.time - tapStartTime < this._helper.tapDetectDuration;
//            //    if (angleCheck && timeCheck)
//            //    {
//            //        return new Swipe(tapStartAnglePosition, pos, Time.time - tapStartTime);
//            //    }
//            //    return null;
//            //}

//            //convert position to angle degree.
//            //private float positionToAngle(float position)
//            //{
//            //    if (position < this._helper.leftLimit)
//            //    {
//            //        position = this._helper.leftLimit;
//            //    }
//            //    else if (this._helper.rightLimit < position)
//            //    {
//            //        position = this._helper.rightLimit;
//            //    }

//            //    var dir = position - this._helper.leftLimit;
//            //    var limitSpan = this._helper.rightLimit - this._helper.leftLimit;
//            //    var rate = dir/limitSpan;
//            //    float correction = 170;
//            //    float angle = rate*360 + correction;
//            //    return angle >= 360 ? angle - 360 : angle;
//            //}

//            //private float deltaToAngleDelta(float delta)
//            //{
//            //    var limitSpan = this._helper.rightLimit - this._helper.leftLimit;
//            //    var rate = delta/limitSpan;
//            //    float angle = rate*360;
//            //    return angle >= 360 ? angle - 360 : angle;
//            //}


//            #region MBTState implementation

////            public override void touchBegan(float position)
////            {
////                var anglePosition = positionToAngle(position);
////                this.tapStartTime = Time.time;
////                this.tapStartAnglePosition = anglePosition;
////            }

////            public override void touchMoved(float position, float delta)
////            {
//////			Debug.Log (string.Format ("move: delta:{0}", delta));
////                var deltaScale = Mathf.Abs(delta);

////                if (0.3 < deltaScale && deltaScale < 30)
////                {
////                    //detect swipe

////                    if (!scrolled)
////                    {
////                        scrolled = true;
////                        this._helper.scrollBegan();
////                    }

////                    _helper.scroll(delta);
////                }
////            }

//            //public override void touchEnded(float position, float delta)
//            //{
//            //    var anglePosition = positionToAngle(position);

//            //    //check scroll has ended.
//            //    if (scrolled)
//            //    {
//            //        scrolled = false;
//            //        this._helper.scrollEnded();
//            //    }

//            //    ////check tap action detected.
//            //    //if (checkTap(anglePosition))
//            //    //{
//            //    //    //check tap is double tap.
//            //    //    if (checkDoubleTap(anglePosition))
//            //    //    {
//            //    //        this._helper.doubleTap(anglePosition);
//            //    //        this.lastTapAnglePosition = 0;
//            //    //        this.lastTapTime = 0;
//            //    //    }
//            //    //    else
//            //    //    {
//            //    //        this._helper.tap(anglePosition);
//            //    //        this.lastTapAnglePosition = anglePosition;
//            //    //        this.lastTapTime = Time.time;
//            //    //    }
//            //    //}
//            //    //else
//            //    //{
//            //    //    //check swipe action is detected.
//            //    //    var swipe = checkSwipe(anglePosition);
//            //    //    if (swipe != null)
//            //    //    {
//            //    //        this._helper.swipe(swipe.Speed, swipe.Direction);
//            //    //    }
//            //    //}


//            //}

//            #endregion

//        }


        //private class Swipe
        //{
        //    private float startAnglePosition;
        //    private float endAnglePosition;
        //    private float timeDuration;

        //    public Swipe(float sap, float eap, float time)
        //    {
        //        startAnglePosition = sap;
        //        endAnglePosition = eap;
        //        timeDuration = time;
        //    }

        //    public float Speed
        //    {
        //        get { return Mathf.Abs(endAnglePosition - startAnglePosition)/timeDuration; }
        //    }

        //    //public SwipeDirection Direction
        //    //{
        //    //    get
        //    //    {
        //    //        var s = checkAngleBlock(startAnglePosition);
        //    //        var e = checkAngleBlock(endAnglePosition);
        //    //        switch (s)
        //    //        {
        //    //            case AngleBlock.UpperLeft:
        //    //                if (e == AngleBlock.UpperRight)
        //    //                {
        //    //                    return SwipeDirection.Right;
        //    //                }
        //    //                else if (e == AngleBlock.LowerLeft)
        //    //                {
        //    //                    return SwipeDirection.Down;
        //    //                }
        //    //                break;
        //    //            case AngleBlock.UpperRight:
        //    //                if (e == AngleBlock.UpperLeft)
        //    //                {
        //    //                    return SwipeDirection.Left;
        //    //                }
        //    //                else if (e == AngleBlock.LowerRight)
        //    //                {
        //    //                    return SwipeDirection.Down;
        //    //                }
        //    //                break;
        //    //            case AngleBlock.LowerLeft:
        //    //                if (e == AngleBlock.UpperLeft)
        //    //                {
        //    //                    return SwipeDirection.Up;
        //    //                }
        //    //                else if (e == AngleBlock.LowerRight)
        //    //                {
        //    //                    return SwipeDirection.Right;
        //    //                }
        //    //                break;
        //    //            case AngleBlock.LowerRight:
        //    //                if (e == AngleBlock.UpperRight)
        //    //                {
        //    //                    return SwipeDirection.Up;
        //    //                }
        //    //                else if (e == AngleBlock.LowerLeft)
        //    //                {
        //    //                    return SwipeDirection.Left;
        //    //                }
        //    //                break;
        //    //        }
        //    //        return startAnglePosition > endAnglePosition ? SwipeDirection.Right : SwipeDirection.Left;
        //    //    }
        //    //}

        //    private enum AngleBlock
        //    {
        //        UpperRight,
        //        UpperLeft,
        //        LowerRight,
        //        LowerLeft
        //    }

        //    private AngleBlock checkAngleBlock(float angle)
        //    {
        //        if (0 <= angle && angle < 90)
        //        {
        //            return AngleBlock.UpperRight;
        //        }
        //        else if (90 <= angle && angle < 180)
        //        {
        //            return AngleBlock.UpperLeft;
        //        }
        //        else if (180 <= angle && angle < 270)
        //        {
        //            return AngleBlock.LowerLeft;
        //        }
        //        return AngleBlock.LowerRight;
        //    }



        //}

    }
}