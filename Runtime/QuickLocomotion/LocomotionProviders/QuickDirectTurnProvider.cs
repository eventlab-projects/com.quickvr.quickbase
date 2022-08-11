using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    public class QuickDirectTurnProvider : ContinuousTurnProviderBase
    {

        #region PUBLIC ATTRIBUTES

        public float _minActivaAngle = -20;
        public float _maxActiveAngle = 20;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRPlayArea _vrPlayArea
        {
            get
            {
                if (!m_VRPlayArea)
                {
                    m_VRPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
                }

                return m_VRPlayArea;
            }
        }
        protected QuickVRPlayArea m_VRPlayArea = null;

        protected QuickVRNode _nodeHead
        {
            get
            {
                if (!m_NodeHead)
                {
                    m_NodeHead = _vrPlayArea.GetVRNode(HumanBodyBones.Head);
                }

                return m_NodeHead;
            }
        }
        protected QuickVRNode m_NodeHead = null;

        #endregion

        #region GET AND SET

        /// <summary>
        /// Reads the current value of the turn input.
        /// </summary>
        /// <returns>Returns the input vector, such as from a thumbstick.</returns>
        protected override Vector2 ReadInput()
        {
            Vector2 result = Vector2.zero;

            if (_nodeHead.IsTracked())
            {
                Vector3 currentForward = GetCurrentForward();
                result = new Vector2(currentForward.x, currentForward.z);
            }

            return result;
        }

        protected virtual Vector3 GetCurrentForward()
        {
            return _nodeHead.GetTrackedObject().transform.forward;
        }

        /// <summary>
        /// Determines the turn amount in degrees for the given <paramref name="input"/> vector.
        /// </summary>
        /// <param name="input">Input vector, such as from a thumbstick.</param>
        /// <returns>Returns the turn amount in degrees for the given <paramref name="input"/> vector.</returns>
        protected override float GetTurnAmount(Vector2 input)
        {
            Vector3 up = _vrPlayArea._origin.transform.up;
            Vector3 lastFwd = Vector3.ProjectOnPlane(_vrPlayArea._origin.transform.forward, up);
            float result = Vector2.SignedAngle(input, new Vector2(lastFwd.x, lastFwd.z));
            _vrPlayArea._origin.Rotate(up, result, Space.World);

            return result;
        }

        #endregion

    }

}

