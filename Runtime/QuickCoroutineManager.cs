using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickCoroutineManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected Dictionary<int, List<Coroutine>> _coroutineSets = new Dictionary<int, List<Coroutine>>();
        protected int _newCoroutineSetID = 0;

        #endregion

        #region GET AND SET

        public virtual int BeginCoroutineSet()
        {
            return _newCoroutineSetID++;
        }

        public Coroutine StartCoroutine(IEnumerator coroutine, int coSet = -1)
        {
            Coroutine c = base.StartCoroutine(coroutine);
            if (coSet != -1)
            {
                if (!_coroutineSets.ContainsKey(coSet)) _coroutineSets[coSet] = new List<Coroutine>();
                _coroutineSets[coSet].Add(c);
            }
            
            return c;
        }

        public virtual IEnumerator WaitForCoroutineSet(int setID)
        {
            if (!_coroutineSets.ContainsKey(setID)) yield break;
            
            List<Coroutine> coroutines = _coroutineSets[setID];
            foreach (Coroutine c in coroutines) yield return c;
            _coroutineSets.Remove(setID);
        }

        public virtual void StopCoroutineSet(int setID)
        {
            if (!_coroutineSets.ContainsKey(setID)) return;

            List<Coroutine> coroutines = _coroutineSets[setID];
            foreach (Coroutine c in coroutines)
            {
                if (c != null) StopCoroutine(c);
            }
        }

        #endregion

    }

}
