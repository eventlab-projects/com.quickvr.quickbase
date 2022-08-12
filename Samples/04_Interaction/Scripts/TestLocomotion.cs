using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR.SampleInteraction
{

    public class TestLocomotion : MonoBehaviour
    {

        public Animator _targetAnimator1 = null;
        public Animator _targetAnimator2 = null;

        // Start is called before the first frame update
        //protected virtual IEnumerator Start()
        //{
        //    yield return new WaitForSeconds(0.5f);

        //    SetTargetAvatar();
        //}


        [ButtonMethod]
        public virtual void SetTargetAvatar1()
        {
            QuickSingletonManager.GetInstance<QuickVRManager>().SetAnimatorTarget(_targetAnimator1);
        }

        [ButtonMethod]
        public virtual void SetTargetAvatar2()
        {
            QuickSingletonManager.GetInstance<QuickVRManager>().SetAnimatorTarget(_targetAnimator2);
        }
    }

}


