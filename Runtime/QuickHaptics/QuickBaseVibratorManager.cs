using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public abstract class QuickBaseVibratorManager : MonoBehaviour {

        #region PUBLIC PARAMETERS

        public bool _active = true;

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected List<string> _vibratorMapping = new List<string>();

        #endregion

        #region CREATION AND DESTRUCTION

        public virtual void Reset()
        {
            name = this.GetType().FullName;

            int numVirtualVibrators = GetComponentInParent<QuickVibratorManager>().GetVirtualVibrators().Count;
            while (_vibratorMapping.Count != numVirtualVibrators) AddVibratorMapping();
        }

        #endregion

        #region GET AND SET

        public virtual int GetNumVibratorsMapped()
        {
            return _vibratorMapping.Count;
        }

        public virtual string[] GetVibratorCodes()
        {
            string[] codes = { BaseInputManager.NULL_MAPPING };
            return codes;
        }

        public virtual string GetVibratorMapping(int vibratorID)
        {
            return (vibratorID >= _vibratorMapping.Count) ? BaseInputManager.NULL_MAPPING : _vibratorMapping[vibratorID];
        }

        public virtual string GetVibratorMapping(string virtualVibrator)
        {
            List<string> virtualVibrators = QuickSingletonManager.GetInstance<QuickVibratorManager>().GetVirtualVibrators();
            for (int i = 0; i < virtualVibrators.Count; i++)
            {
                if (virtualVibrators[i] == virtualVibrator) return _vibratorMapping[i];
            }

            return BaseInputManager.NULL_MAPPING;
        }

        public virtual void ResetAllMapping()
        {
            _vibratorMapping.Clear();
            
            int numVibrators = QuickSingletonManager.GetInstance<QuickVibratorManager>().GetNumVibrators();
            for (int i = 0; i < numVibrators; i++) AddVibratorMapping();
        }

        public virtual void AddVibratorMapping()
        {
            _vibratorMapping.Add("");
        }

        public virtual void RemoveLastVibratorMapping()
        {
            _vibratorMapping.RemoveAt(_vibratorMapping.Count - 1);
        }

        public virtual void Vibrate(string virtualVibrator, float timeOut = 0.15f)
        {
            ImpVibrate(GetVibratorMapping(virtualVibrator));
            if (timeOut > 0.0f) StartCoroutine(CoStopVibrating(virtualVibrator, timeOut));
        }

        public abstract void ImpVibrate(string vibrator);

        public virtual void StopVibrating(string virtualVibrator)
        {
            ImpStopVibrating(GetVibratorMapping(virtualVibrator));
        }

        public abstract void ImpStopVibrating(string vibrator);

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoStopVibrating(string virtualVibrator, float timeOut)
        {
            yield return new WaitForSeconds(timeOut);

            StopVibrating(virtualVibrator);
        }

        #endregion

    }

}
