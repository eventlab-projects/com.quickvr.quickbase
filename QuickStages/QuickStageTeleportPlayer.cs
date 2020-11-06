using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageTeleportPlayer : QuickStageTeleport
    {
        public override void Init()
        {
            _sourceTransform = _gameManager.GetPlayer();

            base.Init();
        }

        protected override void Teleport()
        {
            if (_sourceTransform && _destTransform)
            {
                _gameManager.MovePlayerTo(_destTransform);

                if (_enableSourceTransform)
                    _sourceTransform.gameObject.SetActive(true);
            }
        }
    }

}
