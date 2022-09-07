using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace QuickVR
{

    public class QuickUIButton : QuickUIInteractiveItem
    {

        #region PUBLIC ATTRIBUTES

        public Color _colorNormal = Color.white;
        public Color _colorSelected = Color.cyan;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Image _buttonBG = null;
        protected TextMeshProUGUI _label = null;

        protected static Material _buttonBGMaterial
        {
            get
            {
                if (!m_ButtonBGMaterial)
                {
                    m_ButtonBGMaterial = Resources.Load<Material>("Materials/GUIText");
                }

                return m_ButtonBGMaterial;
            }
        }
        protected static Material m_ButtonBGMaterial = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _buttonBG = gameObject.GetOrCreateComponent<Image>();
            _buttonBG.material = new Material(_buttonBGMaterial);
            _buttonBG.color = _colorNormal;
        }

        protected override void OnDisable()
        {
            _buttonBG.color = _colorNormal;

            base.OnDisable();
        }

        #endregion

        #region UPDATE

        public override void Over()
        {
            base.Over();

            _buttonBG.color = _colorSelected;
        }

        public override void Out()
        {
            base.Out();

            _buttonBG.color = _colorNormal;
        }

        protected virtual void Update()
        {
            if (_buttonBG.material)
            {
                _buttonBG.material.color = _buttonBG.color;
            }
        }

        #endregion

    }

}


