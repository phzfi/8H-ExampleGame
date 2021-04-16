using System.Collections;
using UniRx;
using Unity.Entities;
using UnityEngine;

public class Counter : MonoBehaviour
{
    private float gameTime;
    private int score;
    private bool timerStarted = false;
    private int initialEnemyCount;
    private int lastCount = 0;
    public float GameTime
    {
        get { return gameTime; }
        set { gameTime = value; }
    }

    public int Score
    {
        get { return score; }
        set { score = value; }
    }

    public bool TimerStarted
    {
        get { return timerStarted; }
        set { timerStarted = value; }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void StartTimer(float newGameTime)
    {
        if (timerStarted == false)
        {
            gameTime = newGameTime;
            timerStarted = true;
            MainThreadDispatcher.StartUpdateMicroCoroutine(ReduceTime());
        }
    }


    public void CountScore()
    {
        int currentCount = World.DefaultGameObjectInjectionWorld.EntityManager.UniversalQuery.CalculateEntityCount();
        if (lastCount != currentCount)
        {
            score += currentCount * (int) gameTime;
            lastCount = currentCount;
        }
    }

    private IEnumerator ReduceTime()
    {
        while (gameTime > 0)
        {
            gameTime -= Time.deltaTime;
            yield return null;
        }

        gameTime = 0;
    }

    public void CheckIfPlayerHighScored(int Value)
    {
        if (Value > PlayerPrefs.GetInt("Score"))
        {
            PlayerPrefs.SetInt("Score", Value);
        }
    }
}