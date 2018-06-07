// Jorge Arroyo Palacios
// Last update 09/July/2012

// This script receives the physio data from our Matlab Simulink models by UDP. It receives the string with the 
// physio data, parse it and separate the different physio data in channels.

// The values of the physio channels to modify a shader of an avatar (for instance to make it blush or breathe) should be accessed through the
// public double[] dataPerChannel variable. Which is an array with the physio data per channel.

// As simple demonstration this script will play a HR sound evrytime a QRS is detected. For this you will need
// to specify the channel assinged to the QRS detection and set to true HRsoundEnabled variable.

// The respiration, SC and other variables can be updated at the update() function of the avatar, since there 
// is no need to updated faster as the breathing animation will be carried out at every frame.

// Keyboard control:
// Press "a" to hide/unhide the UDP box

// Notes:
// If you try to play a sound from another thread that is not from Unity (i.e. the ReceivePhysio asynchronously callback) you will get an error.
// The problem is that Unity is not thread safe so doees not support communication with other threads 
// (e.g. Windows.Media to play sounds)
// Thus the playing of the sound is at Update() function. However, if update() is called 
// at 30 samples per second this will add a maximum delay on the start of sound of 33.33 ms.


using UnityEngine;
using System.Collections.Generic;

using System;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace QuickVR
{

    public class QuickUDPReceive : MonoBehaviour
    {
        #region PUBLIC PARAMETERS
        public string _StringIP = "127.0.0.1";
        public int _Port = 9090;

        public delegate void onDataReceived(string data);
        /// <summary>
        /// Occurs when on data received. IMPORTANT: this function is called asynchronously. Do not change any object in the scene, nor any unity structure in this function. 
        /// Save the data instead and use it in an Update function
        /// </summary>
        public event onDataReceived OnDataReceived;
        /// <summary>
        /// Occurs the first update after data is received. This event is similar to OnDataReceived, but synchronous.
        /// </summary>
        public event onDataReceived OnSynchronousDataReceived;

        #endregion

        #region PRIVATE PARAMETERS
        private IPAddress IP;
        private double[] dataPerChannel;
        private string physioString;

        private IPEndPoint remoteEndPoint;
        private UdpClient receivingUdpClient;

        private List<string> physioStringList = new List<string>();

        // Delimiter to parse the UDP string
        private char[] delimiterChar = { '|' };

        private int maxNumChannels;					// maximum number of channels to be received
        private string[] stringPerChannel;			// data per channel in the form of string	

        // variables for displaying the physio data received on the GUI	
        private string physioDebugString;						// string with the UDP physio data from all channels
        private bool UDPreceivingStarted = false;

        public bool udpDebug = false;
        #endregion

        #region CREATION AND DESTRUCTION
        public void Init()
        {

            IP = IPAddress.Parse(_StringIP);
            // initialize the dataPerChannel array with a Maximum number of channels
            maxNumChannels = 20;
            dataPerChannel = new double[maxNumChannels];

            //Creates a UdpClient for reading incoming data.
            receivingUdpClient = new UdpClient(_Port);

            //Creates an IPEndPoint to record the IP Address and port number of the sender. 
            // The IPEndPoint will allow you to read datagrams sent from any source.            
            remoteEndPoint = new IPEndPoint(IP, 0);

            // Start to receive udp messages
            receivingUdpClient.BeginReceive(new AsyncCallback(ReceiveUDP), null);		// Non blocking instruction for UDP. Receives a datagram from a remote host asynchronously.				
        }

        protected virtual void OnApplicationQuit()
        {
            CloseUDP();
        }

        public virtual void CloseUDP()
        {
            if (receivingUdpClient != null)
            {
                receivingUdpClient.Close();
            }
        }
        #endregion

        #region UPDATE
        protected virtual void Update()
        {
            if (physioStringList.Count > 0 && OnSynchronousDataReceived != null)
            {
                foreach (string data in physioStringList)
                {
                    OnSynchronousDataReceived(data);
                }
                physioStringList.Clear();
            }
        }

        protected virtual void SplitDataChannels(string udpString)
        {
            UDPreceivingStarted = true;
            if (string.IsNullOrEmpty(udpString) == false)
            {
                stringPerChannel = udpString.Split(delimiterChar);

                for (int i = 0; i < stringPerChannel.Length - 1; i++)				// stringPerChannel.Length gives me number of channels + 1
                {
                    dataPerChannel[i] = double.Parse(stringPerChannel[i]);
                }
            }
        }

        #endregion

        #region GUI
        protected virtual void OnGUI()
        {
            // physioString variable keeps the last UDP value until another package arrives.
            physioDebugString = physioString;

            if (string.IsNullOrEmpty(physioDebugString))
            {
                physioDebugString = "Waiting UDP messages";
            }


            if (udpDebug)
            {
                DisplayPhysio();
            }
            if (!Event.current.isKey)
                return;


        }

        protected virtual void DisplayPhysio()
        {

            GUI.BeginGroup(new Rect(10, 10, 400, 300));

            GUI.Box(new Rect(0, 0, 400, 300), "Physio Data");

            if (UDPreceivingStarted)		// If UDPdata has been already received
            {
                GUI.Label(new Rect(20, 25, 400, 100), "UDP string:  " + physioDebugString);

                for (int i = 0; i < stringPerChannel.Length - 1; i++)
                {
                    GUI.Label(new Rect(20, 50 + (i * 25), 300, 50), "Channel " + i + " = " + dataPerChannel[i]);
                }
            }
            else
            {
                GUI.Label(new Rect(20, 25, 300, 25), physioDebugString);
            }

            GUI.EndGroup();
        }
        #endregion

        #region GET AND SET
        public virtual string GetPacket()
        {
            return physioString;
        }

        public virtual double GetDataPerChannel(int index)
        {
            return dataPerChannel[index];
        }
        #endregion

        #region CALLBACKS
        protected virtual void ReceiveUDP(IAsyncResult res)
        {
            Byte[] receiveBytes = receivingUdpClient.EndReceive(res, ref remoteEndPoint);	// Ends a pending asynchronous receive. Returns: If successful, the number of bytes received. If unsuccessful, this method returns 0.

            string message = Encoding.ASCII.GetString(receiveBytes);
            physioString = message.ToString();			// physioString variable keeps the last UDP value until another package arrives.
            physioStringList.Add(physioString);
            if (OnDataReceived != null)
            {
                OnDataReceived(physioString);
            }

            // get next packet 
            receivingUdpClient.BeginReceive(ReceiveUDP, null);
        }
        #endregion

    }
}