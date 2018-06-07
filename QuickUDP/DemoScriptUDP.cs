using UnityEngine;
using System.Collections;
using QuickVR;

public class DemoScriptUDP : MonoBehaviour
{

    public GameObject goSender;
    public GameObject goReceiver;

    private QuickUDPReceive _udpReceiver;
    private QuickUDPSend _udpSender;



    void Start()
    {
        _udpReceiver = goReceiver.GetComponent<QuickUDPReceive>();
        _udpSender = goSender.GetComponent<QuickUDPSend>();

        //we can specify the IP and port of the sender or the receiver
        string sTestIP = "255.255.255.255";
        int testPort = 6666;
        _udpReceiver._StringIP = sTestIP;
        _udpReceiver._Port = testPort;

        //since we may have to read the IP and port from the main menu, we need to call Init function to create the connection
        _udpReceiver.Init();
        _udpSender.Init();


        //send string via UDP connection
        //the string must be in the format 1_X where X means the marker sent ( usually an integer )
        string sMessageToSend = "1_X_\n";
        _udpSender.SendString(sMessageToSend);

        //retrieve data received via UDP
        //the UDP receive script is storing the received data internally.
        //we can retrieve the whole received packet 
        //string sMessageReceived = _udpReceiver.GetPacket();
        //we can also retrrieve the specific value on a channel
        //double _sChannelData = _udpReceiver.GetDataPerChannel(0);

    }
}
