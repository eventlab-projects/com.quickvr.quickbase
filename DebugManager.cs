using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using QuickVR;

public class DebugManager : MonoBehaviour
{

    #region PROTECTED PARAMETERS

    string _message = "";
    protected Color _textColor = Color.white;

    #endregion

    #region GET AND SET

    public virtual string GetLastLogMessage()
    {
        return _message;
    }

    public virtual void Log(string message)
    {        
        Log(message, Color.white);
    }

    public virtual void Log(string message, Color textColor)
    {
        _message = message;
        ParseIncodeKeys();
        _textColor = textColor;
        Debug.Log(message);
    }

    protected virtual void ParseIncodeKeys()
    {
        Dictionary<string, string> keyMap = new Dictionary<string, string>();
        for (int i = 0; i < _message.Length; i++)
        {
            if (_message[i] == '$')
            {
                int keyIndexStart = i + 2;
                while (_message[i] != ')') i++;
                int keyIndexEnd = i;
                string virtualButton = _message.Substring(keyIndexStart, keyIndexEnd - keyIndexStart);
                string keyName = GetKeyName(virtualButton);
                keyMap[virtualButton] = keyName;
            }
        }
        foreach (var pair in keyMap) _message = _message.Replace("$(" + pair.Key + ")", pair.Value);
    }

    public virtual void Clear()
    {
        _message = "";
    }

    public static string GetKeyName(string virtualButton)
    {
        return "[" + virtualButton + "]";
    }

    #endregion

    #region UPDATE

    protected virtual void OnGUI()
    {
        GUI.color = _textColor;
        GUI.Label(new Rect(0, Screen.height - 25, 1024, 25), _message);
    }

    #endregion

}
