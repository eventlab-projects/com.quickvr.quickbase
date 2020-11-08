using UnityEngine;
using QuickVR;

public static class SettingsTestVR
{


    #region GET AND SET
    
    public static bool GetIsCube()
    {
        return QuickPlayerPrefs.GetBool("IsCube");
    }
    
    public static void SetIsCube(bool value)
    {
        QuickPlayerPrefs.SetValue("IsCube", value);
    }
    
    #endregion

}
