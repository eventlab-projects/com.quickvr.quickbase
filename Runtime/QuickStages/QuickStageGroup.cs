using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageGroup : QuickStageBase
    {

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _avoidable = false;
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            QuickStageBase firstStage = null;
            for (int i = 0; !firstStage && i < transform.childCount; i++)
            {
                Transform tChild = transform.GetChild(i);
                if (tChild.gameObject.activeSelf)
                {
                    firstStage = tChild.GetComponent<QuickStageBase>();
                }
            }

            if (firstStage)
            {
                firstStage.Init();
            }
            
            while (GetTopStage() != this)
            {
                yield return null;
            }
        }

        #endregion

    }

}


