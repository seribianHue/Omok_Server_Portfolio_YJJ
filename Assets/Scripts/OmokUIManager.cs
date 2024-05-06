using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OmokUIManager : MonoBehaviour
{
    private void Start()
    {
        _lobbyGO.SetActive(true);
    }

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

    [Header("Lobby")]
    [SerializeField] GameObject _lobbyGO;

    public void SetLobby(bool onoff)
    {
        _lobbyGO.SetActive(onoff);
    }
    #region winlose
    [Header("WinLose")]
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

    public void SetWinLoseTextOff()
    {
        _winText.SetActive(false);
        _loseText.SetActive(false);

    }
    #endregion

    [Header("Timer")]
    [SerializeField] Slider _timeSlider;
    [SerializeField] Image _fillImage;

    public void SetSlider(float timeLimit, float curTime)
    {
        _timeSlider.maxValue = timeLimit;
        _timeSlider.value = curTime;
        _fillImage.color = Color.Lerp(Color.red, Color.green, curTime / timeLimit);
    }

    #region RuleUI

    [Header("·ê ¾î±è ÅØ½ºÆ® (3*3)"), SerializeField]
    GameObject _ruleAgainst33;

    [Header("·ê ¾î±è ÅØ½ºÆ® (3*4)"), SerializeField]
    GameObject _ruleAgainst34;

    [Header("·ê ¾î±è ÅØ½ºÆ® (4*4)"), SerializeField]
    GameObject _ruleAgainst44;
    public void Show_RuleAgainst33()
    {
        StartCoroutine(RuleAgainstShow33());
    }
    IEnumerator RuleAgainstShow33()
    {
        _ruleAgainst33.SetActive(true);
        yield return new WaitForSeconds(1);
        _ruleAgainst33.SetActive(false);
        yield return null;
    }
    public void Show_RuleAgainst34()
    {
        StartCoroutine(RuleAgainstShow34());
    }
    IEnumerator RuleAgainstShow34()
    {
        _ruleAgainst34.SetActive(true);
        yield return new WaitForSeconds(1);
        _ruleAgainst34.SetActive(false);
        yield return null;
    }
    public void Show_RuleAgainst44()
    {
        StartCoroutine(RuleAgainstShow44());
    }
    IEnumerator RuleAgainstShow44()
    {
        _ruleAgainst44.SetActive(true);
        yield return new WaitForSeconds(1);
        _ruleAgainst44.SetActive(false);
        yield return null;
    }
    #endregion

    #region restart

    [Header("Restart")]
    [SerializeField] Button _RSBTN;
    [SerializeField] GameObject _RSwaitingText;

    public void SetRSBTNOnOff(bool onoff)
    {
        _RSBTN.gameObject.SetActive(onoff);
    }

    public void SetRSwaitingText(bool onoff)
    {
        _RSwaitingText.SetActive(onoff);
    }

    #endregion
}
