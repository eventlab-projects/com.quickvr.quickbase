using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxisMapping {

    #region PUBLIC PARAMETERS

    public bool _showInInspector = true;

    public string _axisCode = BaseInputManager.NULL_MAPPING;

    [HideInInspector]
    public float _value = 0.0f;

    #endregion

    #region PROTECTED PARAMETERS

    [SerializeField] 
	protected ButtonMapping _positiveButton = new ButtonMapping();

	[SerializeField] 
	protected ButtonMapping _negativeButton = new ButtonMapping();

	#endregion

	#region GET AND SET

	public ButtonMapping GetPositiveButton() {
		return _positiveButton;
	}

	public ButtonMapping GetNegativeButton() {
		return _negativeButton;
	}

    #endregion

}