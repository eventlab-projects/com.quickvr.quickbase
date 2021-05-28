using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public static class QuickEyeTracking 
    {
        
        private static QuickVRPlayArea _vrPlayArea
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
        private static QuickVRPlayArea m_VRPlayArea = null;

        private static QuickVRNodeEye _vrNodeEyeLeft
        {
            get
            {
                if (!m_VRNodeEyeLeft)
                {
                    m_VRNodeEyeLeft = _vrPlayArea.GetVRNode(HumanBodyBones.LeftEye) as QuickVRNodeEye;
                }

                return m_VRNodeEyeLeft;
            }
        }
        private static QuickVRNodeEye m_VRNodeEyeLeft = null;

        private static QuickVRNodeEye _vrNodeEyeRight
        {
            get
            {
                if (!m_VRNodeEyeRight)
                {
                    m_VRNodeEyeRight = _vrPlayArea.GetVRNode(HumanBodyBones.RightEye) as QuickVRNodeEye;
                }

                return m_VRNodeEyeRight;
            }
        }
        private static QuickVRNodeEye m_VRNodeEyeRight = null;

        public static Vector3 GetDirectionEyeLeft()
        {
            return _vrNodeEyeLeft.transform.forward;
        }

        public static Vector3 GetDirectionEyeRight()
        {
            return _vrNodeEyeRight.transform.forward;
        }

        public static Vector3 GetDirectionEyeCombined()
        {
            return Vector3.Lerp(GetDirectionEyeLeft(), GetDirectionEyeRight(), 0.5f);
        }

        public static float GetBlinkFactorEyeLeft()
        {
            return _vrNodeEyeLeft.GetBlinkFactor();
        }

        public static float GetBlinkFactorEyeRight()
        {
            return _vrNodeEyeRight.GetBlinkFactor();
        }

        public static bool IsEyeLeftTracked()
        {
            return _vrNodeEyeLeft.IsTracked();
        }

        public static bool IsEyeRightTracked()
        {
            return _vrNodeEyeRight.IsTracked();
        }

        public static bool IsEyeCombinedTracked()
        {
            return IsEyeLeftTracked() && IsEyeRightTracked();
        }
    }


}


