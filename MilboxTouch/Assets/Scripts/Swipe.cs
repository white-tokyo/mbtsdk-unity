using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MilboxTouch
{
    public class Swipe
    {
        private Angle _startAnglePosition;
        private Angle _endAnglePosition;
        private float _timeduration;

        public Swipe(Angle startAnglePosition, Angle endAnglePosition, float timeDurationMilsec)
        {
            this._startAnglePosition = startAnglePosition;
            this._endAnglePosition = endAnglePosition;
            this._timeduration = timeDurationMilsec;
        }

        /// <summary>
        /// deg/sec
        /// </summary>
        public float Speed
        {
            get { return (_endAnglePosition - _startAnglePosition).Abs/_timeduration*1000; }
        }

        public SwipeDirection Direction
        {
            get
            {
                var s = _startAnglePosition.AngleBlock;
                var e = _endAnglePosition.AngleBlock;
                switch (s)
                {
                    case AngleBlock.UpperLeft:
                        if (e == AngleBlock.UpperRight) return SwipeDirection.Right;
                        if(e==AngleBlock.LowerLeft) return SwipeDirection.Down;
                        break;
                    case AngleBlock.UpperRight:
                        if (e == AngleBlock.UpperLeft) return SwipeDirection.Left;
                        if(e==AngleBlock.LowerRight)return SwipeDirection.Down;
                        break;
                    case AngleBlock.LowerRight:
                        if(e==AngleBlock.UpperRight)return SwipeDirection.Up;
                        if(e==AngleBlock.LowerLeft)return SwipeDirection.Left;
                        break;
                    case AngleBlock.LowerLeft:
                        if(e==AngleBlock.UpperLeft)return SwipeDirection.Up;
                        if(e==AngleBlock.LowerRight)return SwipeDirection.Right;
                        break;
                }
                if (_startAnglePosition.Value > _endAnglePosition.Value)
                {
                    return SwipeDirection.Right;
                }
                else
                {
                    return SwipeDirection.Left;
                }
            }
        }
    }
}
