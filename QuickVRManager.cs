using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickVRManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        //A list of the Tracking Systems present in the scene, sorted by priority. Lower number indicates higher priority
        protected Dictionary<int, HashSet<QuickBaseTrackingManager>> _trackingManagers = new Dictionary<int, HashSet<QuickBaseTrackingManager>>();

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

        protected virtual void OnDestroy()
        {
            _trackingManagers.Clear();
        }

        #endregion

        #region GET AND SET

        public virtual void AddTrackingManager(int priority, QuickBaseTrackingManager tManager)
        {
            if (!_trackingManagers.ContainsKey(priority)) _trackingManagers[priority] = new HashSet<QuickBaseTrackingManager>();
            _trackingManagers[priority].Add(tManager);
        }

        public virtual void RemoveTrackingManager(int priority, QuickBaseTrackingManager tManager)
        {
            if (!_trackingManagers.ContainsKey(priority)) return;
            _trackingManagers[priority].Remove(tManager);
        }

        protected virtual List<int> GetSortedKeys()
        {
            List<int> sortedKeys = new List<int>(_trackingManagers.Keys);
            sortedKeys.Sort();
            return sortedKeys;
        }

        public virtual void Calibrate(bool forceCalibration = false)
        {
            List<int> sortedKeys = GetSortedKeys();

            foreach (int k in sortedKeys)
            {
                HashSet<QuickBaseTrackingManager> tManagers = _trackingManagers[k];
                foreach (QuickBaseTrackingManager tm in tManagers)
                {
                    if (tm.gameObject.activeInHierarchy && (!tm.IsCalibrated() || forceCalibration))
                    {
                        tm.Calibrate();
                    }
                }
            }
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            List<int> sortedKeys = GetSortedKeys();

            if (OnPreUpdateTracking != null) OnPreUpdateTracking();

            //Calibrate the TrackingManagers that needs to be calibrated. 
            Calibrate();

            //Update the TrackingManagers
            foreach (int k in sortedKeys)
            {
                HashSet<QuickBaseTrackingManager> tManagers = _trackingManagers[k];
                foreach (QuickBaseTrackingManager tm in tManagers)
                {
                    if (tm.gameObject.activeInHierarchy && tm.enabled)
                    {
                        tm.UpdateTracking();
                    }
                }
            }

            if (OnPostUpdateTracking != null) OnPostUpdateTracking();
        }

        #endregion
    }

}

