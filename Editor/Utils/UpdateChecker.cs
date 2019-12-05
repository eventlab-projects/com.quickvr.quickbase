//#define DEVELOPMENT
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;
using UnityEditor;
using System;
using System.Collections;

namespace AltProg.CleanEmptyDir
{
    public static class UpdateChecker
    {
        const string VERSION = "1.0";

        public class Message
        {
            public string Msg;
            public string Link;
        }

        // Action: message, button text, button link
        public static Action<Message> OnDone;
        static UnityWebRequest www;

        static UpdateChecker()
        {
            www = null;
        }

        // Action: message
        public static void Check()
        {
            // No duplicated request
            if (www != null)
                return;

            www = new UnityWebRequest("http://update.altprog.com/CleanEmptyDir.json");

            EditorApplication.update += EditorApplication_Update;
        }

        static void EditorApplication_Update()
        {
            if (www.isDone)
            {
                EditorApplication.update -= EditorApplication_Update;

                if (string.IsNullOrEmpty(www.error))
                {
                    if (null != OnDone)
                    {
                        OnDone( ParseMessage(www.downloadHandler.text) );
                    }
                } else
                {
                    #if DEVELOPMENT
                    Debug.LogError(www.error);
                    #endif
                }

                www = null;
            }
        }

        static Message ParseMessage(string fullContents)
        {
            /* Format:
            {
                "last_ver" : "1.0",
                "last_ver_msg" : ["New Update 1.0", "Learn More", "http://altprog.com/unity-asset"];
                "msgs" : [ 
                        ["msg 1", "btn 1", "link1"],
                        ["msg 2", "btn 2", "link1"]]
            }
             */

            try
            {
                var json = SimpleJSON.JSON.Parse(fullContents);

                if ( (string)json ["last_ver"] != VERSION)
                    return ToMessage(json ["last_ver_msg"].AsArray);

                var msgs = json ["msgs"];
                return ToMessage( msgs [Random.Range(0, msgs.Count - 1)].AsArray );

            } 
            #if DEVELOPMENT
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            #else
            catch (Exception)
            {
            }
            #endif

            return null;
        }

        static Message ToMessage( SimpleJSON.JSONArray  msg )
        {
            var msgObj = new Message();
            msgObj.Msg = msg[0];
            msgObj.Link = msg[1];
            return msgObj;
        }
    }

}