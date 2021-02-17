using UnityEngine;
using System.Collections;

namespace QuickVR {

	public abstract class QuickStageCondition : QuickStageBase 
    {

		#region PROTECTED PARAMETERS

		protected QuickStageGroup _ifGroup = null;
        protected QuickStageGroup _elseGroup = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            Reset();
        }

        protected virtual void Reset()
        {
            _ifGroup = transform.CreateChild("__IF__").GetOrCreateComponent<QuickStageGroup>();
            _elseGroup = transform.CreateChild("__ELSE__").GetOrCreateComponent<QuickStageGroup>();
        }

		public override void Init() 
        {
            _avoidable = false;

            base.Init();
        }

        #endregion

        #region GET AND SET

        protected abstract bool Condition();

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            QuickStageGroup executedGroup;
            if (Condition())
            {
                executedGroup = _ifGroup;
                _elseGroup.gameObject.SetActive(false);
            }
            else
            {
                executedGroup = _elseGroup;
                _ifGroup.gameObject.SetActive(false);
            }

            executedGroup.gameObject.SetActive(true);
            executedGroup.Init();

            while (GetTopStage() != this)
            {
                yield return null;
            }
        }

        #endregion

    }

}
