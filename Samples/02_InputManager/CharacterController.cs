using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

namespace QuickVR.Samples
{
    public class CharacterController : MonoBehaviour
    {

        public bool _move = true;

        public float _maxLinearSpeed = 3;
        public float _maxAngularSpeed = 45;

        void Update()
        {
            if (InputManager.GetButtonDown("ToggleMove"))
            {
                _move = !_move;
            }

            if (_move)
            {
                float vValue = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
                transform.Translate(Vector3.forward * vValue * _maxLinearSpeed * Time.deltaTime, Space.Self);

                float hValue = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
                transform.Rotate(Vector3.up, hValue * _maxAngularSpeed * Time.deltaTime, Space.Self);
            }
        }

    }

}





