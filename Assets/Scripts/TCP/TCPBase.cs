using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;

public class TCPBase : MonoBehaviour
{
    private void Awake()
    {
        _ipAddress = NetCommon.GetHostIP();
    }

    [Header("IP Address"), SerializeField]
    protected string _ipAddress;
    public string _IPAddress => _ipAddress;

    [Header("Port Number"), SerializeField]
    protected int _port = 3000;
    public int _Port => _port;

    protected Socket _socket = null;


    public bool _IsConnected { get; protected set; }

    public const int _BUFFER_LENGTH = 1400;

    public void SendMsg(string msg)
    {
        try
        {
            if (_socket.Poll(0, SelectMode.SelectWrite))
            {
                byte[] sendPacket = Encoding.UTF8.GetBytes(msg);
                while (sendPacket.Length > 0)
                {
                    _socket.Send(sendPacket, SocketFlags.None);
                    sendPacket = new byte[0];
                }
            }
        }
        catch { return; }
    }

    public string ReciveMsg()
    {
        try
        {
            while (_socket.Poll(0, SelectMode.SelectRead))
            {
                byte[] receivePacket = new byte[_BUFFER_LENGTH];
                int recvSize = _socket.Receive(receivePacket);

                if (recvSize == 0)
                {
                    Debug.Log("Disconnect recv from client.");
                    Disconnect();
                    return null;
                }
                else
                {
                    string message = Encoding.UTF8.GetString(receivePacket);
                    return message;
                }
            }
            return null;
        }
        catch { return null; }
    }

    protected Socket _socketL = null;

    [Header("Connect Sign Delay Time"), SerializeField]
    int _backLog = 1;

    protected bool _isThreadRun = false;
    Thread _thread = null;

    public bool StartServer(int port, int backLog)
    {
        _port = port;
        _backLog = backLog;
        Debug.Log("Start Server Called!!");

        bool isOk = StartServer();

        if (isOk) Start_Thread();

        return isOk;
    }

    public bool StartServer()
    {
        try
        {
            _socketL = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socketL.Bind(new IPEndPoint(IPAddress.Any, _port));
            _socketL.Listen(_backLog);

            return true;
        }
        catch { return false; }
    }

    protected bool Start_Thread()
    {
        Debug.Log("Start Thread Called!!");

        try
        {
            _isThreadRun = true;
            _thread = new Thread(new ThreadStart(RunThread));
            _thread.IsBackground = true;
            _thread.Start();
        }
        catch
        {
            Debug.Log("Start Thread Failed TT");
            return false;
        }
        return true;
    }

    protected void RunThread()
    {
        Debug.Log("Run Thread Called!!");

        while (_isThreadRun)
        {
            AcceptClient();
            if (_socket != null)
                _IsConnected = !((_socket.Poll(1000, SelectMode.SelectRead) && (_socket.Available == 0)) || !_socket.Connected);
            else
            {
                Disconnect();
            }

            if (_socket != null && _IsConnected)
            {

            }
            Thread.Sleep(1);
        }
        Debug.Log("Run Thread Ended..!");
    }

    void AcceptClient()
    {
        if (_socketL != null && _socketL.Poll(0, SelectMode.SelectRead))
        {
            _socket = _socketL.Accept();
            _IsConnected = true;
        }
    }

    public virtual void Disconnect()
    {
        _IsConnected = false;
        if (_socket != null)
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;
        }
    }

    public void Finish_Thread()
    {
        _isThreadRun = false;
        if (_thread != null)
        {
            _thread.Join();
            _thread = null;
        }
    }

    public void Close()
    {
        Debug.Log("[ StopServer ] called ..");

        Finish_Thread();
        Disconnect();

        if (_socketL != null)
        {
            _socketL.Close();
            _socketL.Dispose();
            _socketL = null;
        }
        Debug.Log("Server stopped..");
    }

    #region Client

    public bool Connect(string ipAddress, int port)
    {
        _ipAddress = ipAddress; _port = port;

        StartClient();
        return _IsConnected;
    }

    public void StartClient()
    {
        Debug.Log("Connect Called!");

        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            _socket.Connect(_ipAddress, _port);

            Debug.Log("Connect to server");

            _IsConnected = Start_Thread();
        }
        catch (Exception e) { 
            Debug.LogError(e);
            _socket = null; }
    }

    #endregion
}
