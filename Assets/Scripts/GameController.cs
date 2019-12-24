using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
public class GameController : MonoBehaviour
{
    string path;
    string jsonString;
    JsonParser data;
    bool playable = false;
    int counter;
    Animator anim;

    public TextMeshProUGUI titleTmp;
    public TextMeshProUGUI selectedTmp;
    public TextMeshProUGUI rewardCoinTMP;
    public TextMeshProUGUI marketCoinTMP;
    public GameObject window;
    public GameObject quad;
    public GameObject coin;

    List<GameObject> selectedObjs = new List<GameObject>();
    List<GameObject> letterList = new List<GameObject>();

    void Start()
    {
        path = Application.dataPath + "/Jsons/Puzzle.json";
        jsonString = File.ReadAllText(path);
        data = JsonUtility.FromJson<JsonParser>(jsonString);
        anim = window.GetComponent<Animator>();
        
        counter = data.words.Length;
        titleTmp.text = data.topic;
        rewardCoinTMP.text = "+ "+data.rewardCoin;
        selectedTmp.text = "";
        StartCoroutine(CreatePuzzle());

    }

    private void OnMouseDown()
    {
        if (playable)
        {
            GameObject go = FindNearestObj();
            selectedObjs.Add(go);
            Material[] mats = go.GetComponent<Renderer>().materials;
            selectedTmp.text += mats[1].name[0];
            PuzzleMarks();
        }
    }
    
    private void OnMouseDrag()
    {
        if (playable)
        {
            GameObject go = FindNearestObj();

            int index = selectedObjs.Count;
            if (index - 1 >= 0)
            {
                if (selectedObjs[index - 1] != FindNearestObj())
                {
                    selectedObjs.Add(go);
                    Material[] mats = go.GetComponent<Renderer>().materials;
                    selectedTmp.text += mats[1].name[0];
                    PuzzleMarks();
                }
            }
            
        }
    }
    private void OnMouseUp()
    {
        if (playable)
        {
            for (int i = 0; i < data.words.Length; i++)
            {
                if (selectedTmp.text == data.words[i])
                {
                    if (selectedObjs.Count != 0)
                    {
                        foreach (GameObject go in selectedObjs)
                        {
                            if (letterList.Count != 0)
                            {
                                letterList.Remove(go);
                                Destroy(go);
                            }

                        }
                    }

                    counter--;
                    if (counter == 0) //level completed.
                    {
                        playable = false;
                        titleTmp.text = "";
                        window.SetActive(true);
                    }
                }
            }
            if(selectedObjs.Count != 0)
            {
                foreach (GameObject go in selectedObjs)
                {
                    Material[] mats = go.GetComponent<Renderer>().materials;
                    mats[2] = Resources.Load("Materials/" + mats[1].name[0], typeof(Material)) as Material;
                    go.GetComponent<Renderer>().materials = mats;
                }
            }
            
            selectedObjs.Clear();
            selectedTmp.text = "";
        }
    }

    IEnumerator CreatePuzzle()
    {
        float multiplier = data.board.boardCol;
        if (multiplier < data.board.boardRow)
            multiplier = data.board.boardRow;

        multiplier = 4 / multiplier;

        for (int i = 0; i < data.board.boardRow; i++)
        {
            foreach (Letters x in data.board.letters)
            {
                if(x.rowIndex==i)
                {
                    GameObject gameObj = Instantiate(quad, new Vector3(x.colIndex * multiplier, x.rowIndex * multiplier, 0), Quaternion.identity);
                    Vector3 scale = new Vector3(gameObj.transform.localScale.x * multiplier , gameObj.transform.localScale.y * multiplier , gameObj.transform.localScale.y);

                    Material newMat = Resources.Load("Materials/" + x.letter, typeof(Material)) as Material;
                    Material[] mats = gameObj.GetComponent<Renderer>().materials;

                    if (newMat != null)
                    {
                        mats[1] = newMat;
                        mats[2] = newMat;
                    }
                    gameObj.GetComponent<Renderer>().materials = mats;

                    for (int j = 1; j <= 10; j++)
                    {
                        gameObj.transform.localScale = scale * j;
                        yield return new WaitForSeconds(0.005f);
                    }
                    

                }
            }

        }

        GameObject[] letters = GameObject.FindGameObjectsWithTag("letter");

        for (int i = 0; i < letters.Length; i++)
        {
            letterList.Add(letters[i]);
        }

        yield return new WaitForSeconds(0.1f);
        Debug.Log("Playable true");
        playable = true;
    }

