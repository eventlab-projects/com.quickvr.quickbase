//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace QuickVR {

//	public class InputManagerMenu : BaseInputManager {

//		#region PUBLIC PARAMETERS

//		public QuickUIMenu _menu = null;

//		#endregion

//		#region GET AND SET

//        public override string[] GetButtonCodes()
//        {
//            List<string> bCodes = new List<string>();
//            bCodes.Add(BaseInputManager.NULL_MAPPING);

//            if (_menu)
//            {
//                bCodes.Add("IconPrev");
//                bCodes.Add("IconNext");

//                QuickUIMenuPage[] pages = _menu.GetComponentsInChildren<QuickUIMenuPage>(true);
//                foreach (QuickUIMenuPage p in pages)
//                {
//                    QuickUIInteractiveItem[] icons = p.GetIcons();
//                    foreach (QuickUIInteractiveItem ic in icons)
//                    {
//                        bCodes.Add(GetIconUniqueName(ic));
//                    }
//                }
//            }

//            return bCodes.ToArray();
//        }

//		public virtual string GetIconUniqueName(QuickUIInteractiveItem icon) {
//			if (!icon) return "";

//			if ((icon.name == "IconPrev") || (icon.name == "IconNext")) return icon.name;

//			string prefix = "";
//			QuickUIMenuPage page = icon.GetComponentInParent<QuickUIMenuPage>();
//			while (page) {
//				prefix = page.name + "_" + prefix;
//				page = page.transform.parent.GetComponent<QuickUIMenuPage>();
//			}
//			return (name == "")? "" : prefix + icon.name;
//		}

//		#endregion

//		#region INPUT MANAGEMENT

//		protected override float ImpGetAxis(string axis) {
//			return 0.0f;
//		}

//		protected override bool ImpGetButton(string button) {
//			if (!_menu) return false;

//			QuickUIInteractiveItem selectedIcon = _menu.GetIconSelected();
//            if (!selectedIcon || !selectedIcon.IsDown()) return false;

//            return button == GetIconUniqueName(selectedIcon);
//		}

//		#endregion

//	}

//}
