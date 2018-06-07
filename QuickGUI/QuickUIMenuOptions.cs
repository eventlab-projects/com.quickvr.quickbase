using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

    public class QuickUIMenuOptions : QuickUIMenu
    {

        #region PUBLIC PARAMETERS

        public float _buttonWidth = 1024.0f;
        public float _buttonHeight = 128.0f;

        public List<string> _options = new List<string>();

        #endregion

        #region CREATION AND DESTRUCTION

        public override void CreatePages(int numPages)
        {
            transform.localScale = Vector3.one;
            transform.DestroyChildsImmediate();
            for (int i = 0; i < numPages; i++)
            {
                QuickUIMenuPageOptions page = CreatePage<QuickUIMenuPageOptions>("Page" + i.ToString(), transform);
                page.transform.Rotate(Vector3.up, 180.0f, Space.Self);
            }

            UpdateDimensions();
        }

        #endregion

        #region UPDATE

        protected override void UpdateOpenClose() { }

        #endregion

    }

}
