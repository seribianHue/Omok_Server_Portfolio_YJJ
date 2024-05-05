using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    public static GameManager Instance { get { return instance; } }

    //hi
    [Header("TCP Base Prefab"), SerializeField]
    GameObject _myTCP;
    TCPBase _tcp;

    [Header("최소 port"), SerializeField]
    int _minPort = 1024;
    [Header("최대 port"), SerializeField]
    int _maxPort = 49151;
    int _port = -1;

    [Header("UI"), SerializeField]
    OmokUIManager _uiManager;
    
    [Header("Board Manager"), SerializeField]
    BoardManager _boardManager;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameObject tcp = Instantiate(_myTCP);
        _tcp = tcp.GetComponent<TCPBase>();

        _uiManager._IFip.text = _tcp._IPAddress;
        if(_tcp.GetValidTCPPort(out _port, _minPort, _maxPort))
        {
            _uiManager._IFport.text = _tcp._Port.ToString();
            _uiManager._port = _tcp._Port;
        }
    }

    bool _isServerOn;

    public void OnBtnStartServer()
    {
        _isServerOn = _tcp.StartServer(_uiManager._port, 1);
        if (_isServerOn)
        {
            _uiManager.SetWaiting();
            _myMark = BoardManager.eMARK.Balck;
            _opponentMark = BoardManager.eMARK.White;
        }
    }

    bool _isConnected;
    public void OnBtnConnect()
    {
        _isConnected = _tcp.Connect(_uiManager._ip, _uiManager._port);
        if (_isConnected)
        {
            _uiManager.SetConnecting();
            _myMark = BoardManager.eMARK.White;
            _opponentMark = BoardManager.eMARK.Balck;

        }
    }

    private void Update()
    {
        if (_tcp._IsConnected && !_isGameOver)
        {
            _uiManager.SetLobby(false);
            UpdateTurn();
            UpdateWinner();
            _uiManager.SetSlider(_timeLimit, _curTime);
        }
    }

    public enum eRESULT { NONE = -1, WIN, LOSE, TIE }
    public eRESULT _eResult = eRESULT.NONE;

    public BoardManager.eMARK _curTurnMark;
    public BoardManager.eMARK _myMark;
    public BoardManager.eMARK _opponentMark;

    public bool isPause = false;

    [SerializeField] float _timeLimit = 15f;
    float _curTime = 0;

    bool DoMyTurn(out int X, out int Y)
    {
        int indexX = 0, indexY = 0;
        if (isPause == false)
        {
            if(_curTime < _timeLimit)
            {
                _curTime += Time.deltaTime;
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 pos = Input.mousePosition;
                    Debug.Log(pos);
                    bool ret = _boardManager.SetMark(pos, _myMark, out indexX, out indexY);
                    if (ret == false) { Debug.Log("Check Position"); }
                    else
                    {
                        string indexXY = indexX.ToString() + "," + indexY.ToString();
                        _tcp.SendMsg(indexXY);

                        _curTime = 0;

                        X = indexX; Y = indexY;
                        return true;
                    }
                }
            }
            else
            {
                //random mark
                _boardManager.setRandomMark(_myMark);

                _curTime = 0;
                X = -1; Y = -1;
                return true;
            }
        }
        else
        {
            X = -1; Y = -1;
            return false;
        }
        X = -1; Y = -1;
        return false;
    }

    bool DoOpponentTurn(out int X, out int Y)
    {
        //byte[] buffer = new byte[2];
        //int recvSize = _tcp.Receive(ref buffer, buffer.Length);

        string recvXY = _tcp.ReciveMsg();

        if (recvXY == null) { X = -1; Y = -1; return false; }

        string[] XY = recvXY.Split(',');
        int indexX = Int32.Parse(XY[0]);
        int indexY = Int32.Parse(XY[1]);


        _boardManager.SetMark(indexX, indexY, _opponentMark);

        X = indexX; Y = indexY;
        return true;
    }

    public void UpdateTurn()
    {
        bool ret = false;
        int X, Y;
        if (_curTurnMark == _myMark)
            ret = DoMyTurn(out X, out Y);
        else
        {
            ret = DoOpponentTurn(out X, out Y);
        }

        if (ret)
        {

            BoardManager.eWINNER eWinner = _boardManager.CheckOmokWinner(X, Y);
            if (eWinner != BoardManager.eWINNER.None)
            {
                if (eWinner == BoardManager.eWINNER.Win && _curTurnMark == _myMark
                    || eWinner == BoardManager.eWINNER.Lose && _curTurnMark == _opponentMark)
                {
                    _eResult = eRESULT.WIN;
                }
                else if (eWinner == BoardManager.eWINNER.Tie) _eResult = eRESULT.TIE;
                else _eResult = eRESULT.LOSE;

            }
            _curTurnMark = (_curTurnMark == BoardManager.eMARK.Balck) ? BoardManager.eMARK.White : BoardManager.eMARK.Balck;
        }
    }
    public bool _isGameOver = false;

    void UpdateWinner()
    {
        if (_eResult == eRESULT.NONE)
        {
            return;
        }
        else if( _eResult == eRESULT.WIN)
        {
            _isGameOver = true;
            _uiManager.SetWinText();
        }
        else
        {
            _isGameOver = true;
            _uiManager.SetLoseText();
        }
    }

    [SerializeField]
    private float _camShakeRoughness;      //거칠기 정도
    [SerializeField]
    private float _camShakeMagnitude;

    public void CamShake(float ShakeAmount, float ShakeTime)
    {
        StartCoroutine(CRT_CamShake(ShakeAmount, ShakeTime));
    }

    IEnumerator CRT_CamShake(float ShakeAmount, float ShakeTime)
    {
        Vector3 initCamPos = Camera.main.transform.position;
        float timer = 0;
        while (timer <= ShakeTime)
        {
            Camera.main.transform.position = initCamPos + new Vector3(UnityEngine.Random.Range(0f, 1f) * ShakeAmount, 0, UnityEngine.Random.Range(0f, 1f) * ShakeAmount);
            timer += Time.deltaTime;
            isPause = true;
            yield return null;
        }
        Camera.main.transform.position = initCamPos;
        isPause = false;
        yield return null;
    }

}
