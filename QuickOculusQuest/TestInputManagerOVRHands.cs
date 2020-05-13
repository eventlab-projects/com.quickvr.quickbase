using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

public class TestInputManagerOVRHands : MonoBehaviour
{

    #region PUBLIC ATTRIBUTES

    public GameObject _testSphere = null;

    public GameObject _testCubeLeft = null;
    public GameObject _testCubeRight = null;

    #endregion

    #region PROTECTED ATTRIBUTES

    protected QuickOVRHandsInitializer _handsInitializer = null;

    #endregion

    #region CREATION AND DESTRUCTION

    protected virtual void Start()
    {
        _handsInitializer = FindObjectOfType<QuickOVRHandsInitializer>();
        Vector3 scale = new Vector3(0.05f, 0.05f, 0.05f);
        if (_testCubeLeft)
        {
            _testCubeLeft.transform.parent = _handsInitializer.GetOVRHand(true).transform;
            _testCubeLeft.transform.ResetTransformation();
            _testCubeLeft.transform.localScale = scale;
        }

        if (_testCubeRight)
        {
            _testCubeRight.transform.parent = _handsInitializer.GetOVRHand(false).transform;
            _testCubeRight.transform.ResetTransformation();
            _testCubeRight.transform.localScale = scale;
        }
    }

    #endregion

    #region UPDATE

    protected virtual void Update()
    {
        _testSphere.GetComponent<Renderer>().material.color = Color.white;
        if (InputManager.GetButton(InputManager.DEFAULT_BUTTON_CONTINUE))
        {
            _testSphere.GetComponent<Renderer>().material.color = Color.yellow;
        }
        else if (InputManager.GetButton(InputManager.DEFAULT_BUTTON_CANCEL))
        {
            _testSphere.GetComponent<Renderer>().material.color = Color.black;
        }
        else if (InputManager.GetButton(InputManager.DEFAULT_BUTTON_CALIBRATE))
        {
            _testSphere.GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    #endregion
    
}
