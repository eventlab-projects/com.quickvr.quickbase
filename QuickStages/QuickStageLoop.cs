using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	public class QuickStageLoop : QuickStageGroup
    {

		#region PUBLIC PARAMETERS

		public int _numIterations = 1;

		#endregion

		#region PROTECTED PARAMETERS

		protected int _currentIteration = 0;

        #endregion

        #region GET AND SET

        public virtual int GetCurrentInteration()
        {
            return _currentIteration;
        }

        protected virtual void IncreaseCurrentIteration()
        {
            _currentIteration++;
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            for (_currentIteration = 0; _currentIteration < _numIterations; IncreaseCurrentIteration())
            {
                yield return StartCoroutine(base.CoUpdate());
            }
        }

        #endregion
	}

}
