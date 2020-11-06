using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Text.RegularExpressions;

namespace QuickVR {

	public class QuickVibratorManager : MonoBehaviour {

        #region CONSTANTS

        public const string DEFAULT_VIBRATOR_LEFT_HAND = "LeftHand";
        public const string DEFAULT_VIBRATOR_RIGHT_HAND = "RightHand";

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField] protected List<string> _virtualVibrators = new List<string>();
        protected bool _initialized = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnApplicationQuit()
        {
            foreach (string v in _virtualVibrators)
            {
                StopVibrating(v);
            }
        }

        protected virtual void Awake()
        {
            if (!_initialized) Reset();
        }

        protected virtual void Reset()
        {
            name = this.GetType().FullName;

            CreateDefaultVibrator(DEFAULT_VIBRATOR_LEFT_HAND);
            CreateDefaultVibrator(DEFAULT_VIBRATOR_RIGHT_HAND);

            //Create the default VibratorVR implementation
            QuickVibratorManagerVR vManager = GetComponentInChildren<QuickVibratorManagerVR>(true);
            if (!vManager)
            {
                vManager = transform.CreateChild("VibratorManagerVR").GetOrCreateComponent<QuickVibratorManagerVR>();
                vManager.Reset();
            }

            _initialized = true;
        }

        public virtual void CreateDefaultVibrator(string virtualVibratorName)
        {
            if (_virtualVibrators.Contains(virtualVibratorName)) return;

            AddNewVibrator();
            _virtualVibrators[_virtualVibrators.Count - 1] = virtualVibratorName;
        }

        #endregion

        #region GET AND SET

        public List<string> GetVirtualVibrators()
        {
            return _virtualVibrators;
        }

        public string GetVirtualVibrator(int vibratorID)
        {
            return vibratorID >= _virtualVibrators.Count ? "" : _virtualVibrators[vibratorID];
        }

        public virtual int GetNumVibrators()
        {
            return _virtualVibrators.Count;
        }

        public QuickBaseVibratorManager[] GetVibratorManagers()
        {
            return GetComponentsInChildren<QuickBaseVibratorManager>();
        }

        [ButtonMethod]
        public void AddNewVibrator()
        {
            _virtualVibrators.Add("New Vibrator");

            QuickBaseVibratorManager[] vManagers = GetVibratorManagers();
            foreach (QuickBaseVibratorManager manager in vManagers)
            {
                manager.AddVibratorMapping();
            }
        }

        [ButtonMethod]
        public void RemoveLastVibrator()
        {
            if (_virtualVibrators.Count == 0) return;
            _virtualVibrators.RemoveAt(_virtualVibrators.Count - 1);

            QuickBaseVibratorManager[] vManagers = GetVibratorManagers();
            foreach (QuickBaseVibratorManager manager in vManagers)
            {
                manager.RemoveLastVibratorMapping();
            }
        }

        public static void Vibrate(string virtualVibrator, float timeOut = 0.15f)
        {
            QuickBaseVibratorManager[] vManagers = QuickSingletonManager.GetInstance<QuickVibratorManager>().GetVibratorManagers();
            foreach (QuickBaseVibratorManager m in vManagers)
            {
                if (m._active) m.Vibrate(virtualVibrator, timeOut);
            }
        }
		
		public static void StopVibrating(string virtualVibrator)
        {
            QuickBaseVibratorManager[] vManagers = QuickSingletonManager.GetInstance<QuickVibratorManager>().GetVibratorManagers();
            foreach (QuickBaseVibratorManager m in vManagers)
            {
                m.StopVibrating(virtualVibrator);
            }
        }

        #endregion

    }

}
