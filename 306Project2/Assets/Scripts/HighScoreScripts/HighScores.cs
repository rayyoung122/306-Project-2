﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HighScores : MonoBehaviour {

    public GameObject totalPanelEasy;
    public GameObject level1PanelEasy;
    public GameObject level2PanelEasy;
    public GameObject level3PanelEasy;
    public GameObject highScorePanel;

    public GameObject playerListPrefab;

    public GameObject noPlayersText;

    private Color levelColor = new Color(0.1135636f, 0.5811083f, 0.8301887f);

    private float startPosForFormat = 185f;
    private float heightInterval = 40f;

    List<bool> showLevelText = new List<bool>();

    //Initialize differculty to 0 for easy (default) at 1 for hard
    private int selectedDifferculty;

    private int levelSelected;

    private Font font;

    private int maxScoreNum = 7;

	// Use this for initialization
	void Start () {


        showLevelText.Add(false);
        showLevelText.Add(false);
        showLevelText.Add(false);
        showLevelText.Add(false);

        noPlayersText.SetActive(false);

        font = (Font) Resources.Load("Font/earthorbiter");

        levelSelected = 0;

        //Initalize reading from file here into (highScorePlayers)
        level1PanelEasy.SetActive(false);
        level2PanelEasy.SetActive(false);
        level3PanelEasy.SetActive(false);

        DirectoryInfo info = new DirectoryInfo(Application.persistentDataPath);

        SaveManager saveManager = new SaveManager();

        List<SaveData> saves = saveManager.LoadSave();

        List<SaveData> sortedSaves = SortLevels(saves);

        float format = startPosForFormat;

        int previousLevel = 0;
        //Loads players to appropriate level
        for (int i = 0; i < sortedSaves.Count; i++)
        {
            if (previousLevel != sortedSaves[i].GetLevel())
            {
                format = startPosForFormat;
            }

            previousLevel = sortedSaves[i].GetLevel();

            format = format - heightInterval;

            SaveData data = sortedSaves[i];

            ReadFile(data.GetLevel(), data, format);
        }
    }

    // Update is called once per frame
    void Update () {
        

    }


    //Reads files related to gamemode and level to show high scores
    private void ReadFile(int level, SaveData data, float format)
    {
        GameObject parentPanel = null;
        float score = 0;

        //If score relates to total
        if (level == 0)
        {
            score = data.TotalScore();
            parentPanel = totalPanelEasy;
        }
        //If score relates to level1
        else if (level == 1)
        {
            score = data.GetLevel1Score();
            parentPanel = level1PanelEasy;
        }
        //If score relates to level2
        else if (level == 2)
        {
            score = data.GetLevel2Score();
            parentPanel = level2PanelEasy;
        }
        //If score relates to level3
        else if (level == 3)
        {
            score = data.GetLevel3Score();
            parentPanel = level3PanelEasy;
        }
       
        
        //Create gameobject for player from script
        GameObject p = Instantiate(playerListPrefab, parentPanel.transform);
        p.transform.localPosition = new Vector2(0f, format);
        p.GetComponent<Text>().text = data.GetPlayerName() + " " + score.ToString();
        p.GetComponent<Text>().font = font;
        p.name = data.GetPlayerName();

    }

    public void Level1()
    {
        //Set no player text if no players exist for that score
        if (showLevelText[0] == false)
        {
            noPlayersText.SetActive(true);
        }
        else
        {
            noPlayersText.SetActive(false);
        }

        levelSelected = 1;

        //Toggle panels
        level1PanelEasy.SetActive(true);
        level2PanelEasy.SetActive(false);
        level3PanelEasy.SetActive(false);
        totalPanelEasy.SetActive(false);

        

        //Color change code
        for (int i = 0; i < highScorePanel.transform.childCount; i++)
        {
            highScorePanel.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
        highScorePanel.transform.GetChild(0).GetComponent<Image>().color = levelColor;

    }

    public void Level2()
    {
        //Set no player text if no players exist for that score
        if (showLevelText[1] == false)
        {
            noPlayersText.SetActive(true);
        }
        else
        {
            noPlayersText.SetActive(false);
        }

        levelSelected = 2;

        //Toggle panels
        level1PanelEasy.SetActive(false);
        level2PanelEasy.SetActive(true);
        level3PanelEasy.SetActive(false);
        totalPanelEasy.SetActive(false);

        //Color change code
        for (int i = 0; i < highScorePanel.transform.childCount; i++)
        {
            highScorePanel.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
        highScorePanel.transform.GetChild(1).GetComponent<Image>().color = levelColor;
    }

    public void Level3()
    {
        //Set no player text if no players exist for that score
        if (showLevelText[2] == false)
        {
            noPlayersText.SetActive(true);
        }
        else
        {
            noPlayersText.SetActive(false);
        }

        levelSelected = 3;

        //Toggle panels
        level1PanelEasy.SetActive(false);
        level2PanelEasy.SetActive(false);
        level3PanelEasy.SetActive(true);
        totalPanelEasy.SetActive(false);

        

        //Color change code
        for (int i = 0; i < highScorePanel.transform.childCount; i++)
        {
            highScorePanel.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
        highScorePanel.transform.GetChild(2).GetComponent<Image>().color = levelColor;
    }

    public void Total()
    {
        //Set no player text if no players exist for that score
        if (showLevelText[3] == false)
        {
            noPlayersText.SetActive(true);
        }
        else
        {
            noPlayersText.SetActive(false);
        }

        levelSelected = 0;

        //Toggle panels
        level1PanelEasy.SetActive(false);
        level2PanelEasy.SetActive(false);
        level3PanelEasy.SetActive(false);
        totalPanelEasy.SetActive(true);

        //Color change code
        for (int i = 0; i < highScorePanel.transform.childCount; i++)
        {
            highScorePanel.transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }
        highScorePanel.transform.GetChild(3).GetComponent<Image>().color = levelColor;
    }

    //Back button to main menu
    public void BackToMenu()
    {
        SceneManager.LoadScene("WelcomeScene");
    }

    //Sorts the player scores from the .dat files
    private List<SaveData> SortLevels(List<SaveData> saveData)
    {

        List<SaveData> sortedData = new List<SaveData>();

        List<SaveData> level1EasySaves = new List<SaveData>();
        List<SaveData> level2EasySaves = new List<SaveData>();
        List<SaveData> level3EasySaves = new List<SaveData>();
        List<SaveData> totalEasySaves = new List<SaveData>();

        //Loop through all saves and add scores to appropriate data structures via level ordering
        foreach (SaveData save in saveData)
        {
            SaveData level1 = new SaveData(save.GetLevel1Score(), 1, save.GetPlayerName());
            level1EasySaves.Add(level1);

            SaveData level2 = new SaveData(save.GetLevel2Score(), 2, save.GetPlayerName());
            level2EasySaves.Add(level2);

            SaveData level3 = new SaveData(save.GetLevel3Score(), 3, save.GetPlayerName());
            level3EasySaves.Add(level3);

            SaveData totalEasy = new SaveData(save.GetTotalScore(), 0, save.GetPlayerName());
            totalEasySaves.Add(totalEasy);

            
        }

        //Sorts by highest to lowest score
        level1EasySaves = level1EasySaves.OrderByDescending(s => s.GetLevel1Score()).ToList();
        level2EasySaves = level2EasySaves.OrderByDescending(s => s.GetLevel2Score()).ToList();
        level3EasySaves = level3EasySaves.OrderByDescending(s => s.GetLevel3Score()).ToList();
        totalEasySaves = totalEasySaves.OrderByDescending(s => s.TotalScore()).ToList();

        List<SaveData> level1EasyData = new List<SaveData>();
        List<SaveData> level2EasyData = new List<SaveData>();
        List<SaveData> level3EasyData = new List<SaveData>();
        List<SaveData> totalEasyData = new List<SaveData>();

        //Loop through all scores and show only the top 7 (maxScoreNum)
        for (int i = 0; i < maxScoreNum; i++)
        {

            if (level1EasySaves.Count > i)
            {
                if (level1EasySaves[i].GetLevel1Score() > 0)
                {
                    level1EasyData.Add(level1EasySaves[i]);
                }

            }

            if (level2EasySaves.Count > i)
            {
                if (level2EasySaves[i].GetLevel2Score() > 0)
                {
                    level2EasyData.Add(level2EasySaves[i]);
                }
                
            }

            if (level3EasySaves.Count > i)
            {
                if (level3EasySaves[i].GetLevel3Score() > 0)
                {
                    level3EasyData.Add(level3EasySaves[i]);
                }
            }

            if (totalEasySaves.Count > i)
            {
                if (totalEasySaves[i].TotalScore() > 0)
                {
                    totalEasyData.Add(totalEasySaves[i]);
                }
                
            }
            

        }

        //Determines weather to display no score text if player hasnt completed the level for that device
        if (level1EasyData.Count > 0) 
        {
            showLevelText[0] = true;
        }
        if (level2EasyData.Count > 0)
        {
            showLevelText[1] = true;
        }
        if (level3EasyData.Count > 0)
        {
            showLevelText[2] = true;
        }

        //Adds level scores IN ORDER to the sorted data IMPORTANT FOR FORMAT DISTRIBUTUION ON SCREEN LATER
        sortedData.AddRange(level1EasyData);
        sortedData.AddRange(level2EasyData);
        sortedData.AddRange(level3EasyData);
        sortedData.AddRange(totalEasyData);

        if (sortedData.Count > 0)
        {
            showLevelText[3] = true;
            
        }
        else {
            noPlayersText.SetActive(true);
        }


        return sortedData;
    }
}
