using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickCoroutineManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected Dictionary<int, List<Coroutine>> _coroutineSets = new Dictionary<int, List<Coroutine>>();
        protected int _currentCoroutineSetID = -1;
        protected List<int> _coroutineSetHistory = new List<int>();
        protected int _newCoroutineSetID = 0;

        #endregion

        #region GET AND SET

        public virtual int BeginCoroutineSet()
        {
            _currentCoroutineSetID = _newCoroutineSetID++;
            _coroutineSetHistory.Add(_currentCoroutineSetID);
            return _currentCoroutineSetID;
        }

        public virtual void EndCoroutineSet()
        {
            if (_coroutineSetHistory.Count == 0) {
                _currentCoroutineSetID = -1;
                return;
            }
            
            int last = _coroutineSetHistory.Count - 1;
            _currentCoroutineSetID = _coroutineSetHistory[last];
            _coroutineSetHistory.RemoveAt(last);
        }

        public new Coroutine StartCoroutine(IEnumerator coroutine)
        {
            Coroutine c = base.StartCoroutine(coroutine);
            if (_currentCoroutineSetID != -1)
            {
                if (!_coroutineSets.ContainsKey(_currentCoroutineSetID)) _coroutineSets[_currentCoroutineSetID] = new List<Coroutine>();
                _coroutineSets[_currentCoroutineSetID].Add(c);
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

        #endregion

    }

}
