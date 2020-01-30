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

        #endregion

        #region EVENTS

        public delegate void PreUpdateTrackingAction();
        public static event PreUpdateTrackingAction OnPreUpdateTracking;

        public delegate void PostUpdateTrackingAction();
        public static event PostUpdateTrackingAction OnPostUpdateTracking;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _fpsCounter = QuickSingletonManager.GetInstance<PerformanceFPS>();
            _fpsCounter._showFPS = false;
        }

        #endregion

        #region GET AND SET

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
                Calibrate();
            }
            
            if (OnPreUpdateTracking != null) OnPreUpdateTracking();

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

            if (OnPostUpdateTracking != null) OnPostUpdateTracking();
        }

        private static bool IsNull(QuickBaseTrackingManager tManager)
        {
            return tManager == null;
        }

        #endregion
    }

}

