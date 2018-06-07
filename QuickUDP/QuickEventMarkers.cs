using UnityEngine;
using System.Collections;
using QuickVR;

namespace QuickVR
{

    public class QuickEventMarkers : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public string _ip = "161.116.27.150";
        public int _port = 8000;
        public bool _debug = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickUDPSend _udpSend;

        #endregion

        #region CREATION AND DESTRUCTION

        public virtual void Awake()
        {
            if (_ip.Length == 0) _ip = "161.116.27.150";
            if (_port == 0) _port = 8000;
            
            _udpSend = gameObject.GetOrCreateComponent<QuickUDPSend>();
            _udpSend._StringIP = _ip;
            _udpSend._Port = _port;
            _udpSend.Init();

            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {
            if (_debug)
            {
                if (Input.GetKey(KeyCode.Space))
                {
                    _udpSend.SendString("1_1_\n");
                    Debug.Log("sent event test");
                }
            }
        }

        public virtual void SendEventMarker(int code, int channel = 1)
        {
            string sEventCode = channel.ToString() + "_" + code.ToString() + "_\n";
            _udpSend.SendString(sEventCode);
            Debug.Log("EventMarkers: sent event with code " + code.ToString());
        }

        #endregion
        
        //example for fixed events
        //public void SendFadeOutMarker()
        //{
        //    if (!bEvent5Sent)
        //    {
        //        _udpSend.SendString("1_5_\n");
        //        bEvent5Sent = true;
        //        Debug.Log("EventMarkers: sent event Fade Out");
        //    }
        //}

    }

}
