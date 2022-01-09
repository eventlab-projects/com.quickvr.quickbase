using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

using System.Collections;

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

        protected Coroutine _coUpdateTextHintError = null;

        #endregion

        #region ACTIONS

        public delegate void OnSubmintAction(string text);
        public event OnSubmintAction OnSubmit;

        #endregion

        #region CONSTANTS

        protected Key[] _keysRowNum = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0 };
        protected Key[] _keysRow1 = { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P };
        protected Key[] _keysRow2 = { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.Semicolon };
        protected Key[] _keysRow3 = { Key.LeftShift, Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.Slash, Key.Period };

        protected const float KEY_WIDTH = 0.16f;
        protected const float KEY_HEIGHT = 0.16f;

        protected const string TEXT_INPUT_NAME = "__TextInput__";
        protected const string TEXT_HINT_NAME = "__TextInputHint__";

        protected Color DEFAULT_COLOR_TEXT_HINT = new Color(0.0f, 0.043f, 1.0f);
        protected Color DEFAULT_COLOR_TEXT_HINT_ERROR = new Color(1.0f, 0.043f, 0.0f);

        #endregion

        #region CREATION AND DESTRUCTION

        //Ensure that the keyboard is loaded from the prefab. 
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        //protected static void Init()
        //{
        //    if (!QuickSingletonManager.IsInstantiated<QuickKeyboard>())
        //    {
        //        QuickKeyboard keyboard = Instantiate(Resources.Load<QuickKeyboard>("Prefabs/pf_QuickKeyboard"));
        //        keyboard.name = "__QuickKeyboard__";
        //    }
        //}

        protected virtual void Awake()
        {
            _textInput = transform.Find(TEXT_INPUT_NAME).GetComponentInChildren<TextMeshProUGUI>();
            _textHint = transform.Find(TEXT_HINT_NAME).GetComponentInChildren<TextMeshProUGUI>();

            CreateRowKeys(_keysRowNum, 0);
            CreateRowKeys(_keysRow1, 1);
            CreateRowKeys(_keysRow2, 2);
            CreateRowKeys(_keysRow3, 3);
            _keys = GetComponentsInChildren<QuickKeyboardKey>();

            foreach (QuickKeyboardKey k in _keys)
            {
                QuickUIButton button = k.GetOrCreateComponent<QuickUIButton>();
                button.OnDown += k.DoAction;
            }

            SetTextHint("");
            SetText("");
            
            Enable(false);
        }

        protected virtual void CreateRowKeys(Key[] rowKeys, int rowNum)
        {
            for (int i = 0; i < rowKeys.Length; i++)
            {
                QuickKeyboardKey key = Instantiate(Resources.Load<Transform>("Prefabs/pf_QuickKeyboardButton"), _rootKeys).GetOrCreateComponent<QuickKeyboardKey>();
                key.transform.localPosition = Vector3.right * (KEY_WIDTH * 0.5f + KEY_WIDTH * i);
                key.transform.localPosition += Vector3.down * ((KEY_HEIGHT * 0.5f) + (KEY_HEIGHT * rowNum));

                Key c = rowKeys[i];
                key._keyCode = c;
                key._hasShiftedValue = ((int)c >= (int)Key.A) && ((int)c <= (int)Key.Z);

                if (c == Key.LeftShift)
                {
                    key.SetLabel('\u25B2'.ToString());
                }
                else if (c == Key.Semicolon)
                {
                    key.SetLabel(":");
                }
                else if (c == Key.Period)
                {
                    key.SetLabel(".");
                }
                else if (c == Key.Slash)
                {
                    key.SetLabel("/");
                }
                else if (c == Key.Digit0)
                {
                    key.SetLabel("0");
                }
                else if (c >= Key.Digit1 && c <= Key.Digit9)
                {
                    key.SetLabel(((int)c - (int)Key.Digit1 + 1).ToString());
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
                InputManagerKeyboard iManager = QuickSingletonManager.GetInstance<InputManager>().GetComponentInChildren<InputManagerKeyboard>();
                iManager.enabled = !enable;
                _rootKeys.gameObject.SetActive(false);
            }

            _isEnabled = enable;
        }

        public virtual bool IsEnabled()
        {
            return _isEnabled;
        }

        public virtual void SetTextHint(string txt)
        {
            SetTextHint(txt, DEFAULT_COLOR_TEXT_HINT);
        }

        public virtual void SetTextHint(string txt, Color color)
        {
            if (_coUpdateTextHintError != null)
            {
                StopCoroutine(_coUpdateTextHintError);
            }

            _textHint.text = txt;
            _textHint.faceColor = color;
        }

        public virtual void SetTextHintError(string txt)
        {
            SetTextHintError(txt, DEFAULT_COLOR_TEXT_HINT_ERROR);
        }

        public virtual void SetTextHintError(string txt, Color color)
        {
            _coUpdateTextHintError = StartCoroutine(CoUpdateTextHintError(txt, color));
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
            if (InputManagerKeyboard.GetKeyUp(Key.LeftShift))
            {
                ToggleShift();
            }

            foreach (QuickKeyboardKey k in _keys)
            {
                if (k._keyCode != Key.None && InputManagerKeyboard.GetKeyDown(k._keyCode))
                {
                    k.DoAction();
                }
            }
        }

        protected virtual IEnumerator CoUpdateTextHintError(string text, Color color)
        {
            string prevText = _textHint.text;
            Color prevColor = _textHint.faceColor;
            _textHint.text = text;
            _textHint.faceColor = color;

            yield return new WaitForSeconds(5);

            SetTextHint(prevText, prevColor);
        }

        #endregion

    }
}


