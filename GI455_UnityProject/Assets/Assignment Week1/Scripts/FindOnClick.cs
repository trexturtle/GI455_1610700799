using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FindOnClick : MonoBehaviour
{
    public List<string> wordList;
    public Text showWordList;
    public Text searchResult;
    public InputField searchBox;
    
    // Start is called before the first frame update
    void Start()
    {
        string text = "";

        for (int i = 0; i < wordList.Count; i++)
        {
            text += wordList[i] + "\n";
        }

        showWordList.text = text;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickResult()
    {
        if (wordList.Contains(searchBox.text))
        {
            searchResult.text = "[ <color=green>" + searchBox.text + "</color> ]" + " is Found.";
        }
        else
        {
            searchResult.text = "[ <color=red>" + searchBox.text + "</color> ]" + " is not Found.";
        }
    }
}
