using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        #endregion

        protected virtual void Awake()
        {
            _buttonBG = gameObject.GetOrCreateComponent<Image>();
            _buttonBG.color = _colorNormal;
        }

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

    }

}


