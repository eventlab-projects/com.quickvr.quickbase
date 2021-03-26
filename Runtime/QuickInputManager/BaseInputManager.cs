using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{
	[System.Serializable]
	public class AxisMapping
	{

		#region PUBLIC PARAMETERS

		public bool _showInInspector = true;

		public string _axisCode = BaseInputManager.NULL_MAPPING;
		public ButtonMapping _positiveButton = new ButtonMapping();
		public ButtonMapping _negativeButton = new ButtonMapping();

		#endregion

	}


	[System.Serializable]
	public class ButtonMapping
	{

		#region PUBLIC PARAMETERS

		public bool _showInInspector = true;

		public string _keyCode = BaseInputManager.NULL_MAPPING;
		public string _altKeyCode = BaseInputManager.NULL_MAPPING;

		#endregion

	}

	[System.Serializable]
	public abstract class BaseInputManager : MonoBehaviour
	{

		#region PUBLIC PARAMETERS

		public enum DefaultCode
		{
			None,
		}

		#endregion

		#region PROTECTED PARAMETERS

		protected static InputManager _inputManager
		{
			get
			{
				if (!m_InputManager)
				{
					m_InputManager = QuickSingletonManager.GetInstance<InputManager>();
				}
				return m_InputManager;
			}
		}
		protected static InputManager m_InputManager = null;

        [SerializeField, HideInInspector]
        protected List<AxisMapping> _axisMapping = new List<AxisMapping>();

        [SerializeField, HideInInspector]
        protected List<ButtonMapping> _buttonMapping = new List<ButtonMapping>();

		[SerializeField, HideInInspector]
		protected bool _initialized = false;

		#endregion

		#region CONSTANTS

		public const string NULL_MAPPING = "None";

		protected const string ROOT_AXIS_MAPPING_NAME = "_AxisMappingRoot_";
		protected const string ROOT_BUTTON_MAPPING_NAME = "_ButtonMappingRoot_";

		protected const string AXIS_MAPPING_NAME = "_AxisMapping_";
		protected const string BUTTON_MAPPING_NAME = "_ButtonMapping_";

		#endregion

		#region ABSTRACT FUNCTIONS TO IMPLEMENT

		protected abstract float ImpGetAxis(string axis);
		protected abstract bool ImpGetButton(string button);

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake()
		{
			Reset();
		}

		protected virtual void Reset()
		{
			name = this.GetType().FullName;

			CheckAxesMapping();
			CheckButtonsMapping();
			
			if (!_initialized)
            {
				ResetDefaultConfiguration();
				_initialized = true;
			}
		}

		public virtual void CheckAxesMapping()
        {
			//Check if we have an axis mapping for each defined virtual axis
			int numVirtualAxes = _inputManager.GetVirtualAxes().Count;
			while (_axisMapping.Count < numVirtualAxes)
			{
				_axisMapping.Add(new AxisMapping());
			}
			while (_axisMapping.Count > numVirtualAxes)
			{
				_axisMapping.RemoveAt(_axisMapping.Count - 1);
			}
		}

		public virtual void CheckButtonsMapping()
        {
			//Check if we have a button mapping for each defined virtual button
			int numVirtualButtons = _inputManager.GetVirtualButtons().Count;
			while (_buttonMapping.Count < numVirtualButtons)
			{
				_buttonMapping.Add(new ButtonMapping());
			}
			while (_buttonMapping.Count > numVirtualButtons)
			{
				_buttonMapping.RemoveAt(_buttonMapping.Count - 1);
			}
		}

		protected virtual void ResetDefaultConfiguration()
        {

        }

		protected virtual void ConfigureDefaultAxis(string virtualAxisName, string axisName)
		{
			AxisMapping aMapping = GetAxisMapping(_inputManager.ToAxisID(virtualAxisName));
			if (aMapping != null)
			{
				if (aMapping._axisCode == NULL_MAPPING)
				{
					aMapping._axisCode = axisName;
				}
			}
		}

		protected virtual void ConfigureDefaultAxis(string virtualAxisName, string kPositive, string kNegative)
		{
			AxisMapping aMapping = GetAxisMapping(_inputManager.ToAxisID(virtualAxisName));
			if (aMapping == null) return;

			if (aMapping._positiveButton._keyCode == NULL_MAPPING)
			{
				aMapping._positiveButton._keyCode = kPositive;
			}
			if (aMapping._negativeButton._keyCode == NULL_MAPPING)
			{
				aMapping._negativeButton._keyCode = kNegative;
			}
		}

		protected virtual void ConfigureDefaultButton(string virtualButtonName, string key, string altKey = NULL_MAPPING)
		{
			ButtonMapping bMapping = GetButtonMapping(_inputManager.ToButtonID(virtualButtonName));
			if (bMapping == null) return;

			if (bMapping._keyCode == NULL_MAPPING) bMapping._keyCode = key;
			if (bMapping._altKeyCode == NULL_MAPPING) bMapping._altKeyCode = altKey;
		}

		#endregion

		#region MAPPING

		public AxisMapping GetAxisMapping(int axisID)
		{
			return (axisID >= 0 && axisID < _axisMapping.Count) ? _axisMapping[axisID] : null;
		}

		public ButtonMapping GetButtonMapping(int buttonID)
		{
			return (buttonID >= 0 && buttonID <= _buttonMapping.Count) ? _buttonMapping[buttonID] : null;
		}

		public virtual int GetNumAxesMapped()
		{
			return _axisMapping.Count;
		}

		public virtual int GetNumButtonsMapped()
		{
			return _buttonMapping.Count;
		}

		#endregion

		#region GET AND SET

		public virtual string[] GetAxisCodes()
		{
			string[] codes = { NULL_MAPPING };
			return codes;
		}

		public virtual string[] GetButtonCodes()
		{
			string[] codes = { NULL_MAPPING };
			return codes;
		}

		protected virtual string[] GetCodes<T>()
		{
			string[] names = System.Enum.GetNames(typeof(T));
			List<string> tmp = new List<string>();
			for (int i = 0; i < names.Length; i++)
			{
				tmp.Add(names[i]);
			}

			return GetCodes(tmp);
		}

		protected virtual string[] GetCodes(List<string> tmp)
		{
			//Force the option "None" to be the first one. 
			tmp.Remove(NULL_MAPPING);
			List<string> codes = new List<string>();
			codes.Add(NULL_MAPPING);
			codes.AddRange(tmp);

			return codes.ToArray();
		}

		public virtual float GetAxis(string axisCode)
		{
			return enabled? ImpGetAxis(axisCode) : 0;
		}

		public virtual bool GetButton(string buttonCode)
        {
			return enabled? ImpGetButton(buttonCode) : false;
        }

		#endregion

	}
}

