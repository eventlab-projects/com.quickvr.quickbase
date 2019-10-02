
using UnityEngine;
using UnityEngine.Rendering;

using System.Collections;
using System.Collections.Generic;

using QuickVR;
using System;

namespace QuickVR
{
    [ExecuteInEditMode] // Make mirror live-update even when not in play mode
    public class QuickMirrorReflection_v2 : QuickMirrorReflectionBase
    {

        #region PUBLIC ATTRIBUTES

        public bool _useRenderWithShader = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Shader _replacementShader = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            _replacementShader = Shader.Find(GetShaderReplacementName());
        }

        protected virtual CommandBuffer CreateInvertCullingCommandBuffer(bool invertCulling)
        {
            CommandBuffer cBuffer = new CommandBuffer();
            cBuffer.SetInvertCulling(invertCulling);
            cBuffer.name = "Invert Culling = " + invertCulling.ToString();

            return cBuffer;
        }

        #endregion

        #region GET AND SET

        protected override string GetShaderName()
        {
            return "QuickVR/MirrorReflection_v2";
        }

        protected virtual string GetShaderReplacementName()
        {
            return "QuickVR/QuickMirrorClipPlane";
        }

        #endregion

        #region UPDATE

        protected override void UpdateCameraModes()
        {
            base.UpdateCameraModes();

            _reflectionCamera.RemoveAllCommandBuffers();
            CommandBuffer invertCullingON = CreateInvertCullingCommandBuffer(true);
            _reflectionCamera.AddCommandBuffer(CameraEvent.BeforeGBuffer, invertCullingON);
            _reflectionCamera.AddCommandBuffer(CameraEvent.BeforeDepthTexture, invertCullingON);
            _reflectionCamera.AddCommandBuffer(CameraEvent.AfterEverything, CreateInvertCullingCommandBuffer(false));
        }

        protected override void RenderReflection()
        {
            base.RenderReflection();

            Material mat = GetMaterial();
            Camera cam = _currentCamera;
            if (cam.stereoEnabled)
            {
                mat.SetMatrix("_mvpEyeLeft", cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left) * cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left) * transform.localToWorldMatrix);
            }
            else
            {
                mat.SetMatrix("_mvpEyeLeft", cam.projectionMatrix * cam.worldToCameraMatrix * transform.localToWorldMatrix);
            }
            mat.SetMatrix("_mvpEyeRight", cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right) * cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right) * transform.localToWorldMatrix);
        }

        protected override void RenderVirtualImage(RenderTexture targetTexture, Camera.StereoscopicEye eye, float stereoSeparation = 0)
        {
            //1) Compute ModelViewMatrix
            // find out the reflection plane: position and normal in world space
            Vector3 pos = transform.position;
            Vector3 normal = GetNormal();

            // Reflect camera around reflection plane
            float d = -Vector3.Dot(normal, pos);
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);
            Matrix4x4 reflection = CalculateReflectionMatrix(reflectionPlane);
            _reflectionCamera.worldToCameraMatrix = (_currentCamera.stereoEnabled) ? _currentCamera.GetStereoViewMatrix(eye) : _currentCamera.worldToCameraMatrix;
            _reflectionCamera.worldToCameraMatrix *= reflection;

            //2) Compute ProjectionMatrix
            // Setup oblique projection matrix so that near plane is our reflection
            // plane. This way we clip everything below/above it for free.
            _reflectionCamera.projectionMatrix = (_currentCamera.stereoEnabled) ? _currentCamera.GetStereoProjectionMatrix(eye) : _currentCamera.projectionMatrix;
            _reflectionCamera.targetTexture = targetTexture;

            if (_useRenderWithShader)
            {
                Shader.SetGlobalVector("MIRROR_PLANE_POS", transform.position);
                Shader.SetGlobalVector("MIRROR_PLANE_NORMAL", normal);
                _reflectionCamera.RenderWithShader(_replacementShader, "RenderType");
            }
            else
            {
                Vector4 clipPlane = CameraSpacePlane(_reflectionCamera.worldToCameraMatrix, pos, normal, 1.0f);
                _reflectionCamera.projectionMatrix = MakeProjectionMatrixOblique(_reflectionCamera.projectionMatrix, clipPlane);
                _reflectionCamera.Render();
            }
        }

        //protected override void RenderVirtualImage(RenderTexture targetTexture, Camera.StereoscopicEye eye, float stereoSeparation = 0)
        //{
        //    Vector3 p = _currentCamera.transform.position;
        //    Vector3 v = p - transform.position;
        //    Vector3 normal = GetNormal();
        //    Quaternion q = new Quaternion(normal.x, normal.y, normal.z, 0);

        //    //_reflectionCamera.transform.position = _currentCamera.transform.position;
        //    //_reflectionCamera.transform.rotation = _currentCamera.transform.rotation;

        //    _reflectionCamera.transform.position = p - 2.0f * normal * Vector3.Dot(v, normal); 
        //    _reflectionCamera.transform.rotation = _currentCamera.transform.rotation;
        //    _reflectionCamera.transform.Rotate(transform.up, 180.0f, Space.World);
        //    _reflectionCamera.projectionMatrix = (_currentCamera.stereoEnabled) ? _currentCamera.GetStereoProjectionMatrix(eye) : _currentCamera.projectionMatrix;
        //    Vector4 clipPlane = CameraSpacePlane(_reflectionCamera.worldToCameraMatrix, transform.position, normal, 1.0f);
        //    _reflectionCamera.projectionMatrix = MakeProjectionMatrixOblique(_reflectionCamera.projectionMatrix, clipPlane);

        //    _reflectionCamera.targetTexture = targetTexture;
        //    _reflectionCamera.Render();
        //}

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 cpos = worldToCameraMatrix.MultiplyPoint(pos);
            Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        private static Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
        {
            Matrix4x4 reflectionMat = Matrix4x4.zero;

            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;

            return reflectionMat;
        }

        // taken from http://www.terathon.com/code/oblique.html
        private static Matrix4x4 MakeProjectionMatrixOblique(Matrix4x4 matrix, Vector4 clipPlane)
        {
            Vector4 q;

            // Calculate the clip-space corner point opposite the clipping plane
            // as (sgn(clipPlane.x), sgn(clipPlane.y), 1, 1) and
            // transform it into camera space by multiplying it
            // by the inverse of the projection matrix

            q.x = (Mathf.Sign(clipPlane.x) + matrix[8]) / matrix[0];
            q.y = (Mathf.Sign(clipPlane.y) + matrix[9]) / matrix[5];
            q.z = -1.0F;
            q.w = (1.0F + matrix[10]) / matrix[14];

            // Calculate the scaled plane vector
            Vector4 c = clipPlane * (2.0F / Vector3.Dot(clipPlane, q));

            // Replace the third row of the projection matrix
            matrix[2] = c.x;
            matrix[6] = c.y;
            matrix[10] = c.z + 1.0F;
            matrix[14] = c.w;

            return matrix;
        }

        #endregion

    }
}

