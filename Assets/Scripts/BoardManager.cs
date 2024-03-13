using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    Camera _mainCam;

    [SerializeField]
    public OmokUIManager _uiManager;
    public enum eWINNER { None = 0/*시합중*/, Win, Lose, Tie };

    [Header("Stones BW"), SerializeField]
    GameObject[] _prefab_Stone;

    public enum ePT { LT, RT, LB, RB, Count }

    [Header("Cell Count"), SerializeField]
    public int _cellCount = 19;
    [Header("Omok Count"), SerializeField]
    public int _omokCount = 5;

    float _cellWidth, _cellHeight;
    float _cellDist;

    public enum eMARK { None = -1, Black, White };
    public struct SCell
    {
        public eMARK _state;
        public Vector2 _pos;
    }
    SCell[,] _cells;

    [Header("Points Transform"), SerializeField]
    Transform[] _pts;
    Vector3[] _ptPos;

    private void Awake()
    {
        _mainCam = Camera.main;

        int index = 0;
        _ptPos = new Vector3[(int)ePT.Count];
        foreach (Transform tmp in _pts)
            _ptPos[index] = _mainCam.WorldToScreenPoint(_pts[index++].position);

        Debug.Log(_ptPos[0].x + ", " + _ptPos[0].y);
        Vector3 dd = _mainCam.ScreenToWorldPoint(_ptPos[0]);

        Debug.Log(dd);


        _cellWidth = (_ptPos[1].x - _ptPos[0].x) / _cellCount;
        _cellHeight = (_ptPos[0].y - _ptPos[2].y) / _cellCount;
        _cellDist = _cellWidth < _cellHeight ? _cellWidth : _cellHeight;

        _cells = new SCell[_cellCount, _cellCount];
        for (int i = 0; i < _cellCount; i++)
        {
            for (int j = 0; j < _cellCount; j++)
            {
                _cells[j, i]._pos = new Vector2(
                    _ptPos[(int)ePT.LT].x + _cellWidth * j + _cellWidth * 0.5f,
                    _ptPos[(int)ePT.LT].y - _cellHeight * i - _cellHeight * 0.5f);

                _cells[j, i]._state = eMARK.None;
            }
        }
    }

    public bool SetMark(Vector3 mousPos, eMARK eMark, out int indexX, out int indexY)
    {
        bool isFind = false;
        indexX = -1;
        indexY = -1;
        for (int i = 0; i < _cellCount; i++)
        {
            for (int j = 0; j < _cellCount; j++)
            {
                if (Dist(mousPos, _cells[j, i]._pos) < _cellDist * 0.5f &&
                    _cells[j, i]._state == eMARK.None)
                {
                    isFind = true;

                    if (isFind)
                    {
                        SetMark(j, i, eMark);
                        indexX = j;
                        indexY = i;
                        Debug.Log(j + " : " + i);
                        break;
                    }

                }
            }
            if (isFind)
                break;
        }
        return (isFind);
    }

    public void SetMark(int x, int y, eMARK eMark)
    {
        _cells[x, y]._state = eMark;

        GameObject tmp = Instantiate(_prefab_Stone[(int)eMark]);
        Vector3 pos = _cells[x, y]._pos;
        pos.z = _mainCam.transform.position.z;
        pos = _mainCam.ScreenToWorldPoint(pos);
        pos.z = 0f;
        tmp.transform.position = pos;
    }

    float Dist(Vector3 mPos, Vector2 gridPos)
    {
        return Mathf.Sqrt(Mathf.Pow(mPos.x - gridPos.x, 2) + Mathf.Pow(mPos.y - gridPos.y, 2));
    }

    public eWINNER CheckWinner()
    {
        //가로 체크
        eMARK eMark = eMARK.None;
        for (int y = 0; y < _cellCount; y++)
        {
            int matchingNum = 1;
            eMark = _cells[0, y]._state;

            if (eMark == eMARK.None) { continue; }

            for (int x = 1; x < _cellCount; x++)
                if (eMark == _cells[x, y]._state) { ++matchingNum; }

            if (matchingNum == _omokCount)
                return (eMark == eMARK.Black) ? eWINNER.Win : eWINNER.Lose;
        }

        //세로 체크
        eMark = eMARK.None;
        for (int x = 0; x < _cellCount; ++x)
        {
            int matchingNum = 1;
            eMark = _cells[x, 0]._state;

            if (eMark == eMARK.None) { continue; }

            for (int y = 1; y < _cellCount; y++)
                if (eMark == _cells[x, y]._state) { ++matchingNum; }

            if (matchingNum == _omokCount)
                return (eMark == eMARK.Black) ? eWINNER.Win : eWINNER.Lose;
        }

        //사선 좌상 -> 우하 체크
        eMark = _cells[0, 0]._state;
        if (eMark != eMARK.None)
        {
            int matchingNum = 1;
            for (int xy = 1; xy < _cellCount; xy++)
                if (eMark == _cells[xy, xy]._state) { ++matchingNum; }

            if (matchingNum == _omokCount)
                return (eMark == eMARK.Black) ? eWINNER.Win : eWINNER.Lose;
        }

        //사선 좌하 -> 우상 체크
        eMark = _cells[0, _cellCount - 1]._state;
        if (eMark != eMARK.None)
        {
            int matchingNum = 1;
            int y = _cellCount - 2;

            for (int x = 1; x < _cellCount; x++)
                if (eMark == _cells[x, y--]._state) { ++matchingNum; }

            if (matchingNum == _omokCount)
                return (eMark == eMARK.Black) ? eWINNER.Win : eWINNER.Lose;
        }

        //무승부
        int emptyCount = 0;
        for (int x = 0; x < _cellCount; x++)
        {
            for (int y = 0; y < _cellCount; y++)
                if (_cells[x, y]._state == eMARK.None) { ++emptyCount; }
        }

        if (emptyCount == 0)
            return eWINNER.Tie;

        return eWINNER.None;
    }
    public eWINNER CheckOmokWinner(int indexX, int indexY, eMARK myMark)
    {
        eMARK eMark = eMARK.None;

        int tmpX = indexX;
        int tmpY = indexY;

        //가로 체크
        int matchingNum = 1;
        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpX - 1 >= 0)
            {
                eMark = _cells[--tmpX, tmpY]._state;
                if (eMark != myMark) { break; }
                else
                {
                    matchingNum++;
                }
            }
            else { break; }
        } while (true);

        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpX + 1 <= 18)
            {
                eMark = _cells[++tmpX, tmpY]._state;
                if (eMark != myMark) { break; }
                else
                {
                    matchingNum++;
                }
            }
            else { break; }
        } while (true);

        if(matchingNum >= 5) { return eWINNER.Win; }

        //세로 체크
        matchingNum = 1;
        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY - 1 >= 0)
            {
                eMark = _cells[tmpX, --tmpY]._state;
                if (eMark != myMark) { break; }
                else
                {
                    matchingNum++;
                }
            }
            else { break; }
        } while (true);

        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY + 1 <= 18)
            {
                eMark = _cells[tmpX, ++tmpY]._state;
                if (eMark != myMark) { break; }
                else
                {
                    matchingNum++;
                }
            }
            else { break; }
        } while (true);

        if (matchingNum >= 5) { return eWINNER.Win; }

        //사선 좌상 -> 우하 체크
        matchingNum = 1;
        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY - 1 >= 0)
            {
                if(tmpX - 1 >= 0)
                {
                    eMark = _cells[--tmpX, --tmpY]._state;
                    if (eMark != myMark) { break; }
                    else
                    {
                        matchingNum++;
                    }
                }
                else { break; }
            }
            else { break; }
        } while (true);

        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY + 1 <= 18)
            {
                if (tmpX + 1 <= 18)
                {
                    eMark = _cells[++tmpX, ++tmpY]._state;
                    if (eMark != myMark) { break; }
                    else
                    {
                        matchingNum++;
                    }
                }
                else { break; }
            }
            else { break; }
        } while (true);

        if (matchingNum >= 5) { return eWINNER.Win; }

        //사선 좌상 -> 우하 체크
        matchingNum = 1;
        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY - 1 >= 0)
            {
                if (tmpX + 1 <= 18)
                {
                    eMark = _cells[++tmpX, --tmpY]._state;
                    if (eMark != myMark) { break; }
                    else
                    {
                        matchingNum++;
                    }
                }
                else { break; }
            }
            else { break; }
        } while (true);

        tmpX = indexX;
        tmpY = indexY;
        do
        {
            if (tmpY - 1 >= 0)
            {
                if (tmpX + 1 <= 18)
                {
                    eMark = _cells[++tmpX, --tmpY]._state;
                    if (eMark != myMark) { break; }
                    else
                    {
                        matchingNum++;
                    }
                }
                else { break; }
            }
            else { break; }
        } while (true);

        if (matchingNum >= 5) { return eWINNER.Win; }

        return eWINNER.None;
    }
}
