using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JsonParser
{
    public string topic;
    public string[] words;
    public int rewardCoin;
    public Board board;
}

[System.Serializable]
public class Board
{
    public int boardRow;
    public int boardCol;
    public List<Letters> letters;
}

[System.Serializable]
public class Letters
{
    public int rowIndex;
    public int colIndex;
    public string letter;

}
