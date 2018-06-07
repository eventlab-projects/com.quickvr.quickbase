using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;

namespace QuickVR
{

    public static class QuickXMLExtensions
    {

        public static void SetValue(this XmlElement element, object value)
        {
            element.AppendChild(element.OwnerDocument.CreateTextNode(value.ToString()));
        }

        public static XmlElement AddElement(this XmlElement element, string name)
        {
            XmlElement newElement = element.OwnerDocument.CreateElement(name);
            element.AppendChild(newElement);

            return newElement;
        }

        public static XmlElement AddElement(this XmlDocument document, string name)
        {
            XmlElement root = null;
            foreach (XmlNode n in document.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    root = (XmlElement)n;
                    break;
                }
            }

            if (root == null)
            {
                root = document.CreateElement("Root");
                document.AppendChild(root);
            }

            return root.AddElement(name);
        }

    }

}
