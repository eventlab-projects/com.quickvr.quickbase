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

            RectTransform t = GetComponent<RectTransform>();
            BoxCollider collider = gameObject.GetOrCreateComponent<BoxCollider>();
            collider.size = new Vector3(t.rect.width, t.rect.height, 0);
            //collider.center = new Vector3(t.rect.width / 2, -t.rect.height / 2, 0);

            Rigidbody rBody = gameObject.GetOrCreateComponent<Rigidbody>();
            rBody.isKinematic = true;
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


