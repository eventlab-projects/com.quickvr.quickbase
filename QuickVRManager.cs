using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickVRManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected List<QuickHeadTracking> _headTrackingSystems = new List<QuickHeadTracking>();
        protected List<QuickBodyTracking> _bodyTrackingSystems = new List<QuickBodyTracking>();
        protected List<QuickIKManager> _ikManagerSystems = new List<QuickIKManager>();

        protected PerformanceFPS _fpsCounter = null;

        protected QuickVRCameraController _cameraController = null;

        #endregion

        #region EVENTS

        public delegate void QuickVRManagerAction();

        public static event QuickVRManagerAction OnPreCalibrate;
        public static event QuickVRManagerAction OnPostCalibrate;

        public static event QuickVRManagerAction OnPreUpdateTracking;
        public static event QuickVRManagerAction OnPostUpdateTracking;

        public static event QuickVRManagerAction OnPreCameraUpdate;
        public static event QuickVRManagerAction OnPostCameraUpdate;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _fpsCounter = QuickSingletonManager.GetInstance<PerformanceFPS>();
            _fpsCounter._showFPS = false;
            _cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();
        }

        #endregion

        #region GET AND SET

        public virtual QuickVRCameraController GetCameraController()
        {
            return _cameraController;
        }

        public virtual void AddHeadTrackingSystem(QuickHeadTracking hTracking)
        {
            _headTrackingSystems.Add(hTracking);
        }

        public virtual void AddBodyTrackingSystem(QuickBodyTracking bTracking)
        {
            _bodyTrackingSystems.Add(bTracking);
        }

        public virtual void AddIKManagerSystem(QuickIKManager ikManager)
        {
            _ikManagerSystems.Add(ikManager);
        }

        protected virtual List<QuickBaseTrackingManager> GetAllTrackingSystems()
        {
            List<QuickBaseTrackingManager> result = new List<QuickBaseTrackingManager>();
            result.AddRange(_headTrackingSystems);
            result.AddRange(_bodyTrackingSystems);
            result.AddRange(_ikManagerSystems);

            return result;
        }

        public virtual void Calibrate()
        {
            foreach (QuickBaseTrackingManager tm in GetAllTrackingSystems())
            {
                if (tm.gameObject.activeInHierarchy)
                {
                    tm.Calibrate();
                }
            }
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            //Calibrate the TrackingManagers that needs to be calibrated. 
            if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CALIBRATE))
            {
                if (OnPreCalibrate != null) OnPreCalibrate();
                Calibrate();
                if (OnPostCalibrate != null) OnPostCalibrate();
            }

            //Update the TrackingManagers
            if (OnPreUpdateTracking != null) OnPreUpdateTracking();
            UpdateTracking();
            if (OnPostUpdateTracking != null) OnPostUpdateTracking();

            //Update the Camera position

            _cameraController.UpdateCameraPosition();
        }

        protected virtual void UpdateTracking()
        {
            //1) Update the HeadTracking systems
            foreach (QuickHeadTracking hTracking in _headTrackingSystems)
            {
                hTracking.UpdateTracking();
            }

            //2) Update the BodyTracking systems
            foreach (QuickBodyTracking bTracking in _bodyTrackingSystems)
            {
                bTracking.UpdateTracking();
            }

            //3) Update the IKManager systems
            foreach (QuickIKManager ikManager in _ikManagerSystems)
            {
                ikManager.UpdateTracking();
            }
        }

        #endregion

    }

}

