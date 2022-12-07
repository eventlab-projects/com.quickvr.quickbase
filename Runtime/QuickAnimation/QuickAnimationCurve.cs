using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public enum QuickAnimEvaluateMethod
    {
        Interpolate,
        KeyFrameNearest,
        KeyFrameFloor,
        KeyFrameCeil,
    }

    public class QuickAnimationCurveBase : AnimationCurve
    {

        #region PUBLIC ATTRIBUTES

        public QuickAnimEvaluateMethod _evaluateMethod = QuickAnimEvaluateMethod.Interpolate;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected List<float> _sortedTime = new List<float>();

        #endregion

        #region GET AND SET

        public new int AddKey(float time, float value)
        {
            int index = base.AddKey(time, value);
            if (index != -1)
            {
                _sortedTime.Add(time);
            }

            return index;
        }

        public new float Evaluate(float time)
        {
            if (_evaluateMethod == QuickAnimEvaluateMethod.Interpolate) return base.Evaluate(time);

            int keyFrameID = GetKeyFrameID(time);
            return this[keyFrameID].value;
        }

        protected int GetKeyFrameID(float time)
        {
            int index = _sortedTime.BinarySearch(time);
            if (index < 0)
            {
                index = Mathf.Max(0, ~index - 1);
                //index = Mathf.Min(~index, _sortedTime.Count - 1);
            }

            return index;

            //float f = _totalTime == 0? 0 : length * Mathf.Clamp01(time / _totalTime);
            ////Debug.Log("time = " + time.ToString("f3"));
            ////Debug.Log("totalTime = " + _totalTime.ToString("f3"));
            ////Debug.Log("f = " + f.ToString("f3"));
            ////Debug.Log("length = " + length);

            //int result = 0;
            //if (_evaluateMethod == QuickAnimEvaluateMethod.KeyFrameFloor) result = Mathf.FloorToInt(f) - 1;
            //else if (_evaluateMethod == QuickAnimEvaluateMethod.KeyFrameCeil) result = Mathf.CeilToInt(f);
            //else result = Mathf.RoundToInt(f);

            //return Mathf.Clamp(result, 0, length - 1);
        }

        public virtual float GetLastTime()
        {
            return _sortedTime.Count > 0 ? _sortedTime[_sortedTime.Count - 1] : 0;
        }

        #endregion

    }

    public class QuickAnimationCurve
    {

        #region PROTECTED ATTRIBUTES

        protected QuickAnimationCurveBase _x = new QuickAnimationCurveBase();
        protected QuickAnimationCurveBase _y = new QuickAnimationCurveBase();
        protected QuickAnimationCurveBase _z = new QuickAnimationCurveBase();
        protected QuickAnimationCurveBase _w = new QuickAnimationCurveBase();

        #endregion

        #region CONSTANTS

        protected static float ZERO = 0.0001f;

        #endregion

        #region GET AND SET

        public QuickAnimationCurveBase this[int key]
        {
            get => GetValue(key);
        }

        public QuickAnimationCurveBase[] GetAnimationCurves()
        {
            return new QuickAnimationCurveBase[] { this[0], this[1], this[2], this[3] };
        }

        protected virtual QuickAnimationCurveBase GetValue(int key)
        {
            if (key == 0) return _x;
            if (key == 1) return _y;
            if (key == 2) return _z;
            if (key == 3) return _w;

            return null;
        }

        public virtual void AddKey(float time, float value, bool forceAdd = false)
        {
            AddKey(_x, time, value, forceAdd);
        }

        public virtual void AddKey(float time, bool value, bool forceAdd = false)
        {
            AddKey(_x, time, value ? 1 : 0, forceAdd);
        }

        public virtual void AddKey(float time, Vector3 value, bool forceAdd = false)
        {
            //AddKey(_x, time, value.x, forceAdd);
            //AddKey(_y, time, value.y, forceAdd);
            //AddKey(_z, time, value.z, forceAdd);

            Vector3 lastValue = EvaluateVector3(_x.GetLastTime());
            if (forceAdd || Vector3.SqrMagnitude(value - lastValue) > 0.0001f * 0.0001f)
            {
                AddKey(_x, time, value.x, true);
                AddKey(_y, time, value.y, true);
                AddKey(_z, time, value.z, true);
            }
        }

        public virtual void AddKey(float time, Quaternion value, bool forceAdd = false)
        {
            //AddKey(_x, time, value.x, forceAdd);
            //AddKey(_y, time, value.y, forceAdd);
            //AddKey(_z, time, value.z, forceAdd);
            //AddKey(_w, time, value.w, forceAdd);

            Quaternion lastValue = EvaluateQuaternion(_x.GetLastTime());
            if (forceAdd || Quaternion.Angle(value, lastValue) > 1)
            {
                AddKey(_x, time, value.x, true);
                AddKey(_y, time, value.y, true);
                AddKey(_z, time, value.z, true);
                AddKey(_w, time, value.w, true);
            }
            
        }

        protected virtual void AddKey(QuickAnimationCurveBase curve, float time, float value, bool forceAdd = false)
        {
            if (Mathf.Abs(value) < ZERO)
            {
                value = 0;
            }

            float lastValue = curve.length > 0 ? curve[curve.length - 1].value : Mathf.Infinity;

            if (forceAdd || !Mathf.Approximately(lastValue, value))
            {
                curve.AddKey(time, value);
            }
        }

        public virtual float Evaluate(float time)
        {
            return _x.Evaluate(time);
        }

        public virtual int EvaluateInt(float time)
        {
            return Mathf.RoundToInt(Evaluate(time));
        }

        public virtual bool EvaluateBool(float time)
        {
            return Evaluate(time) != 0;
        }

        public virtual Vector3 EvaluateVector3(float time)
        {
            return new Vector3(_x.Evaluate(time), _y.Evaluate(time), _z.Evaluate(time));
        }

        public virtual Quaternion EvaluateQuaternion(float time)
        {
            return new Quaternion(_x.Evaluate(time), _y.Evaluate(time), _z.Evaluate(time), _w.Evaluate(time));
        }

        public virtual void SetEvaluateMethod(QuickAnimEvaluateMethod evaluateMethod)
        {
            _x._evaluateMethod = evaluateMethod;
            _y._evaluateMethod = evaluateMethod;
            _z._evaluateMethod = evaluateMethod;
            _w._evaluateMethod = evaluateMethod;
        }

        public virtual void Compress(float epsilon)
        {
            _x.Compress(epsilon);
            _y.Compress(epsilon);
            _z.Compress(epsilon);
            _w.Compress(epsilon);
        }

        #endregion

    }

}

