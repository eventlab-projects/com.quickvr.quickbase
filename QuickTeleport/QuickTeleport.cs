using System.Collections;
using UnityEngine;

namespace QuickVR
{

    public class QuickTeleport : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public float _teleportTime = 0.3f;
        public Transform _pfTrajectoryTarget = null;

        public QuickUICursor.Role _vrCursorType = QuickUICursor.Role.Head;
        public string _teleportKey = "Teleport";

        #endregion

        #region PROTECTED PARAMETERS

        protected Transform _trajectoryTarget = null;
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
            StartCoroutine(CoUpdate());
            StartCoroutine(CoUpdateColor());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        #region GET AND SET

        protected virtual QuickUICursor GetCursor()
        {
            return QuickUICursor.GetVRCursor(_vrCursorType);
        }

        public virtual void SetTrajectoryVisible(bool v)
        {
            _trajectoryTarget.gameObject.SetActive(v);
        }

        protected virtual Vector3 GetPositionEnd()
        {
            QuickUICursor cursor = GetCursor();
            return cursor? cursor.GetRaycastResult().point : transform.position;
        }

        public virtual bool IsTeleportWalkableObjectSelected()
        {
            QuickUICursor cursor = GetCursor();
            QuickUIInteractiveItem selectedItem = cursor? cursor.GetCurrentInteractible() : null;
            return selectedItem && (selectedItem.GetComponent<QuickTeleportWalkableObject>() != null);
        }

        public virtual bool IsNoRayObjectSelected()
        {
            QuickUICursor cursor = GetCursor();
            if (cursor.GetRaycastResult().collider != null)
                return cursor.GetRaycastResult().collider.gameObject.CompareTag("NoRay");            
            return false;
        }

        public virtual void SetTrajectoryTargetColor(Color c)
        {
            _trajectoryTarget.GetComponent<Renderer>().material.color = c;
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoUpdate()
        {
            yield return null;

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

        protected virtual IEnumerator CoUpdateColor()
        {
            yield return null;

            while (true)
            {
                Color c = IsTeleportWalkableObjectSelected() ? Color.green : Color.red;                
                SetTrajectoryTargetColor(c);
                QuickUICursor cursor = GetCursor();
                if (cursor) cursor.SetColor(c);
                                
                cursor._drawRay = !IsNoRayObjectSelected();
                cursor._drawCursor = false;

                yield return null;
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
