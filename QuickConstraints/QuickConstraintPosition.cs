using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickConstraintPosition : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        [SerializeField]
        protected Transform _transformConstraint = null;

        protected Vector3 _posOffset = Vector3.zero;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            SetTransformConstraint(_transformConstraint);
        }

        #endregion

        #region GET AND SET

        public virtual void SetTransformConstraint(Transform t)
        {
            _transformConstraint = t;
            if (_transformConstraint)
            {
                _posOffset = _transformConstraint.parent.InverseTransformVector(transform.position - _transformConstraint.position);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (_transformConstraint)
            {
                transform.position = _transformConstraint.position + _transformConstraint.parent.TransformVector(_posOffset);
            }
        }

        #endregion
    }

}


