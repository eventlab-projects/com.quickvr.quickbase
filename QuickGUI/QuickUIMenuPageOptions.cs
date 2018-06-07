using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickUIMenuPageOptions : QuickUIMenuPage
    {

        #region PUBLIC PARAMETERS

        public List<string> _options = new List<string>();

        #endregion

        #region CREATION AND DESTRUCTION

        public override void CreateIconMatrix()
        {
            _iconsRoot = transform.CreateChild("__IconsRoot__");
            _iconsRoot.DestroyChildsImmediate();
            Vector2 iconSize = new Vector2(_width * 0.8f, 64.0f);
            Vector2 iconOffset = new Vector2(32.0f, 32.0f);

            Resize();
            Vector3 tl = GetCornerPosition(UICorner.TOP_LEFT, Space.Self) - Vector3.up * (BACKGROUND_MARGIN_SIZE * 2.0f + 64.0f);
            
            for (int i = 0; i < _options.Count; i++)
            {
                Vector3 pos = new Vector3(0.0f, tl.y - (iconSize.y * (i + 0.5f) + iconOffset.y * i), 0.0f);
                string iconName = "Icon_" + i.ToString();
                RawImage icon = CreateButton(iconName, _imgBackground, pos, iconSize);
                icon.transform.SetParent(_iconsRoot);
                icon.transform.localRotation = Quaternion.identity;
                icon.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        protected virtual RawImage CreateButton(string iName, Texture tex, Vector3 pos, Vector2 size, string description = "")
        {
            RawImage img = CreateUIElement<RawImage>(iName, pos);
            img.GetComponent<RectTransform>().sizeDelta = size;
            img.texture = tex;

            Text text = CreateText(iName + "_text", "Default Text", pos);
            text.transform.SetParent(img.transform);
            text.transform.localRotation = Quaternion.identity;

            BoxCollider bCollider = img.gameObject.AddComponent<BoxCollider>();
            bCollider.size = new Vector3(size.x, size.y, 0.0f);
            QuickUIInteractiveItem item = img.gameObject.AddComponent<QuickUIInteractiveItem>();
            item._description = description;

            return img;
        }

        #endregion

        #region GET AND SET

        protected virtual void Resize()
        {
            Vector2 iconSize = new Vector2(_width * 0.8f, 64.0f);
            Vector2 iconOffset = new Vector2(32.0f, 32.0f);
            _height = (BACKGROUND_MARGIN_SIZE * 2.0f + 64.0f) + _options.Count * (iconSize.y + iconOffset.y);
            Vector2 canvasSize = new Vector2(_width, _height);
            _canvas.GetComponent<RectTransform>().sizeDelta = _background.GetComponent<RectTransform>().sizeDelta = canvasSize;

            _textTitle.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, _height * 0.5f - 64.0f, 0.0f);

            GetComponent<BoxCollider>().size = new Vector3(canvasSize.x, canvasSize.y, 0.0f);
        }

        public override void SetVisible(bool v)
        {
            base.SetVisible(v);

            SetActiveElement(UIElement.IconNext, false);
            SetActiveElement(UIElement.IconPrev, false);
        }

        #endregion

        #region UPDATE

        protected override void Update()
        {
            base.Update();

            UpdateOptions();
        }

        public virtual void UpdateOptions()
        {
            if (_options.Count != _iconsRoot.childCount) CreateIconMatrix();

            for (int i = 0; i < _options.Count; i++)
            {
                _iconsRoot.GetChild(i).GetComponentInChildren<Text>().text = _options[i];
            }
        }

        #endregion

    }

}
