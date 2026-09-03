using OSC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScoreBoardDisplayer : MonoBehaviour
{
    [SerializeField]
    private string _unit = "pts";

    [SerializeField]
    GameObject _scoreBoardObject;

    [SerializeField]
    GameObject _scoreDisplay;

    [SerializeField]
    Transform _collum;

    [SerializeField]
    UnityEvent _onDisplay;

    [SerializeField]
    bool _demandNameOnEnable = false;

    List<GameObject> _instantiateScoreDisplay = new List<GameObject>();

    PlayerData[] _datas;
    PlayerData _currentWinner;
    PlayerData _defaultPlayer;
    Func<TMP_FontAsset> _fontFunc;
    Color _winnerColor = Color.white;
    Color _defaultColor = Color.white;
    int _currentRank;
    bool _displayValue;

    private void Start()
    {
        if(_demandNameOnEnable)
            OSC_Manager.Instance.ShowSoftKeyboard();
    }

    public void InitScoreBoard(PlayerData[] datas, Func<TMP_FontAsset> fontFunc, PlayerData defaultPlayer = null, bool displayValue = false)
    {
        Debug.Log("InitScoreBoard called with datas length: " + datas.Length);
        _onDisplay?.Invoke();

        _datas = datas;
        _currentWinner = _datas.ToList().Find(x => x.WinNow);
        _currentRank = _currentWinner.Rank;
        _fontFunc = fontFunc;
        _winnerColor = Color.white;
        _defaultColor = Color.white;
        _defaultPlayer = defaultPlayer;
        _displayValue = displayValue;

        Init(FindPlayerData(_currentRank), _displayValue);

        DisplayScoreBoard();
    }

    public void InitScoreBoard(PlayerData[] datas, Func<TMP_FontAsset> fontFunc, Color winnerColor, PlayerData defaultPlayer = null, bool displayValue = false)
    {
        Debug.Log("InitScoreBoard called with datas length: " + datas.Length);
        _onDisplay?.Invoke();

        _datas = datas;
        _currentWinner = _datas.ToList().Find(x => x.WinNow);
        _currentRank = _currentWinner.Rank;
        _fontFunc = fontFunc;
        _winnerColor = winnerColor;
        _defaultColor = Color.white;
        _defaultPlayer = defaultPlayer;
        _displayValue = displayValue;

        Init(FindPlayerData(_currentRank), _displayValue);

        DisplayScoreBoard();
    }

    public void InitScoreBoard(PlayerData[] datas, Func<TMP_FontAsset> fontFunc, Color winnerColor, Color defaultColor, PlayerData defaultPlayer = null, bool displayValue = false)
    {
        Debug.Log("InitScoreBoard called with datas length: " + datas.Length);
        _onDisplay?.Invoke();

        _datas = datas;
        _currentWinner = _datas.ToList().Find(x => x.WinNow);
        _currentRank = _currentWinner.Rank;
        _fontFunc = fontFunc;
        _winnerColor = winnerColor;
        _defaultColor = defaultColor;
        _defaultPlayer = defaultPlayer;
        _displayValue = displayValue;

        Init(FindPlayerData(_currentRank), _displayValue);

        DisplayScoreBoard();
    }

    public void PageUp()
    {
        if(!_scoreBoardObject.activeSelf) return;

        _currentRank = Mathf.Clamp(_currentRank - 8, 0, _datas.Length);
        Init(FindPlayerData(_currentRank), _displayValue);
    }

    public void PageDown()
    {
        if (!_scoreBoardObject.activeSelf) return;

        _currentRank = Mathf.Clamp(_currentRank + 8, 0, _datas.Length);
        Init(FindPlayerData(_currentRank), _displayValue);
    }

    private PlayerData[] FindPlayerData(int rank)
    {
        int sec = (rank - 1) / 8;
        List<PlayerData> playerFind = new List<PlayerData>();
        for (int i = 0; i < 8; i++)
        {
            if(((sec * 8) + i) >= _datas.Length)
            {
                if (_defaultPlayer != null)
                {
                    PlayerData unknown = new PlayerData()
                    {
                        Name = _defaultPlayer.Name,
                        Score = _defaultPlayer.Score,
                        Rank = (sec * 8) + i + 1
                    };
                    playerFind.Add(unknown);
                }
                continue;
            }

            playerFind.Add(_datas[(sec * 8) + i]);
        }
        return playerFind.ToArray();
    }

    private void Init(PlayerData[] playerDatas, bool displayValue = false)
    {
        ResetDisplay();

        for (int i = 0; i < playerDatas.Length; i++)
        {
            GameObject go = Instantiate(_scoreDisplay, _collum);
            _instantiateScoreDisplay.Add(go);

            string[] texts = new string[3]
            {
                playerDatas[i].Rank + ". ",
                playerDatas[i].Name,
                " : " + (displayValue ? playerDatas[i].Value : playerDatas[i].Score.ToString()) + _unit
            };
            TMP_FontAsset font = playerDatas[i].WinNow ? _fontFunc() : null;
            Color color = playerDatas[i].WinNow ? _winnerColor : _defaultColor;

            go.GetComponent<ScoreDisplayer>().Init(texts, color, font);
        }
    }

    private void ResetDisplay()
    {
        foreach (GameObject go in _instantiateScoreDisplay)
            Destroy(go);
        _instantiateScoreDisplay.Clear();
    }
    public void DisplayScoreBoard() => _scoreBoardObject.SetActive(true);
    public void HideScoreBoard() => _scoreBoardObject.SetActive(false);
}
