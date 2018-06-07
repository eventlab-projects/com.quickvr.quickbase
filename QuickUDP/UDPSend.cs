/*
	UDPSend Class allows a one-way connection to a given port for a given IP for sending messages through UDP.
	
	-To use it, create an object of the UDPSend class and initialise it with the following command:
		
		-->udpSend= ScriptableObject.CreateInstance("UDPSend") as UDPSend;
	
	-In the Start or the Awake function of your MonoBehaviour script (in which you are going to do the send call)
	call the init() function
	- Close the connection at onApplicationQuit.
	  
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace QuickVR
{

    public class QuickUDPSend : MonoBehaviour
    {

        #region PUBLIC PARAMETERS
        public string _StringIP = "127.0.0.1";
        public int _Port = 8000;
        #endregion

        #region PRIVATE PARAMETERS
        private IPEndPoint remoteEndPoint;
        private UdpClient client;
        #endregion

        #region CREATION AND DESTRUCTION
        public void Init()
        {
            // ----------------------------
            // Set the IP endPoint and establish the connection
            // ----------------------------
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(_StringIP), _Port);
            client = new UdpClient();

            try
            {
                client.Connect(remoteEndPoint);
            }
            catch (Exception e)//error
            {
                Debug.LogError("UDPSend: Error connecting UdpClient: " + e.Message);
            }

        }

        public void OnApplicationQuit()
        {
            client.Close();
        }
        #endregion

        #region UPDATE
        /// <summary>
        /// Sends additional channels to the matlab model to be stored in a mat file with the physio data. 
        /// (E.g. tracking data: x, y, z coordinates of two right and left arm trackers).
        /// </summary>
        /// <param name='data'>
        /// Data.
        /// </param>
        public void SendAdditionalData(float[] data)
        {
            string message = "";
            int index = (int)'2';
            for (int i = 0; i < data.Length; i++)
            {
                message += (char)index + "_" + data[i] + "_";
                index += 1;
            }
            SendString(message + "\n");
        }

        /// <summary>
        /// Creates the message from data. This is it returns a string "1_data_\n"
        /// </summary>
        /// <returns>
        /// The message correctly formated to be send at channel 1
        /// </returns>
        /// <param name='data'>
        /// Data.
        /// </param>
        public string CreateMessageFromData(float data)
        {
            return CreateMessageFromData(data, 1);
        }

        /// <summary>
        /// Creates the message from data. This is it returns a string "channel_data_\n"
        /// </summary>
        /// <returns>
        /// The message correctly formated to be send at the given channel
        /// </returns>
        /// <param name='data'>
        /// Data.
        /// </param>
        public string CreateMessageFromData(float data, int channel)
        {
            return channel + "_" + data + "_\n";
        }

        // sendData
        public void SendString(string message)
        {
            try
            {
                // Encode string message
                byte[] data = Encoding.UTF8.GetBytes(message);

                // Send message
                client.Send(data, data.Length);

            }
            catch (Exception err)
            {
                Debug.LogError("UDPSend: Error sending message: " + err.Message);
            }
        }

        #endregion

    }
}