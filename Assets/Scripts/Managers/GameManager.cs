using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class GameManager : MonoBehaviour
{
    
    public GameObject victoryPopup, mainMenuUI, gamePlayUI, showLevelsUI, settingsPopup, exitPopup, infoPopup;
    public GridManager gridManager; 

    public TextMeshProUGUI levelButtonText;

    private int currentLevel;

    private void Start()
    {
        
        currentLevel = LevelFileLoader.GetCurrentLevelNumber();
    }

    private void Update()
    {
        levelButtonText.text = "Level " + currentLevel.ToString();
    }

    private void Awake()
    {
        SetupGameStart();
    }

    private void SetupGameStart()
    {
        currentLevel = LevelFileLoader.GetCurrentLevelNumber();
        mainMenuUI.SetActive(true);
        gamePlayUI.SetActive(false);
        showLevelsUI.SetActive(false);
        settingsPopup.SetActive(false);
        exitPopup.SetActive(false);
        infoPopup.SetActive(false);
        victoryPopup.SetActive(false);
    }

    public void LoadLevelAndStartGame()
    {
        string levelFileName = GetLevelData();
        levelFileName += ".json";
        Debug.Log("Level adi gamemanager: " + levelFileName);
        LevelData levelData = LevelFileLoader.LoadLevel(levelFileName);
        Debug.Log("Level yuklendi");
        if (levelData != null)
        {

            // Generate grid using level data
            Transform boardTransform = gamePlayUI.transform.Find("Board");
            if (boardTransform != null)
            {
                Debug.Log("Eski Board silindi!!!");
                Transform newParent = gamePlayUI.transform.Find("GarbageCollector"); 
                boardTransform.transform.SetParent(newParent);
                Destroy(boardTransform.gameObject);
            }
            Transform backgroundTransform = gamePlayUI.transform.Find("Background");
            if (backgroundTransform != null)
            {
                Debug.Log("Destroying background objects...");

                Transform newParent = gamePlayUI.transform.Find("GarbageCollector");

                List<Transform> childrenToDestroy = new List<Transform>();

                foreach (Transform child in backgroundTransform)
                {
                    childrenToDestroy.Add(child);
                }

                foreach (Transform child in childrenToDestroy)
                {
                    child.SetParent(newParent);
                    Destroy(child.gameObject);
                }

                Debug.Log("All background objects destroyed.");
            }


            gridManager.GenerateGrid(levelData.grid_width, levelData.grid_height, levelData.grid, levelData.move_count);
            
            mainMenuUI.SetActive(false);
            gamePlayUI.SetActive(true);
            showLevelsUI.SetActive(false);
            settingsPopup.SetActive(false);
            exitPopup.SetActive(false);
            infoPopup.SetActive(false);

        }
        else
        {
            Debug.LogError("Failed to load level: " + levelFileName);
        }
    }

    public string GetLevelData()
    {
        string levelName = "level_";
        if(currentLevel < 10)
        {
            levelName += "0" + currentLevel.ToString();
        }
        else
        {
            levelName += currentLevel.ToString();
        }

        return levelName;
    }

    public void IncreaseLevelData()
    {
        currentLevel++;
        string levelFileName = GetLevelData();
        levelFileName += ".json";
        LevelData levelData = LevelFileLoader.LoadLevel(levelFileName);
        int currentLevelNumber = levelData.level_number;
        LevelFileLoader.UpdateCurrentLevelNumber(currentLevelNumber);
    }
}