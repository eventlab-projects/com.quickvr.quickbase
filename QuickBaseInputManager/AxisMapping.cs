using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxisMapping : MonoBehaviour {

    #region PUBLIC PARAMETERS

    public bool _showInInspector = true;

    public string _axisCode = BaseInputManager.NULL_MAPPING;

    [HideInInspector]
    public float _value = 0.0f;

    #endregion

    #region PROTECTED PARAMETERS

    [SerializeField] protected ButtonMapping _positiveButton = null;
	[SerializeField] protected ButtonMapping _negativeButton = null;
	[SerializeField] protected GameObject _buttonMappingGO = null;
	
	#endregion

	#region CREATION AND DESTRUCTION

    public virtual void Init() {
		if (!_buttonMappingGO) {
			_buttonMappingGO = new GameObject("_ButtonMapping_");
			_positiveButton = _buttonMappingGO.AddComponent<ButtonMapping>();
			_negativeButton = _buttonMappingGO.AddComponent<ButtonMapping>();

			_buttonMappingGO.transform.parent = transform;
			_buttonMappingGO.transform.localPosition = Vector3.zero;
		}
	}

	protected virtual void OnDestroy() {
		if (_buttonMappingGO) {
			DestroyImmediate(_buttonMappingGO);
			_buttonMappingGO = null;
		}
		_positiveButton = null;
		_negativeButton = null;
	}

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