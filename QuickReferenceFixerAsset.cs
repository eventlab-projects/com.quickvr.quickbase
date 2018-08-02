using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickReferenceFixerAsset : ScriptableObject {

    #region PUBLIC ATTRIBUTES

    [System.Serializable]
    public class ClassData
    {
        public string _tNamespace = "";
        public string _tName = "";
        public string _quickvrModule = "";

        public ClassData(string tNamespace, string tName, string quickvrModule)
        {
            _tNamespace = tNamespace;
            _tName = tName;
            _quickvrModule = quickvrModule;
        }
    }

    #endregion

    #region PROTECTED ATTRIBUTES

    [SerializeField]
    public List<string> _keys = new List<string>();

    [SerializeField]
    public List<ClassData> _values = new List<ClassData>();

    #endregion

    #region GET AND SET

    public virtual void Clear()
    {
        _keys.Clear();
        _values.Clear();
    }

    public virtual void Add(string key, ClassData data)
    {
        _keys.Add(key);
        _values.Add(data);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif

    }

    #endregion

}
