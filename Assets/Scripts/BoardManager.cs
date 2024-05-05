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

    public enum eMARK { None = -1, Balck, White };
    public struct SCell
    {
        public eMARK _state;
        public Vector2 _pos;
    }
    SCell[,] _cells;

    [Header("Points Transform"), SerializeField]
    Transform[] _pts;
    Vector3[] _ptScreenPos;

    private void Awake()
    {
        _mainCam = Camera.main;

        int index = 0;
        _ptScreenPos = new Vector3[(int)ePT.Count];
        foreach (Transform tmp in _pts)
            _ptScreenPos[index] = _mainCam.WorldToScreenPoint(_pts[index++].position);

        _cellWidth = (_ptScreenPos[1].x - _ptScreenPos[0].x) / (_cellCount - 1);
        _cellHeight = (_ptScreenPos[0].y - _ptScreenPos[2].y) / (_cellCount - 1);
        _cellDist = _cellWidth < _cellHeight ? _cellWidth : _cellHeight;

        _cells = new SCell[_cellCount, _cellCount];
        for (int i = 0; i < _cellCount; i++)
        {
            for (int j = 0; j < _cellCount; j++)
            {
                _cells[j, i]._pos = new Vector2(
                    _ptScreenPos[(int)ePT.LT].x + _cellWidth * j,
                    _ptScreenPos[(int)ePT.LT].y - _cellHeight * i);

                _cells[j, i]._state = eMARK.None;
            }
        }
/*        Debug.Log(_ptScreenPos[(int)ePT.LT].x + ", " + _ptScreenPos[(int)ePT.LT].y);
        Debug.Log("_cells[0, 0]._pos : " + _cells[0, 0]._pos);
        Debug.Log("_mainCam.ScreenToWorldPoint(_cells[0, 0]._pos : " + _mainCam.ScreenToWorldPoint(_cells[0, 0]._pos));
        Debug.Log("_cells[18, 18]._pos : " + _cells[18, 18]._pos);
        Debug.Log("_mainCam.ScreenToWorldPoint(_cells[18, 18]._pos : " + _mainCam.ScreenToWorldPoint(_cells[18, 18]._pos));*/
    }

    public bool SetMark(Vector3 mousPos, eMARK eMark, out int indexX, out int indexY)
    {
        bool isFind = false;
        bool isRuleOk = false;
        int how; 
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

                    isRuleOk = CheckOmokRule(j, i, eMark, out how);

                    if (isRuleOk)
                    {
                        SetMark(j, i, eMark);
                        indexX = j;
                        indexY = i;
                        Debug.Log(j + " : " + i);
                        break;
                    }
                    else
                    {
                        if (how == 3)
                            _uiManager.Show_RuleAgainst33();
                        else
                            _uiManager.Show_RuleAgainst34();
                        break;
                    }

                }
            }
            if (isFind && isRuleOk)
                break;
        }
        if(isFind == false)
            GameManager.Instance.CamShake(0.2f, 0.5f);

        return (isFind && isRuleOk);
    }

    public void SetMark(int x, int y, eMARK eMark)
    {
        _cells[x, y]._state = eMark;

        GameObject tmp = Instantiate(_prefab_Stone[(int)eMark]);
        Vector3 pos = _cells[x, y]._pos;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(pos);

        if(Physics.Raycast(ray, out hit, 100f))
        {
            print(hit.transform.name);
            Debug.Log(hit.point);
            tmp.transform.position = hit.point;

        }


/*        Debug.Log(pos);
        //pos.z = pos.y;
        pos.z = 0.5f;
        pos = _mainCam.ScreenToWorldPoint(pos);
        Debug.Log(pos);
        pos.y = 0.5f;
        //tmp.transform.position = pos;*/

    }

    public void setRandomMark(eMARK eMark)
    {
        bool isOK = false;
        int ranX = -1;
        int ranY = -1;

        while (isOK == false)
        {
            ranX = Random.Range(0, _cellCount);
            ranY = Random.Range(0, _cellCount);
            if (_cells[ranX, ranY]._state == eMARK.None)
                isOK = true;
        }

        SetMark(ranX, ranY, eMark);

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
                return (eMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
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
                return (eMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
        }

        //사선 좌상 -> 우하 체크
        eMark = _cells[0, 0]._state;
        if (eMark != eMARK.None)
        {
            int matchingNum = 1;
            for (int xy = 1; xy < _cellCount; xy++)
                if (eMark == _cells[xy, xy]._state) { ++matchingNum; }

            if (matchingNum == _omokCount)
                return (eMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
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
                return (eMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
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

    public eWINNER CheckOmokWinner(int indexX, int indexY)
    {
        eMARK curMark = _cells[indexX, indexY]._state;
        eMARK nextMark = eMARK.None;
        int matchingNum = 1;
        bool isOmok = false;

        if (CheckStoneUpDown(indexX, indexY, curMark) >= 4)
            return (curMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
        if (CheckStoneLeftRight(indexX, indexY, curMark) >= 4)
            return (curMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
        if (CheckStoneLeftUp(indexX, indexY, curMark) >= 4)
            return (curMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;
        if (CheckStoneRightUp(indexX, indexY, curMark) >= 4)
            return (curMark == eMARK.Balck) ? eWINNER.Win : eWINNER.Lose;

        //무승부
        int emptyCount = 0;
        for (int x = 0; x < _cellCount; x++)
        {
            for (int y = 0; y < _cellCount; y++)
                if (_cells[x, y]._state == eMARK.None) { ++emptyCount; }
        }

        if (emptyCount == 0)
            return eWINNER.Tie;

        //아직 게임 진행중
        return eWINNER.None;
    }

    public bool CheckOmokRule(int indexX, int indexY, eMARK eMark, out int howRule)
    {
        howRule = 0;
        int count3 = 0;
        int count4 = 0;

        int UpDown = CheckStoneUpDown(indexX, indexY, eMark);
        if (UpDown >= 2)
        {
            if (UpDown == 2)
                ++count3;
            else if (UpDown == 3)
                ++count4;
        }

        int LeftRight = CheckStoneLeftRight(indexX, indexY, eMark);
        if (LeftRight >= 2)
        {
            if (LeftRight == 2)
                ++count3;
            else if (LeftRight == 3)
                ++count4;
        }

        int LeftUp = CheckStoneLeftUp(indexX, indexY, eMark);
        if (LeftUp >= 2)
        {
            if (LeftUp == 2)
                ++count3;
            else if (LeftUp == 3)
                ++count4;
        }

        int RightUp = CheckStoneRightUp(indexX, indexY, eMark);
        if (RightUp >= 2)
        {
            if (RightUp == 2)
                ++count3;
            else if (RightUp == 3)
                ++count4;
        }

        if (count3 >= 1)
        {
            if (count3 == 2)
            {
                howRule = 3; return false;
            }
            else if (count4 == 1)
            {
                howRule = 4; return false;
            }
        }
        else if (count4 >= 2)
        {
            howRule = 4; return false;
        }
        return true;

    }

    public int CheckStoneUpDown(int x, int y, eMARK eMark)
    {
        eMARK curMark = eMark;
        eMARK nextMark = eMARK.None;

        int tmpX = x, tmpY = y;
        int stoneCount = 0;
        //상
        int jump = 0;
        do
        {
            try
            {
                nextMark = _cells[tmpX, --tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        tmpX = x; tmpY = y;

        //하
        do
        {
            try
            {
                nextMark = _cells[tmpX, ++tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        return stoneCount;
    }
    public int CheckStoneLeftRight(int x, int y, eMARK eMark)
    {
        eMARK curMark = eMark;
        eMARK nextMark = eMARK.None;

        int tmpX = x, tmpY = y;
        int stoneCount = 0;
        int jump = 0;
        //좌
        do
        {
            try
            {
                nextMark = _cells[--tmpX, tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        tmpX = x; tmpY = y;

        //우
        do
        {
            try
            {
                nextMark = _cells[++tmpX, tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        return stoneCount;
    }
    public int CheckStoneLeftUp(int x, int y, eMARK eMark)
    {
        eMARK curMark = eMark;
        eMARK nextMark = eMARK.None;

        int tmpX = x, tmpY = y;
        int stoneCount = 0;
        //상
        int jump = 0;
        do
        {
            try
            {
                nextMark = _cells[++tmpX, --tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;
                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        tmpX = x; tmpY = y;

        //하
        do
        {
            try
            {
                nextMark = _cells[--tmpX, ++tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        return stoneCount;
    }
    public int CheckStoneRightUp(int x, int y, eMARK eMark)
    {
        eMARK curMark = eMark;
        eMARK nextMark = eMARK.None;

        int tmpX = x, tmpY = y;
        int stoneCount = 0;
        //상
        int jump = 0;
        do
        {
            try
            {
                nextMark = _cells[--tmpX, --tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        tmpX = x; tmpY = y;

        //하
        do
        {
            try
            {
                nextMark = _cells[++tmpX, ++tmpY]._state;
                if (nextMark == curMark)
                {
                    ++stoneCount;
                    continue;
                }
                else if (nextMark == eMARK.None)
                {
                    ++jump;
                }
                else if (jump >= 2)
                {
                    jump = 0;

                    break;
                }
                else
                {
                    break;
                }
            }
            catch
            {
                break;
            }
        } while (nextMark == curMark);

        return stoneCount;
    }
}
