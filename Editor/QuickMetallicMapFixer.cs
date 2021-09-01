using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.IO;

namespace QuickVR
{
    public class QuickMetallicMapFixer : EditorWindow
    {

        #region PROTECTED ATTRIBUTES

        protected Texture2D _texture = null;

        #endregion

        #region UPDATE

        protected virtual void OnGUI()
        {
            _texture = EditorGUILayout.ObjectField("Texture", _texture, typeof(Texture2D), true) as Texture2D;

            if (_texture && GUILayout.Button("Fix"))
            {
                Fix();
            }
        }

        protected virtual void Fix()
        {
            Texture2D result = new Texture2D(_texture.width, _texture.height);
            for (int mip = 0; mip < _texture.mipmapCount; ++mip)
            {
                Color[] cols = _texture.GetPixels(mip);
                for (int i = 0; i < cols.Length; ++i)
                {
                    cols[i] = new Color(cols[i].r, cols[i].g, cols[i].b, 1.0f - cols[i].a);
                }
                result.SetPixels(cols, mip);
            }
            result.Apply(false);

            string pathSrc = AssetDatabase.GetAssetPath(_texture);
            string pathDst = Path.GetDirectoryName(pathSrc) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(pathSrc) + "_FIXED" + Path.GetExtension(pathSrc);
            string extension = Path.GetExtension(pathSrc).ToLower();

            byte[] bytes;
            if (extension == ".png") bytes = result.EncodeToPNG();
            else if (extension == ".jpg") bytes = result.EncodeToJPG();
            else if (extension == ".tga") bytes = result.EncodeToTGA();
            else if (extension == ".exr") bytes = result.EncodeToEXR();
            else return;

            string filePath = Path.Combine(Directory.GetCurrentDirectory(), pathSrc);
            filePath.Replace("/", "\\");

            FileStream fStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            BinaryWriter writer = new BinaryWriter(fStream);
            writer.Write(bytes);
            writer.Close();
            fStream.Close();

            AssetDatabase.ImportAsset(pathSrc);
            AssetDatabase.Refresh();

            //Debug.Log(Directory.GetCurrentDirectory());
        }

        #endregion

    }

}
