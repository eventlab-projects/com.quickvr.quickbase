using System.Collections;
using UnityEngine;

namespace QuickVR
{

    public class QuickTeleport : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public float _teleportTime = 0.3f;
        public Transform _pfTrajectoryTarget = null;

        public VRCursorType _vrCursorType = VRCursorType.HEAD;
        public string _teleportKey = "Teleport";

        #endregion

        #region PROTECTED PARAMETERS

        protected Transform _trajectoryTarget = null;
        protected QuickUICursor _cursor = null;
        protected CameraFade _cameraFade = null;

        #endregion

        #region EVENTS

        public delegate void TeleportAction();
        public static event TeleportAction OnPreTeleport;
        public static event TeleportAction OnPostTeleport;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _cameraFade = QuickSingletonManager.GetInstance<CameraFade>();
            if (!_pfTrajectoryTarget) _pfTrajectoryTarget = Resources.Load<Transform>("Prefabs/pf_QuickTeleportTarget");
            _trajectoryTarget = Instantiate<Transform>(_pfTrajectoryTarget);

            SetTrajectoryVisible(false);
        }

        protected virtual void OnEnable()
        {
            _cursor = GetComponentInChildren<QuickHeadTracking>().GetVRCursor(_vrCursorType);
            StartCoroutine(CoUpdate());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        #region GET AND SET

        public virtual void SetTrajectoryVisible(bool v)
        {
            _trajectoryTarget.gameObject.SetActive(v);
        }

        protected virtual Vector3 GetPositionEnd()
        {
            return _cursor.GetRaycastResult().point;
        }

        public virtual bool IsTeleportWalkableObjectSelected()
        {
            QuickUIInteractiveItem selectedItem = _cursor.GetCurrentInteractible();
            return selectedItem && (selectedItem.GetComponent<QuickTeleportWalkableObject>() != null);
        }

        public virtual void SetTrajectoryTargetColor(Color c)
        {
            _trajectoryTarget.GetComponent<Renderer>().material.color = c;
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoUpdate()
        {
            while (true)
            {
                //1) Wait while the teleport key is not pressed
                while (!InputManager.GetButton(_teleportKey)) yield return null;

                //2) Draw the trajectory of the teleport, while the teleport key is pressed
                yield return StartCoroutine(CoDrawTrajectoryPoints());

                //3) Do the teleport process if the current selected object by the cursor is a teleportable one
                if (IsTeleportWalkableObjectSelected())
                {
                    yield return StartCoroutine(StartTeleport());
                }
            }
        }

        protected virtual IEnumerator CoDrawTrajectoryPoints()
        {
            while (InputManager.GetButton(_teleportKey))
            {
                SetTrajectoryVisible(IsTeleportWalkableObjectSelected());
                DrawTrajectoryPoints();
                yield return null;
            }

            SetTrajectoryVisible(false);
        }

        protected virtual void DrawTrajectoryPoints()
        {
            Vector3 endPos = GetPositionEnd();

            //set the final target icon's position and scale
            _trajectoryTarget.position = new Vector3(endPos.x, endPos.y + 0.01f, endPos.z);
            float fDistFactor = Vector3.Distance(transform.position, endPos) / 100.0f;
            _trajectoryTarget.localScale = new Vector3(fDistFactor, fDistFactor, fDistFactor);
        }

        protected virtual IEnumerator StartTeleport()
        {
            if (OnPreTeleport != null) OnPreTeleport();

            float halfDuration = _teleportTime * 0.5f;

            _cameraFade.StartFade(Color.black, halfDuration);
            while (_cameraFade.IsFading()) yield return null;

            transform.position = GetPositionEnd();

            _cameraFade.StartFade(Color.clear, halfDuration);
            while (_cameraFade.IsFading()) yield return null;

            if (OnPostTeleport != null) OnPostTeleport();
        }

        #endregion

    }

}
