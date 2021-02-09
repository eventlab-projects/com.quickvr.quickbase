using UnityEngine;
using System.Collections;

namespace QuickVR {

	public abstract class QuickStageCondition : QuickStageBase {

		#region PROTECTED PARAMETERS

		protected QuickStageGroup _ifGroup = null;
        protected QuickStageGroup _elseGroup = null;

        protected QuickStageGroup _executedGroup = null;

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
            _ifGroup._finishPolicy = FinishPolicy.Nothing;

            _elseGroup = transform.CreateChild("__ELSE__").GetOrCreateComponent<QuickStageGroup>();
            _elseGroup._finishPolicy = FinishPolicy.Nothing;
        }

		public override void Init() {
            _avoidable = false;
            _executedGroup = Condition() ? _ifGroup : _elseGroup;
            _executedGroup.Init();

            base.Init();
        }

        #endregion

        #region GET AND SET

        protected abstract bool Condition();

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            while (!_executedGroup.IsFinished()) yield return null;
        }

        #endregion

    }

}
