using System;
using System.Collections;
using System.Collections.Generic;
using FromTargetingSystem2._0;
using Unity.Entities;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Counter gameCounter;
    [SerializeField] private SimplePlayerController playerController;
    [SerializeField] private GameObject levelRoot;
    [SerializeField] private UIManager guiManager;
    [SerializeField] private float gameRoundTime;
    [SerializeField] private GameObject player = null;
    [SerializeField] private GameObject playerMouseController = null;
    [SerializeField] private Transform spawnPoint = null;
    
    private GameObject levelRootClone;
    private GameObject playerClone = null;
    
    // Start the game by setting the welcome text active
    void Start()
    {
        guiManager.StartGameMenu.SetActive(true);
    }

    //Update and manage game
    private void Update()
    {
        if (ManageStartMenuScreen()) return;
        ManageTimeAndScore();
        ManageGameRestart();
        ManageGameEnded();
    }
    
    bool ManageStartMenuScreen()
    {
        if (Input.GetMouseButtonDown(0) && guiManager.StartGameMenu.activeSelf )
        {
            guiManager.StartGameMenu.SetActive(false);
            guiManager.HideStartMenu();
            gameCounter.TimerStarted = false;
            guiManager.ShowTimeCounterAndScoreMenu();
            guiManager.ShowHighScoreMenu();
            BuildLevel();
        }

        return  guiManager.StartGameMenu.activeSelf;
    }

    /// <summary>
    /// Uses Counter and UIManager to update number side of the game
    /// </summary>
    private void ManageTimeAndScore()
    {
        gameCounter.StartTimer(gameRoundTime);
        gameCounter.CountScore();
        gameCounter.CheckIfPlayerHighScored(gameCounter.Score);
        guiManager.UpdateScoreText(gameCounter.Score, gameCounter.GameTime);
        guiManager.UpdateHighScoreText();
    }

    private void ManageGameEnded()
    {
        ShowEndScreen(CheckForEndCondition());
    }

    private void ShowEndScreen(bool checkForEndCondition)
    {
        if (checkForEndCondition)
        {
            guiManager.HideTimeCounterAndScoreMenu();
            guiManager.ShowEndMenu();
            CleanUpLevel();
            PlayerPrefs.Save();
        }
    }

    private bool CheckForEndCondition()
    {
        return gameCounter.GameTime < 0.01;
    }
    
    
    private void ManageGameRestart()
    {
        if (guiManager.StartAgainMenu.activeSelf && Input.GetMouseButtonDown(0))
        {
            gameCounter.TimerStarted = false;
            guiManager.ShowTimeCounterAndScoreMenu();
            guiManager.StartAgainMenu.SetActive(false);
            BuildLevel();
        }
    }
    
    void BuildLevel()
    {
        levelRootClone = Instantiate(levelRoot);
        if (playerClone == null)
        {
            playerClone = Instantiate(player);
            playerClone.transform.position = spawnPoint.position;
            Instantiate(playerMouseController);
        }
        else
        {
            playerClone.SetActive(true);
            playerClone.transform.position = spawnPoint.position;
        }

        gameCounter.Score = 0;

    }
    
    void CleanUpLevel()
    {
        Destroy(levelRootClone);
        playerClone.SetActive(false);
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<MissileSystem>().KillAllEntities = true;
    }


}