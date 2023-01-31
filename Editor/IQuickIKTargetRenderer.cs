using UnityEngine;

namespace QuickVR
{
    public interface IQuickIKTargetRenderer
    {
        Transform transform { get; }
        bool visible { get; set; }

        void OnSceneGUI();
    }
}
