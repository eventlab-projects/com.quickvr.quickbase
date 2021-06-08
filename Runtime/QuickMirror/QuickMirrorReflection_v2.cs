
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

        protected override void RenderReflection()
        {
            base.RenderReflection();

            Material mat = GetMaterial();
            Camera cam = _currentCamera;
            Matrix4x4 refl = CalculateReflectionMatrix();
            if (cam.stereoEnabled)
            {
                mat.SetMatrix("_mvpEyeLeft", cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left) * cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left) * transform.localToWorldMatrix);
                mat.SetMatrix("_mvpEyeRight", cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right) * cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right) * transform.localToWorldMatrix);
            }
            else
            {
                mat.SetMatrix("_mvpEyeLeft", cam.projectionMatrix * cam.worldToCameraMatrix * transform.localToWorldMatrix);
            }
        }

        protected virtual void ReflectCamera(float stereoSeparation)
        {
            //Reflect camera around reflection plane
            Vector3 normal = GetNormal();
            //Vector3 camToPlane = (_currentCamera.transform.position + _currentCamera.transform.right * stereoSeparation) - transform.position;
            Vector3 camToPlane = _currentCamera.transform.position - transform.position;
            Vector3 reflectionCamToPlane = Vector3.Reflect(camToPlane, normal);
            Vector3 camPosRS = transform.position + reflectionCamToPlane;
            _reflectionCamera.transform.position = camPosRS;

            ///Compute the orientation of the reflection camera
            Vector3 reflectionFwd = Vector3.Reflect(_currentCamera.transform.forward, normal);
            Vector3 reflectionUp = Vector3.Reflect(_currentCamera.transform.up, normal);
            Quaternion q = Quaternion.LookRotation(reflectionFwd, reflectionUp);
            _reflectionCamera.transform.rotation = q;
        }

        //protected override void RenderVirtualImage(RenderTexture targetTexture, Camera.StereoscopicEye eye, float stereoSeparation = 0)
        //{
        //    ReflectCamera(stereoSeparation);

        //    Matrix4x4 eLeftViewMatrix = _currentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
        //    Matrix4x4 eRightViewMatrix = _currentCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
        //    Matrix4x4 eLeftProjMatrix = _currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        //    Matrix4x4 eRightProjMatrix = _currentCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);

        //    _reflectionCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, eLeftProjMatrix);
        //    _reflectionCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, eRightProjMatrix);
        //    _reflectionCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, eRightViewMatrix);
        //    _reflectionCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, eLeftViewMatrix);
        //    Debug.Log("MATRICES");
        //    Debug.Log(_reflectionCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left).ToString("f3"));
        //    Debug.Log(_reflectionCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right).ToString("f3"));
        //    Debug.Log(_reflectionCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left).ToString("f3"));
        //    Debug.Log(_reflectionCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right).ToString("f3"));

        //    //2) Compute ProjectionMatrix
        //    // Setup oblique projection matrix so that near plane is our reflection
        //    // plane. This way we clip everything below/above it for free.
        //    _reflectionCamera.ResetProjectionMatrix();
        //    //_reflectionCamera.projectionMatrix = (_currentCamera.stereoEnabled) ? _currentCamera.GetStereoProjectionMatrix(eye) : _currentCamera.projectionMatrix;
        //    //_reflectionCamera.projectionMatrix = _currentCamera.projectionMatrix;
        //    _reflectionCamera.targetTexture = targetTexture;

        //    if (_useRenderWithShader)
        //    {
        //        Shader.SetGlobalVector("MIRROR_PLANE_POS", transform.position);
        //        Shader.SetGlobalVector("MIRROR_PLANE_NORMAL", GetNormal());
        //        _reflectionCamera.RenderWithShader(_replacementShader, "RenderType");
        //    }
        //    else
        //    {
        //        Vector4 clipPlane = CameraSpacePlane(_reflectionCamera.worldToCameraMatrix, transform.position, GetNormal(), 1.0f);
        //        _reflectionCamera.projectionMatrix = _reflectionCamera.CalculateObliqueMatrix(clipPlane);
        //        _reflectionCamera.Render();
        //    }
        //}

        protected override void RenderVirtualImage(Camera.StereoscopicEye eye, float stereoSeparation = 0)
        {
            GL.invertCulling = true;

            //1) Compute worldToCamera Matrix
            _reflectionCamera.worldToCameraMatrix = _currentCamera.stereoEnabled ? _currentCamera.GetStereoViewMatrix(eye) : _currentCamera.worldToCameraMatrix;
            _reflectionCamera.worldToCameraMatrix *= CalculateReflectionMatrix();

            //2) Compute the projection matrix
            _reflectionCamera.projectionMatrix = (_currentCamera.stereoEnabled) ? _currentCamera.GetStereoProjectionMatrix(eye) : _currentCamera.projectionMatrix;

            //3) Do the render
#if UNITY_ANDROID && UNITY_EDITOR
            Shader.SetGlobalInt(QuickMirrorReflectionManager.REFLECTION_INVERT_Y, Application.isPlaying ? 0 : 1);
#else
            Shader.SetGlobalInt(QuickMirrorReflectionManager.REFLECTION_INVERT_Y, Application.isMobilePlatform ? 0 : 1);
#endif

            if (_useRenderWithShader)
            {
                Shader.SetGlobalVector("MIRROR_PLANE_POS", transform.position);
                Shader.SetGlobalVector("MIRROR_PLANE_NORMAL", GetNormal());
                _reflectionCamera.RenderWithShader(_replacementShader, "RenderType");
            }
            else
            {
                Vector4 clipPlane = CameraSpacePlane(_reflectionCamera.worldToCameraMatrix, transform.position, GetNormal(), 1.0f);
                _reflectionCamera.projectionMatrix = _reflectionCamera.CalculateObliqueMatrix(clipPlane);
                _reflectionCamera.Render();
            }

            GL.invertCulling = false;
        }

        // Given position/normal of the plane, calculates plane in camera space.
        private Vector4 CameraSpacePlane(Matrix4x4 worldToCameraMatrix, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 cpos = worldToCameraMatrix.MultiplyPoint(pos);
            Vector3 cnormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        // Calculates reflection matrix around the given plane
        protected virtual Matrix4x4 CalculateReflectionMatrix()
        {
            Vector3 normal = GetNormal();
            float d = -Vector3.Dot(normal, transform.position);
            Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);

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

        #endregion

    }
}

