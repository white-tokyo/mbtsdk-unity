using UnityEngine;

namespace MilboxTouch
{
    public class Angle
    {
        private float _value;

        public Angle(float value)
        {
            while (value<0)
            {
                value += 360;
            }
            while (value>=360)
            {
                value -= 360;
            }
            _value = value;
        }

        public float Abs
        {
            get { return Mathf.Abs(_value); }
        }

        public float Value
        {
            get { return _value; }
        }

        public float Value180
        {
            get { return Value > 180 ? Value-360 : Value; }
        }

        public AngleBlock AngleBlock
        {
            get
            {
                if (Value < 90)return AngleBlock.UpperRight;
                if (Value < 180) return AngleBlock.UpperLeft;
                if(Value<270)return AngleBlock.LowerLeft;
                return AngleBlock.LowerRight;
            }
        }

        public static Angle operator +(Angle a,Angle b)
        {
            return new Angle(a.Value+b.Value);
        }

        public static Angle operator -(Angle a, Angle b)
        {
            return new Angle(a.Value-b.Value);
        }


        public class Factory
        {
            public float LeftBounds;
            public float RightBounds;

            public Factory(float leftBounds, float rightBounds)
            {
                this.LeftBounds = leftBounds;
                this.RightBounds = rightBounds;
            }

            public Angle ToAngle(float pos)
            {
                var currentPos = pos < LeftBounds ? LeftBounds : pos > RightBounds ? RightBounds : pos;
                var dir = currentPos - LeftBounds;
                var limit = RightBounds - LeftBounds;
                var rate = dir/limit;
                var correction = 170;
                var angle = rate*360 + correction;
                return new Angle(angle);
            }
        }
    }
}
