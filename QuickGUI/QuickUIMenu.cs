using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
	public class QuickUIMenu : QuickMonoBehaviour {

		#region PUBLIC PARAMETERS

		public float _resolutionX = DEFAULT_PAGE_RESOLUTION;
		public float _resolutionY = DEFAULT_PAGE_RESOLUTION;

		public float _size = 1.0f;

		#endregion

		#region PROTECTED PARAMETERS

		protected int _pageSelectedID = 0;

		protected bool _isIdle = true;
		protected bool _isVisible = false;

        #endregion

		#region CONSTANTS

		public const float DEFAULT_PAGE_RESOLUTION = 1024.0f;

		public const string BUTTON_OPEN_CLOSE = "OpenCloseMenu";
		public const string BUTTON_PAGE_NEXT = "PageMenuNext";
		public const string BUTTON_PAGE_PREV = "PageMenuPrev";

		#endregion

		#region EVENTS

		public event Action OnOpen;		//Called when the menu is opened
		public event Action OnClose;	//Called when the menu is closed

		#endregion

		#region CREATION AND DESTRUCTION

        public override void Init() {
            base.Init();
        	CreatePages(1);
		}

        protected virtual void OnEnable() {
			foreach (Transform t in transform) {
				QuickUIMenuPage p = t.GetComponent<QuickUIMenuPage>();
				p.GetElement(QuickUIMenuPage.UIElement.IconNext).GetComponent<QuickUIInteractiveItem>().OnDown += ChangePageNext;
				p.GetElement(QuickUIMenuPage.UIElement.IconPrev).GetComponent<QuickUIInteractiveItem>().OnDown += ChangePagePrev;
			}

            InputManager.OnPostUpdateInput += UpdateInput;
		}

		protected virtual void OnDisable() {
			foreach (Transform t in transform) {
				QuickUIMenuPage p = t.GetComponent<QuickUIMenuPage>();
				p.GetElement(QuickUIMenuPage.UIElement.IconNext).GetComponent<QuickUIInteractiveItem>().OnDown -= ChangePageNext;
				p.GetElement(QuickUIMenuPage.UIElement.IconPrev).GetComponent<QuickUIInteractiveItem>().OnDown -= ChangePagePrev;
			}

            InputManager.OnPostUpdateInput -= UpdateInput;
		}

		public virtual void CreatePages(int numPages) {
			transform.localScale = Vector3.one;
			transform.DestroyChildsImmediate();
			for (int i = 0; i < numPages; i++) CreatePage<QuickUIMenuPage>("Page" + i.ToString(), transform);

			UpdateDimensions();
		}

		public virtual T CreatePage<T>(string pageName, Transform pageParent) where T : QuickUIMenuPage {
			Transform t = new GameObject(pageName).transform;

			T p = t.GetComponent<T>();
			if (!p) p = t.gameObject.AddComponent<T>();

			p.transform.SetParent(pageParent);
			p.transform.localPosition = Vector3.zero;
			p.transform.localRotation = Quaternion.identity;
			p.transform.localScale = Vector3.one;

			p.GetElementText(QuickUIMenuPage.UIElement.TextTitle).text = pageName;

			return p;
		}

        protected virtual void Start() {
            Close();

			InputManager iManager = QuickSingletonManager.GetInstance<InputManager>();
			iManager.CreateDefaultButton(BUTTON_OPEN_CLOSE);
			iManager.CreateDefaultButton(BUTTON_PAGE_NEXT);
			iManager.CreateDefaultButton(BUTTON_PAGE_PREV);

            InputManagerUnity iUnity = InputManager.GetInputImplementation<InputManagerUnity>();
            iUnity.ConfigureDefaultButton(BUTTON_OPEN_CLOSE, KeyCode.M, KeyCode.JoystickButton7);
            iUnity.ConfigureDefaultButton(BUTTON_PAGE_NEXT, KeyCode.RightArrow, KeyCode.JoystickButton5);
            iUnity.ConfigureDefaultButton(BUTTON_PAGE_PREV, KeyCode.LeftArrow, KeyCode.JoystickButton4);
		}

		#endregion

		#region GET AND SET

		public virtual QuickUIMenuPage GetPageSelected() {
			return GetPage(_pageSelectedID);
		}

        public virtual QuickUIInteractiveItem GetIconSelected() {
			QuickUIMenuPage page = GetPageSelected();
			return page.GetIconSelected();
		}

        public virtual int GetIconSelectedID()
        {
            QuickUIMenuPage page = GetPageSelected();
            return page.GetIconSelectedID();
        }

        public virtual QuickUIInteractiveItem GetIconTriggered()
        {
            return InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE) ? GetIconSelected() : null;
        }

        public virtual int GetIconTriggeredID()
        {
            return GetIconTriggered() ? GetIconSelectedID() : -1;
        }

		public virtual QuickUIMenuPage GetPage(int pageID) {
			if (pageID >= transform.childCount) return null;
			return transform.GetChild(pageID).GetComponent<QuickUIMenuPage>();
		}

		public virtual int GetNumPages() {
			return transform.childCount;
		}

		protected virtual void SetVisiblePage(int pageID, bool v) {
			GetPage(pageID).SetVisible(v);
		}

		public virtual void Open() {
			_isVisible = true;
			_pageSelectedID = 0;
			for (int i = 0; i < transform.childCount; i++) SetVisiblePage(i, (i == _pageSelectedID));

			if (OnOpen != null) OnOpen();
		}

		public virtual void Close() {
			_isVisible = false;
			for (int i = 0; i < transform.childCount; i++) SetVisiblePage(i, false);

			if (OnClose != null) OnClose();
		}

		#endregion

		#region UPDATE

		public virtual void UpdateDimensions() {
			float s = _size / _resolutionX;
			transform.localScale = new Vector3(s, s, s);
		}

        protected virtual void Update() {
            UpdateOpenClose();
			if (_isVisible && _isIdle) UpdateInput();
		}

        protected virtual void UpdateOpenClose()
        {
            if (InputManager.GetButtonDown(BUTTON_OPEN_CLOSE))
            {
                if (_isVisible) Close();
                else Open();
            }
        }

		private void UpdateInput() {
            if (_isVisible && _isIdle) ImpUpdateInput();
		}

        protected virtual void ImpUpdateInput()
        {
            if (InputManager.GetButtonDown(BUTTON_PAGE_NEXT)) ChangePageNext();
            if (InputManager.GetButtonDown(BUTTON_PAGE_PREV)) ChangePagePrev();
        }

		protected virtual void ChangePageNext() {
			_isIdle = false;
			StartCoroutine(CoChangePage(1.0f));
		}

		protected virtual void ChangePagePrev() {
			_isIdle = false;
			StartCoroutine(CoChangePage(-1.0f));
		}

		protected virtual IEnumerator CoChangePage(float sign) {
			//Set the selected canvas according to the currently selected canvas and the sign of the rotation
			SetVisiblePage(_pageSelectedID, false);
			_pageSelectedID = (sign > 0)? (_pageSelectedID + 1) % GetNumPages() : _pageSelectedID - 1;
			if (_pageSelectedID < 0) _pageSelectedID = GetNumPages() - 1;
			SetVisiblePage(_pageSelectedID, true);

			yield return null;

			_isIdle = true;
		}

		#endregion

	}

}
