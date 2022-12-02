using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageInstantiatePlayer : QuickStageBase
    {

        #region PUBLIC ATTRIBUTES

        public Transform _playerOrigin = null;

        #endregion

        public override void Init()
        {
            base.Init();

            if (QuickStageChoosePlayer._selectedPlayer)
            {
                GameObject goPlayer = Instantiate(QuickStageChoosePlayer._selectedPlayer);
                if (_playerOrigin)
                {
                    goPlayer.transform.position = _playerOrigin.position;
                    goPlayer.transform.rotation = _playerOrigin.rotation;
                }
                
                _vrManager.SetAnimatorTarget(goPlayer.GetComponent<Animator>());
            }
        }

    }

}


