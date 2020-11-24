using System;
using UnityEngine;
using TMPro;

namespace QuickVR
{
    public class QuickKeyboard : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public float _blinkTime = 0.5f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected string _text = "";
        protected TextMeshProUGUI _textInput = null;
        protected TextMeshProUGUI _textHint = null;

        protected float _timeBlinking = 0;
        protected QuickKeyboardKey[] _keys = null;

        protected bool _isEnabled = true;
        protected bool _shifted = false;

        protected Transform _rootKeys
        {
            get
            {
                return transform.CreateChild("__Keyboard__");
            }
        }

        #endregion

        #region ACTIONS

        public delegate void OnSubmintAction(string text);
        public event OnSubmintAction OnSubmit;

        #endregion

        #region CONSTANTS

        protected KeyCode[] _keysRow1 = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P };
        protected KeyCode[] _keysRow2 = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.Colon };
        protected KeyCode[] _keysRow3 = { KeyCode.LeftShift, KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M, KeyCode.Slash, KeyCode.Period };

        protected const float KEY_WIDTH = 0.16f;
        protected const float KEY_HEIGHT = 0.16f;

        #endregion

        #region CREATION AND DESTRUCTION

        //Ensure that the keyboard is loaded from the prefab. 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        protected static void Init()
        {
            if (!QuickSingletonManager.IsInstantiated<QuickKeyboard>())
            {
                QuickKeyboard keyboard = Instantiate(Resources.Load<QuickKeyboard>("Prefabs/pf_QuickKeyboard"));
                keyboard.name = "__QuickKeyboard__";
            }
        }

        protected virtual void Awake()
        {
            _textInput = transform.CreateChild("__TextInput__").GetComponentInChildren<TextMeshProUGUI>();

            CreateRowKeys(_keysRow1, 1);
            CreateRowKeys(_keysRow2, 2);
            CreateRowKeys(_keysRow3, 3);
            _keys = GetComponentsInChildren<QuickKeyboardKey>();

            foreach (QuickKeyboardKey k in _keys)
            {
                QuickUIButton button = k.GetOrCreateComponent<QuickUIButton>();
                button.OnDown += k.DoAction;
            }

            Enable(false);
        }

        protected virtual void CreateRowKeys(KeyCode[] rowKeys, int rowNum)
        {
            for (int i = 0; i < rowKeys.Length; i++)
            {
                QuickKeyboardKey key = Instantiate(Resources.Load<Transform>("Prefabs/pf_QuickKeyboardButton"), _rootKeys).GetOrCreateComponent<QuickKeyboardKey>();
                key.transform.localPosition = Vector3.right * (KEY_WIDTH * 0.5f + KEY_WIDTH * i);
                key.transform.localPosition += Vector3.down * ((KEY_HEIGHT * 0.5f) + (KEY_HEIGHT * rowNum));

                KeyCode c = rowKeys[i];
                key._keyCode = c;
                key._hasShiftedValue = ((int)c >= (int)KeyCode.A) && ((int)c <= (int)KeyCode.Z);

                if (c == KeyCode.LeftShift)
                {
                    key.SetLabel('\u25B2'.ToString());
                }
                else if (c == KeyCode.Colon)
                {
                    key.SetLabel(":");
                }
                else if (c == KeyCode.Period)
                {
                    key.SetLabel(".");
                }
                else if (c == KeyCode.Slash)
                {
                    key.SetLabel("/");
                }
                else
                {
                    //Letter key
                    key.SetLabel(c.ToString().ToLower());
                }

                key.name = "Key: " + c.ToString();
            }
        }

        #endregion

        #region GET AND SET

        public virtual void Enable(bool enable, bool clearTextOnEnable = true)
        {
            if (enable)
            {
                Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
                if (animator)
                {
                    Vector2 hSize = GetComponent<RectTransform>().GetSizeHalf();
                    Transform t = QuickVRManager.IsXREnabled() ? animator.transform : Camera.main.transform;
                    Vector3 pos = animator.GetBoneTransform(HumanBodyBones.Head).position;
                    transform.position = pos - (t.right * hSize.x) + (t.up * hSize.y) + t.forward * 2;
                    transform.rotation = t.rotation;

                }

            }

            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(enable);
            }

            if (enable && clearTextOnEnable)
            {
                SetText("");
            }

            if (!QuickVRManager.IsXREnabled())
            {
                InputManagerUnity iManager = QuickSingletonManager.GetInstance<InputManager>().GetComponentInChildren<InputManagerUnity>();
                iManager._active = !enable;
                _rootKeys.gameObject.SetActive(false);
            }

            _isEnabled = enable;
        }

        public virtual bool IsEnabled()
        {
            return _isEnabled;
        }

        public virtual void SetText(string txt)
        {
            _text = txt;
            _timeBlinking = 0;
        }

        public virtual void AddText(string txt)
        {
            SetText(_text + txt);
        }

        public virtual void Backspace()
        {
            string text = _text;
            if (text.Length > 0)
            {
                text = text.Substring(0, text.Length - 1);
            }

            SetText(text);
        }

        public virtual bool ToggleShift()
        {
            _shifted = !_shifted;

            foreach (QuickKeyboardKey key in _keys)
            {
                key.SetShifted(_shifted);
            }

            return _shifted;
        }

        public virtual void Submit()
        {
            if (OnSubmit != null)
            {
                OnSubmit(_text);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (!QuickVRManager.IsXREnabled())
            {
                UpdateKeyboardMono();
            }

            _textInput.text = _text;
            if (_timeBlinking < _blinkTime)
            {
                _textInput.text += "|";
            }

            _timeBlinking += Time.deltaTime;
            if (_timeBlinking > _blinkTime * 2)
            {
                _timeBlinking = 0;
            }
        }

        protected virtual void UpdateKeyboardMono()
        {
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                ToggleShift();
            }

            foreach (QuickKeyboardKey k in _keys)
            {
                if (k._keyCode != KeyCode.None && Input.GetKeyDown(k._keyCode))
                {
                    k.DoAction();
                }
            }
        }

        #endregion

    }
}


