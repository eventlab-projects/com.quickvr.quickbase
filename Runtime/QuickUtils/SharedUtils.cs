using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This file contains common classes and extension methods used accross different QuickVR packages. 
//Ideally, there should be a package QuickVR.Dependencies.Base and set all those common code on that 
//github. However, Unity packages does not currently supports defining git dependencies at the package
//level (i.e., on the package.json file), only on the manifest.json file of the project. 

//In order to simplify the installation process for the final user, this shared file will be copied 
//(and properly renamed its namespace) across all the packages needing these features, until Unity
//finally introduces package dependencies from git. 

//src: https://github.com/eventlab-projects/com.quickvr.quickbase/blob/master/Runtime/QuickUtils/SharedUtils.cs

namespace QuickVR
{

    public class CustomAsyncOperation<T> : CustomAsyncOperation
    {

        #region PUBLIC ATTRIBUTES

        public T _result
        {
            get
            {
                return m_Result;
            }

            set
            {
                m_Result = value;
                _isDone = true;
            }
        }
        protected T m_Result;

        #endregion

    }

    public class CustomAsyncOperation : CustomYieldInstruction
    {

        #region PUBLIC ATTRIBUTES

        public bool _isDone
        {
            get; set;
        }

        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        #endregion

    }

}


