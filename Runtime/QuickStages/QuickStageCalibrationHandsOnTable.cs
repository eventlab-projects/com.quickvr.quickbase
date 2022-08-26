using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageCalibrationHandsOnTable : QuickStageCalibration
    {

        #region PUBLIC ATTRIBUTES

        public Transform _tableRoot = null;

        #endregion

        #region EVENTS

        public delegate void QuickCalibrationAction(float newHandHeight);
        public static event QuickCalibrationAction OnCalibrationHandHeightUpdated;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected bool _applyTableOffset = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            QuickVRManager.OnPostCalibrate += RequestApplyTableOffset;
            QuickVRManager.OnPostCopyPose += ApplyTableOffset;
        }

        protected virtual void OnDestroy()
        {
            QuickVRManager.OnPostCalibrate -= RequestApplyTableOffset;
            QuickVRManager.OnPostCopyPose -= ApplyTableOffset;
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdateStateForwardDirection()
        {
            //HMD Forward Direction calibration
            _guiCalibration._autoUpdateHint = false;
            _guiCalibration.SetTextInstructions("Keep your hands\n on the table and do not\n move while the\n screen is in black");
            _guiCalibration.SetTextHint("");
            _instructionsManager.Play(_headTrackingCalibrationInstructions);
            while (_instructionsManager.IsPlaying()) yield return null;

            //while (!IsContinueTriggered()) yield return null;
            QuickVRPlayArea playArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            QuickVRNode nLeftHand = playArea.GetVRNode(HumanBodyBones.LeftHand);
            QuickVRNode nRightHand = playArea.GetVRNode(HumanBodyBones.RightHand);

            const float maxThreshold = 0.025f;
            const int maxSecondsStill = 5;
            int secondsStill = maxSecondsStill;
            while (!nLeftHand.IsTracked() || !nRightHand.IsTracked()) yield return null;

            while (secondsStill > 0)
            {
                Vector3 posStartLeft = nLeftHand.transform.position;
                Vector3 posStartRight = nRightHand.transform.position;

                _guiCalibration.SetTextHint("Keep the position for " + secondsStill.ToString() + " seconds.");

                yield return new WaitForSeconds(1);

                float mLeft = (nLeftHand.transform.position - posStartLeft).magnitude;
                float mRight = (nRightHand.transform.position - posStartRight).magnitude;
                if ((mLeft > maxThreshold) || (mRight > maxThreshold))
                {
                    secondsStill = maxSecondsStill;
                }
                else
                {
                    secondsStill--;
                }

            }
            _guiCalibration._autoUpdateHint = true;

            _instructionsManager.Stop();
            yield return null;
        }

        protected virtual void RequestApplyTableOffset()
        {
            if (_tableRoot)
            {
                _applyTableOffset = true;
            }
        }

        protected virtual void ApplyTableOffset()
        {
            if (_applyTableOffset)
            {
                Vector3 currentPos = _tableRoot.transform.position;
                Animator animator = _vrManager.GetAnimatorTarget();
                float handHeight = Mathf.Min(animator.GetBoneTransform(HumanBodyBones.LeftHand).position.y, animator.GetBoneTransform(HumanBodyBones.RightHand).position.y);
                _tableRoot.transform.position = new Vector3(currentPos.x, handHeight - 0.035f, currentPos.z);

                _applyTableOffset = false;

                if (OnCalibrationHandHeightUpdated != null)
                {
                    OnCalibrationHandHeightUpdated(handHeight);
                }
            }
        }

        #endregion

    }

}


