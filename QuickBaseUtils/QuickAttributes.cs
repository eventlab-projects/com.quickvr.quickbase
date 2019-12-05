using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    #region BUTTON METHOD ATTRIBUTE

    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonMethodAttribute : Attribute
    {

        public ButtonMethodAttribute()
        {

        }
    }

    #endregion

    #region BITMASK ATTRIBUTE

    public class BitMaskAttribute : PropertyAttribute
    {
        public Type _type;
        public int _startID;
        public int _endID;

        public BitMaskAttribute(Type t)
        {
            _type = t;
            var itemValues = Enum.GetValues(t) as int[];
            _startID = 0;
            _endID = itemValues.Length - 1;
        }

        public BitMaskAttribute(Type t, object first, object last)
        {
            _type = t;
            var itemValues = Enum.GetValues(t) as int[];
            for (int i = 0; i < itemValues.Length; i++)
            {
                if (itemValues[i] == (int)first) _startID = i;
                else if (itemValues[i] == (int)last) _endID = i;
            }
        }
    }

    #endregion

    #region READ ONLY ATTRIBUTE

    public class ReadOnlyAttribute : PropertyAttribute
    {

    }

    #endregion

    #region FLOAT RANGE ATTRIBUTE

    [System.Serializable]
    public class FloatRangeAttribute : PropertyAttribute
    {
        [SerializeField]
        public float _maxValue;

        [SerializeField]
        public float _value;

    }

    #endregion

}
