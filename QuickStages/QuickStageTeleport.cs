using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace QuickVR
{

    public class QuickStageTeleport : QuickStageBase
    {

        #region PUBLIC PARAMETERS

        public Transform _sourceTransform = null;
        public Transform _destTransform = null;

        public bool _enableSourceTransform = true;

        #endregion

        #region CREATION AND DESTRUCTION

        public override void Init()
        {
            base.Init();

            Teleport();
        }

        #endregion

        #region GET AND SET

        protected virtual void Teleport()
        {
            if (_sourceTransform && _destTransform)
            {
                _sourceTransform.position = _destTransform.position;
                _sourceTransform.rotation = _destTransform.rotation;

                if (_enableSourceTransform)
                    _sourceTransform.gameObject.SetActive(true);
            }
        }

        #endregion

    }



}