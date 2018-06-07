using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
	public class QuickUIMenuPage : QuickMonoBehaviour {

		#region PUBLIC PARAMETERS 

        public enum UICorner
        {
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_RIGHT,
        }

		public enum UIElement {
			Background,
			TextTitle,
			TextDescription,
			IconPrev,
			IconNext,
		}

		public float _width = 1024.0f;
		public float _height = 1024.0f;

		public Texture _imgBackground = null;
		public Texture _imgIconPrev = null;
		public Texture _imgIconNext = null;

		#endregion

		#region PROTECTED PARAMETERS

        [SerializeField] protected Canvas _canvas = null;
		[SerializeField] protected RawImage _background = null;
		[SerializeField] protected Text _textTitle = null;
		[SerializeField] protected Text _textDescription = null;
		[SerializeField] protected RawImage _iconPrev = null;
		[SerializeField] protected RawImage _iconNext = null;

		[SerializeField] protected Transform _iconsRoot = null;
		[SerializeField] protected Texture _iconDefaultTex = null;

		protected QuickUICursor _cursor = null;
		protected QuickUIInteractiveItem _iconSelected = null;

		protected List<QuickUIMenuPage> _childPages = new List<QuickUIMenuPage>();

        protected Collider[] _colliders;

        protected QuickHeadTracking _headTracking = null;
        
        #endregion

		#region CONSTANTS

		protected const float BACKGROUND_MARGIN_SIZE = 32.0f;

		#endregion

		#region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            //Ensure that all the colliders are set to the UI layer and are triggers. 
            _colliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in _colliders)
            {
                c.isTrigger = true;
                c.gameObject.layer = LayerMask.NameToLayer("UI");
            }
        }

        public override void Init()
        {
            base.Init();

            _iconDefaultTex = Resources.Load<Texture>("DefaultIcon");

            _imgBackground = Resources.Load<Texture>("UIPanel");
            _imgIconPrev = Resources.Load<Texture>("ArrowLeft");
            _imgIconNext = Resources.Load<Texture>("ArrowRight");

            _canvas = gameObject.GetComponent<Canvas>();
            if (!_canvas) _canvas = gameObject.AddComponent<Canvas>();
            _canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(_width, _height);

            _background = CreateBackground();

            //Create the text components
            _textTitle = CreateText(UIElement.TextTitle.ToString(), "TITLE HERE", new Vector3(0.0f, (_height - 64.0f) * 0.5f - BACKGROUND_MARGIN_SIZE, 0.0f));
            _textDescription = CreateText(UIElement.TextDescription.ToString(), "DESCRIPTION HERE", new Vector3(0.0f, (-_height + 64.0f) * 0.5f + BACKGROUND_MARGIN_SIZE, 0.0f));

            //Create the prev and next icons
            _iconPrev = CreateIcon(UIElement.IconPrev.ToString(), _imgIconPrev, new Vector3((-_width + 128.0f) * 0.5f + BACKGROUND_MARGIN_SIZE, 0.0f, 0.0f), new Vector2(128.0f, 128.0f), "Move to Previous Page");
            _iconNext = CreateIcon(UIElement.IconNext.ToString(), _imgIconNext, new Vector3((_width - 128.0f) * 0.5f - BACKGROUND_MARGIN_SIZE, 0.0f, 0.0f), new Vector2(128.0f, 128.0f), "Move to Next Page");

            //Add the collider of this page
            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            if (!collider) collider = gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(_width, _height, 0.0f);
            collider.center = new Vector3(0.0f, 0.0f, 1.0f);

            //Create the icon matrix
            CreateIconMatrix();
        }

		protected virtual T CreateUIElement<T>(string tName, Vector3 pos) where T : MaskableGraphic {
			Transform t = _canvas.transform.Find(tName);
			if (!t) t = new GameObject(tName).transform;

			t.transform.SetParent(_canvas.transform);
			t.transform.localPosition = pos;
            t.transform.localRotation = Quaternion.identity;

			T element = t.GetComponent<T>();
			if (!element) element = t.gameObject.AddComponent<T>();

			return element;
		}

		protected virtual RawImage CreateBackground() {
			RawImage bg = CreateUIElement<RawImage>("Background", Vector3.zero);
			bg.texture = _imgBackground;
			bg.color = new Color(1,1,1, 100.0f / 255.0f);
			bg.GetComponent<RectTransform>().sizeDelta = new Vector2(_width, _height);

			return bg;
		}

		protected virtual Text CreateText(string tName, string defaultText, Vector3 pos) {
			Text text = CreateUIElement<Text>(tName, pos);
            text.font = Resources.Load<Font>("Fonts/Arial");
			text.GetComponent<RectTransform>().sizeDelta = new Vector2(_width, 64.0f);
			text.fontSize = 28;
			text.color = Color.black;
			text.alignment = TextAnchor.MiddleCenter;
			text.text = defaultText;

			return text;
		}

		protected virtual RawImage CreateIcon(string iName, Texture tex, Vector3 pos, Vector2 size, string description = "") {
			RawImage img = CreateUIElement<RawImage>(iName, pos);
			img.GetComponent<RectTransform>().sizeDelta = size;
			img.texture = tex;

			BoxCollider bCollider = img.gameObject.AddComponent<BoxCollider>();
			bCollider.size = new Vector3(size.x, size.y, 0.0f);
			QuickUIInteractiveItem item = img.gameObject.AddComponent<QuickUIInteractiveItem>();
			item._description = description;

			return img;
		}

		public virtual void CreateIconMatrix() {
			_iconsRoot = transform.CreateChild("__IconsRoot__");
			_iconsRoot.DestroyChildsImmediate();
			Vector2 iconSize = new Vector2(128.0f, 128.0f);
			Vector2 iconOffset = new Vector2(32.0f, 32.0f);
			for (int i = 0; i < 4; i++) {
				for (int j = 0; j < 4; j++) {
					Vector3 pos = new Vector3((-640.0f + iconSize.x + iconOffset.x) * 0.5f + (iconSize.x + iconOffset.x) * j, (640.0f - (iconSize.y + iconOffset.y)) * 0.5f - (iconSize.y + iconOffset.y) * i, 0.0f);
					string iconName = "Icon_" + i.ToString() + "_" + j.ToString();
					RawImage icon = CreateIcon(iconName, _iconDefaultTex, pos, iconSize, iconName);
					icon.transform.SetParent(_iconsRoot);
                    icon.transform.localRotation = Quaternion.identity;
					icon.transform.localScale = new Vector3(1,1,1);
				}
			}
		}

		protected virtual void Start() {
			_iconDefaultTex = Resources.Load<Texture>("DefaultIcon");

			foreach (Transform t in transform) {
				QuickUIMenuPage page = t.GetComponent<QuickUIMenuPage>();
				if (page) {
					_childPages.Add(page);
					page.SetVisible(false);
				}
			}
		}

		#endregion

		#region GET AND SET

        public virtual Vector3 GetCornerPosition(UICorner corner, Space space)
        {
            float signX = (corner == UICorner.TOP_RIGHT || corner == UICorner.BOTTOM_RIGHT) ? 1.0f : -1.0f;
            float signY = (corner == UICorner.TOP_LEFT || corner == UICorner.TOP_RIGHT) ? 1.0f : -1.0f;
            Vector3 localPos = new Vector3(signX * _width, signY * _height, 0.0f) * 0.5f;

            return (space == Space.Self)? localPos : transform.TransformPoint(localPos);
        }

		public virtual void AddChildPage() {
			GetComponentInParent<QuickUIMenu>().CreatePage<QuickUIMenuPage>("Page" + _childPages.Count.ToString(), transform);
		}

		public virtual void RemoveChildPage() {
			QuickUIMenuPage lastChildPage = null;
			foreach (Transform t in transform) {
				QuickUIMenuPage p = t.GetComponent<QuickUIMenuPage>();
				if (p) lastChildPage = p;
			}
			if (lastChildPage) DestroyImmediate(lastChildPage.gameObject);
		}

		public virtual QuickUIMenuPage GetChildPage(int id) {
			return (id >= _childPages.Count)? null : _childPages[id];
		}

		public virtual QuickUIInteractiveItem[] GetIcons() {
			return _iconsRoot.GetComponentsInChildren<QuickUIInteractiveItem>(true);
		}

		public virtual QuickUIInteractiveItem GetIconSelected() {
            if (!_headTracking) _headTracking = FindObjectOfType<QuickHeadTracking>();

            QuickUIInteractiveItem result = null;
            if (_headTracking) 
            {
                List<VRCursorType> cursors = QuickUtils.GetEnumValues<VRCursorType>();
                for (int i = 0; !result && (i < cursors.Count); i++)
                {
                    QuickUICursor c = _headTracking.GetVRCursor(cursors[i]);
                    if (c && c.gameObject.activeInHierarchy) result = c.GetCurrentInteractible();
                }
            }

            return result;
		}

        public virtual int GetIconSelectedID()
        {
            QuickUIInteractiveItem itemSelected = GetIconSelected();
            if (itemSelected)
            {
                for (int i = 0; i < _iconsRoot.childCount; i++)
                {
                    if (_iconsRoot.GetChild(i) == itemSelected.transform) return i;
                }
            }

            return -1;
        }

		public virtual MaskableGraphic GetElement(UIElement e) {
			return transform.Find(e.ToString()).GetComponent<MaskableGraphic>();
		}

		public virtual Text GetElementText(UIElement e) {
			return (Text)GetElement(e);
		}

		public virtual RawImage GetElementRawImage(UIElement e) {
			return (RawImage)GetElement(e);
		}

		public virtual void SetActiveElement(UIElement e, bool v) {
			GetElement(e).gameObject.SetActive(v); 
		}

		public virtual void SetVisible(bool v) {
			foreach (Transform t in transform) {
				if (!t.GetComponent<QuickUIMenuPage>()) t.gameObject.SetActive(v);
			}

			foreach (Transform t in _iconsRoot) {
				t.gameObject.SetActive(v && !IsDefaultIcon(t.GetComponent<RawImage>()));
			}

            foreach (Collider c in _colliders)
            {
                c.enabled = v;
            }
		}

		protected virtual bool IsDefaultIcon(RawImage icon) {
			return icon.texture == _iconDefaultTex;
		}

		#endregion

		#region UPDATE

		protected virtual void Update() {
			QuickUIInteractiveItem icon = GetIconSelected();
			_textDescription.text = icon? icon._description : "";
		}

		#endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            float r = 0.025f;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(GetCornerPosition(UICorner.TOP_LEFT, Space.World), r);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(GetCornerPosition(UICorner.TOP_RIGHT, Space.World), r);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(GetCornerPosition(UICorner.BOTTOM_LEFT, Space.World), r);

            Gizmos.color = Color.white;
            Gizmos.DrawSphere(GetCornerPosition(UICorner.BOTTOM_RIGHT, Space.World), r);
        }

        #endregion

    }

}
