using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startGameMenu;
    [SerializeField] private GameObject counterAndScoreMenu;
    [SerializeField] private GameObject highScoreMenu;
    [FormerlySerializedAs("endMenuStartAgain")] [SerializeField] private GameObject startAgainMenu;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    public GameObject StartAgainMenu
    {
        get { return startAgainMenu; }
        set { startAgainMenu = value; }
    }

    public GameObject StartGameMenu
    {
        get { return startGameMenu; }
        set { startGameMenu = value; }
    }

    public void HideStartMenu()
    {
        startGameMenu.SetActive(false);
    }

    public void ShowStartMenu()
    {
        startGameMenu.SetActive(true);
    }

    public void UpdateScoreText(float score, float time)
    {
        scoreText.text = score + "/" + time;
    }
    public void ShowTimeCounterAndScoreMenu()
    {
        counterAndScoreMenu.SetActive(true);
    }

    public void HideTimeCounterAndScoreMenu()
    {
        counterAndScoreMenu.SetActive(false);
    }

    public void ShowEndMenu()
    {
        startAgainMenu.SetActive(true);
    }
    
    public void HideEndMenu()
    {
        startAgainMenu.SetActive(false);
    }
    
    public void ShowHighScoreMenu()
    {
        highScoreMenu.SetActive(true);
    }
    public void HideHighScoreMenu()
    {
        highScoreMenu.SetActive(false);
    }


    public void UpdateHighScoreText()
    {
        highScoreText.text = PlayerPrefs.GetInt("Score").ToString();
    }
}