using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class QuickOVRHandBonePhysics : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected SphereCollider _collider = null;
        protected Rigidbody _rigidBody = null;

        protected Transform _debugger = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _collider = transform.GetOrCreateComponent<SphereCollider>();
            _collider.radius = 0.5f;
            _collider.isTrigger = true;

            _rigidBody = transform.GetOrCreateComponent<Rigidbody>();
            _rigidBody.isKinematic = true;

            _debugger = transform.CreateChild("__Debugger__");
            _debugger.GetOrCreateComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/QuickDiffuseRed");
            _debugger.GetOrCreateComponent<MeshFilter>().mesh = QuickUtils.GetUnityPrimitiveMesh(PrimitiveType.Sphere);
        }

        #endregion

        #region GET AND SET

        public virtual void SetRadius(float radius)
        {
            _collider.radius = radius;

            float sf = radius * 2.0f;
            _debugger.transform.localScale = new Vector3(sf, sf, sf);
        }

        public virtual SphereCollider GetCollider()
        {
            return _collider;
        }

        #endregion

        #region UPDATE

#if UNITY_EDITOR
        protected virtual void Update()
        {
            _debugger.gameObject.SetActive(QuickSingletonManager.GetInstance<QuickOVRHandsInitializer>()._debug);
        }
#endif

#endregion

    }
}


