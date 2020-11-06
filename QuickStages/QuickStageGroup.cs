using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageGroup : QuickStageBase
    {

        #region PROTECTED ATTRIBUTES

        protected List<QuickStageBase> _stages = new List<QuickStageBase>();
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _avoidable = false;
        }

        public override void Init()
        {
            foreach (Transform t in transform)
            {
                QuickStageBase s = t.GetComponent<QuickStageBase>();
                if (s && s.gameObject.activeInHierarchy) _stages.Add(s);
            }

            base.Init();
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            foreach (QuickStageBase s in _stages)
            {
                s._finishPolicy = FinishPolicy.Nothing; //The next stage execution is controlled by the _stages order
                s.Init();
                while (!s.IsFinished()) yield return null;
            }
        }

        #endregion

    }

}


