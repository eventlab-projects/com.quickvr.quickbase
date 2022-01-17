using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{

    public class QuickXRRig : XRRig
    {

        protected new void Awake()
        {

        }

        protected new void Start()
        {
            Camera cam = QuickVRCameraController.GetCamera();
            cameraGameObject = cam.gameObject;
            cameraFloorOffsetObject = cam.transform.parent.gameObject;

            if (!rig)
            {
                rig = gameObject;
            }
        }

        protected override void OnDrawGizmos()
        {
            if (rig != null)
            {
                // Draw XR Rig box
                Gizmos.color = Color.green;
                GizmoHelpers.DrawWireCubeOriented(rig.transform.position, rig.transform.rotation, 3f);
                GizmoHelpers.DrawAxisArrows(rig.transform, 0.5f);
            }

            if (cameraFloorOffsetObject != null)
            {
                GizmoHelpers.DrawAxisArrows(cameraFloorOffsetObject.transform, 0.5f);
            }

            if (rig != null)
            {
                var cameraPosition = rig.transform.position;
                Gizmos.color = Color.red;
                GizmoHelpers.DrawWireCubeOriented(cameraPosition, rig.transform.rotation, 0.1f);
                GizmoHelpers.DrawAxisArrows(rig.transform, 0.5f);

                if (rig != null)
                {
                    var floorPos = cameraPosition;
                    floorPos.y = rig.transform.position.y;
                    Gizmos.DrawLine(floorPos, cameraPosition);
                }
            }
        }

    }

}