    public void OnClick()
    {
        if (rewardCoinTMP.text == "+ " + data.rewardCoin)
        {
            StartCoroutine(ChangingNumbers());
        } 
    }

    IEnumerator ChangingNumbers()
    {
        List<GameObject> coins = new List<GameObject>();
        
        for (int i = data.rewardCoin; i >= 0; i--)
        {
            if (i % 3 == 0)
            {
                GameObject go = Instantiate(coin, coin.transform.position, Quaternion.identity);
                Animator anim = go.GetComponent<Animator>();
                int trigger = Random.Range(1, 5);
                anim.SetTrigger("coin" + trigger);
                coins.Add(go);
            }

            rewardCoinTMP.text = "+ " + i;
            yield return new WaitForSeconds(0.001f);
        }

        for (int i = 0; i <= data.rewardCoin; i++)
        {
            marketCoinTMP.text = i.ToString();
            yield return new WaitForSeconds(0.001f);
        }

        yield return new WaitForSeconds(1f);
        foreach(GameObject go in coins)
        {
            Destroy(go);
        }
        anim.SetTrigger("close");
    }

    GameObject FindNearestObj()
    {
        GameObject selected = null;
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        float closestTarget = Mathf.Infinity;

        if (letterList.Count != 0)
        {
            foreach (GameObject go in letterList)
            {
                Vector3 diff = go.transform.position - mousePos;
                float distance = diff.sqrMagnitude;
                if (distance < closestTarget)
                {
                    closestTarget = distance;
                    selected = go;
                }
            }
        }

        return selected;
    }

    void PuzzleMarks()
    {
        if (selectedObjs.Count == 1)
        {
            SetMaterial(selectedObjs[0], "11");
        }
        
        if (selectedObjs.Count == 2)
        {
            
            if (Mathf.RoundToInt(selectedObjs[0].transform.position.x*10f)/10f < Mathf.RoundToInt(selectedObjs[1].transform.position.x*10f)/10f) //sağa 
            {
                SetMaterial(selectedObjs[0], "1");
                SetMaterial(selectedObjs[1], "2");
            }
            if (Mathf.RoundToInt(selectedObjs[0].transform.position.x* 10f)/ 10f > Mathf.RoundToInt(selectedObjs[1].transform.position.x* 10f)/ 10f) //sola
            {
                SetMaterial(selectedObjs[0], "2");
                SetMaterial(selectedObjs[1], "1");
            }
            if (Mathf.RoundToInt(selectedObjs[0].transform.position.y* 10f)/ 10f < Mathf.RoundToInt(selectedObjs[1].transform.position.y * 10f) / 10f) //yukarı
            {
                SetMaterial(selectedObjs[0], "4");
                SetMaterial(selectedObjs[1], "3");
            }
            if (Mathf.RoundToInt(selectedObjs[0].transform.position.y * 10f) / 10f > Mathf.RoundToInt(selectedObjs[1].transform.position.y * 10f) / 10f) //aşağı
            {
                SetMaterial(selectedObjs[0], "3");
                SetMaterial(selectedObjs[1], "4");
            }

        }

        if (selectedObjs.Count > 2)
        {
            for (int i = 1; i < selectedObjs.Count - 1; i++)
            {
                if (Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i + 1].transform.position.x * 10f) / 10f) //sağa
                {
                    SetMaterial(selectedObjs[i + 1], "2");
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "5");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "7");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "10");
                    }

                }
                if (Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i + 1].transform.position.x * 10f) / 10f) //sola
                {
                    SetMaterial(selectedObjs[i + 1], "1");
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "5");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "8");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "9");
                    }
                }
                if (Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i + 1].transform.position.y * 10f) / 10f) //yukarı
                {
                    SetMaterial(selectedObjs[i + 1], "3");
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "6");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "9");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "10");
                    }
                }
                if (Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i + 1].transform.position.y * 10f) / 10f) //aşağı
                {
                    SetMaterial(selectedObjs[i + 1], "4");
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.y * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.y * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "6");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f < Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "8");
                    }
                    if (Mathf.RoundToInt(selectedObjs[i - 1].transform.position.x * 10f) / 10f > Mathf.RoundToInt(selectedObjs[i].transform.position.x * 10f) / 10f)
                    {
                        SetMaterial(selectedObjs[i], "7");
                    }
                }
            }
        }
    }

    void SetMaterial(GameObject go, string matName)
    {
        Material[] mats = go.GetComponent<Renderer>().materials;
        mats[2] = Resources.Load("Materials/" + matName, typeof(Material)) as Material;
        go.GetComponent<Renderer>().materials = mats;
    }


}


