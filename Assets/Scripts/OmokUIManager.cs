using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OmokUIManager : MonoBehaviour
{
    [SerializeField] public TMP_InputField _IFip;
    [SerializeField] public TMP_InputField _IFport;
    //[SerializeField] public TMP_InputField _IFText;

    public string _ip;
    public int _port;

    public void SetIP(string ip)
    {
        _ip = ip;
    }

    public void SetPort(string port)
    {
        _port = Int32.Parse(port);
    }
    [Space]
    [SerializeField] GameObject _textWaiting;
    [SerializeField] GameObject _textConnecting;

    public void SetWaiting()
    {
        _textWaiting.SetActive(true);
    }

    public void SetConnecting()
    {
        _textConnecting.SetActive(true);
    }
    [Space]
    [SerializeField] GameObject _lobbyGO;

    public void SetLobby(bool onoff)
    {
        _lobbyGO.SetActive(onoff);
    }

    [Space]
    [SerializeField] GameObject _winText;
    [SerializeField] GameObject _loseText;

    public void SetWinText()
    {
        _winText.SetActive(true);
    }

    public void SetLoseText()
    {
        _loseText.SetActive(true);
    }

}
