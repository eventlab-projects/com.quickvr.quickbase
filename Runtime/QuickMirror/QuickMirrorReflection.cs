using UnityEngine;
using UnityEngine.XR;

using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR
{

    /// <summary>
    /// Mirror Reflection Script for planar surfaces with dynamic shadows support. 
    /// \author Ramon Oliva
    /// </summary>

    [ExecuteInEditMode] // Make mirror live-update even when not in play mode
    public class QuickMirrorReflection : QuickMirrorReflectionBase
    {

        #region PROTECTED PARAMETERS

        protected enum Corner
        {
            TOP_LEFT,
            TOP_RIGHT,
            BOTTOM_LEFT,
            BOTTOM_RIGHT,
        };

        #endregion

        #region GET AND SET

        protected override string GetShaderName()
        {
            return "QuickVR/MirrorReflection";
        }

        protected virtual Vector3 GetCornerPosition(Corner corner)
        {
            Bounds bounds = _mFilter.sharedMesh.bounds;
            Vector3 result = bounds.center;
            Vector3 halfSize = bounds.extents * _reflectionScale;

            if (corner == Corner.BOTTOM_LEFT)
            {
                result += Vector3.Scale(halfSize, new Vector3(-1, -1, 0));
            }
            else if (corner == Corner.TOP_LEFT)
            {
                result += Vector3.Scale(halfSize, new Vector3(-1, 1, 0));
            }
            else if (corner == Corner.BOTTOM_RIGHT)
            {
                result += Vector3.Scale(halfSize, new Vector3(1, -1, 0));
            }
            else
            {
                result += Vector3.Scale(halfSize, new Vector3(1, 1, 0));
            }

            return transform.TransformPoint(result);
        }

        protected virtual Dictionary<Corner, Vector3> GetCameraCornerRays(Camera cam, float d)
        {
            Vector3[] corners = new Vector3[4];
            cam.CalculateFrustumCorners(cam.rect, d, Camera.MonoOrStereoscopicEye.Mono, corners);
            Dictionary<Corner, Vector3> result = new Dictionary<Corner, Vector3>();

            result[Corner.BOTTOM_LEFT] = cam.transform.TransformVector(corners[0]);
            result[Corner.TOP_LEFT] = cam.transform.TransformVector(corners[1]);
            result[Corner.TOP_RIGHT] = cam.transform.TransformVector(corners[2]);
            result[Corner.BOTTOM_RIGHT] = cam.transform.TransformVector(corners[3]);

            return result;
        }

#endregion

#region MIRROR RENDER

        protected override void RenderVirtualImage(Camera.StereoscopicEye eye, float stereoSeparation = 0.0f)
        {
            //Debug.Log("MATRICES");
            //Debug.Log(_currentCamera.transform.worldToLocalMatrix.ToString("f3"));
            //Debug.Log(_currentCamera.worldToCameraMatrix.ToString("f3"));
            //Debug.Log(_currentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).ToString("f3"));
            //Debug.Log(_currentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).ToString("f3"));

            //Setup the projection and worldView matrices as explained in:
            //http://csc.lsu.edu/~kooima/pdfs/gen-perspective.pdf 

            Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
            Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
            Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);

            Vector3 pe = GetReflectedPosition(_currentCamera.transform.position + _currentCamera.transform.right * stereoSeparation); // eye position

            Vector3 va = pa - pe;
            Vector3 vb = pb - pe;
            Vector3 vc = pc - pe;
            Vector3 vr = transform.right;       // right axis of screen
            Vector3 vu = transform.up;          // up axis of screen
            Vector3 vn = -transform.forward;    // normal vector of screen

            //Adjust the near and far clipping planes of the reflection camera. 
            Vector3 v = pe - transform.position;
            Vector3 projectedPoint = pe - Vector3.Project(v, vn);
            float n = Mathf.Max(_currentCamera.nearClipPlane, Vector3.Distance(pe, projectedPoint));
            float f = Mathf.Max(n, _currentCamera.farClipPlane);

            float d = -Vector3.Dot(va, vn);			// distance from eye to screen 
            float l = Vector3.Dot(vr, va) * n / d;  // distance to left screen edge
            float r = Vector3.Dot(vr, vb) * n / d;  // distance to right screen edge
            float b = Vector3.Dot(vu, va) * n / d;  // distance to bottom screen edge
            float t = Vector3.Dot(vu, vc) * n / d;  // distance to top screen edge

            //Projection matrix
            Matrix4x4 p = new Matrix4x4();
            p.SetRow(0, new Vector4(2.0f * n / (r - l), 0.0f, (r + l) / (r - l), 0.0f));
            p.SetRow(1, new Vector4(0.0f, 2.0f * n / (t - b), (t + b) / (t - b), 0.0f));
            p.SetRow(2, new Vector4(0.0f, 0.0f, (f + n) / (n - f), 2.0f * f * n / (n - f)));
            p.SetRow(3, new Vector4(0.0f, 0.0f, -1.0f, 0.0f));

            //Rotation matrix
            Matrix4x4 rm = Matrix4x4.identity;
            rm.SetRow(0, new Vector4(vr.x, vr.y, vr.z, 0.0f));
            rm.SetRow(1, new Vector4(vu.x, vu.y, vu.z, 0.0f));
            rm.SetRow(2, new Vector4(vn.x, vn.y, vn.z, 0.0f));

            //Translation matrix
            Matrix4x4 tm = Matrix4x4.identity;
            tm.SetColumn(3, new Vector4(-pe.x, -pe.y, -pe.z, 1.0f));

            // set matrices
            _reflectionCamera.projectionMatrix = p;
            _reflectionCamera.worldToCameraMatrix = rm * tm;

            // The original paper puts everything into the projection 
            // matrix (i.e. sets it to p * rm * tm and the other 
            // matrix to the identity), but this doesn't appear to 
            // work with Unity's shadow maps.
            _reflectionCamera.Render();
        }

#endregion

#region DEBUG

        //protected override void OnDrawGizmos()
        //{
        //    float r = 0.05f;

        //    Gizmos.color = Color.blue;
        //    Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
        //    Gizmos.DrawSphere(pa, r);

        //    Gizmos.color = Color.yellow;
        //    Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
        //    Gizmos.DrawSphere(pb, r);

        //    Gizmos.color = Color.red;
        //    Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);
        //    Gizmos.DrawSphere(pc, r);

        //    Gizmos.color = Color.green;
        //    Vector3 pd = GetCornerPosition(Corner.TOP_RIGHT);
        //    Gizmos.DrawSphere(pd, r);

        //    DebugExtension.DrawCoordinatesSystem(transform.position, transform.right, transform.up, transform.forward);
        //}

        protected virtual void DrawReflectionCamera(Camera reflectionCamera)
        {
            reflectionCamera.ResetProjectionMatrix();
            float s = 0.05f;
            Vector3 pe = reflectionCamera.transform.position; // eye position

            Gizmos.color = Color.grey;
            Gizmos.DrawCube(reflectionCamera.transform.position, new Vector3(s, s, s));

            Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
            Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
            Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);

            Vector3 va = pa - pe;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pe, va);

            Vector3 vb = pb - pe;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pe, vb);

            Vector3 vc = pc - pe;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pe, vc);

            Dictionary<Corner, Vector3> camCornerRays = GetCameraCornerRays(reflectionCamera, reflectionCamera.farClipPlane);
            Plane plane = new Plane(pc, pb, pa);
            float rayDistance;
            Vector3 tl = Vector3.zero;
            Vector3 tr = Vector3.zero;
            Vector3 bl = Vector3.zero;
            Vector3 br = Vector3.zero;

            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.TOP_LEFT]), out rayDistance))
            {
                tl = pe + camCornerRays[Corner.TOP_LEFT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.TOP_RIGHT]), out rayDistance))
            {
                tr = pe + camCornerRays[Corner.TOP_RIGHT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.BOTTOM_RIGHT]), out rayDistance))
            {
                br = pe + camCornerRays[Corner.BOTTOM_RIGHT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.BOTTOM_LEFT]), out rayDistance))
            {
                bl = pe + camCornerRays[Corner.BOTTOM_LEFT].normalized * rayDistance;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }

#endregion
    }

}