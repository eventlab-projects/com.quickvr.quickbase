using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickLODManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            LODGroup lod = gameObject.GetOrCreateComponent<LODGroup>();
            if (Application.isMobilePlatform)
            {
                lod.ForceLOD(lod.lodCount - 1);
            }
        }

    }

}


