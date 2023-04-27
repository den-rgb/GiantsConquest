using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class playerResources : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI stoneText;
    void Start()
    {
        PlayerPrefs.SetInt("wood", 100);
        PlayerPrefs.SetInt("stone", 100);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {
        stoneText.text = "Stone: " + PlayerPrefs.GetInt("stone").ToString();
        woodText.text = "Wood: " + PlayerPrefs.GetInt("wood").ToString();
    }
}
