using UnityEngine;
using System;
using DefaultNamespace;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour
{
    [SerializeField] public LevelsParserService levelsParserService;
    [SerializeField] public PlayerStateService playerStateService;
    [SerializeField] public ElementsGameController elementsGameController;

    private int _currentLevel = 1;
    
    private void Start()
    {
        elementsGameController.AddHandlers(LevelCompleteHandler, StateSaveHandler);
        LoadLevelAndStart();
    }

    private void LoadLevelAndStart()
    {
        var savedFieldData = playerStateService.LoadFieldDataFromPlayerPrefs();
        if (savedFieldData != null)
        {
            _currentLevel = savedFieldData.Level;
            elementsGameController.UpdateModel(savedFieldData);
        }
        else
        {
            var levelData = levelsParserService.ParseLevel(_currentLevel);
            elementsGameController.UpdateModel(levelData);
        }
    }
    
    private void LevelCompleteHandler()
    {
        ChangeLevel(1);
    }
    
    private void StateSaveHandler(FieldData fieldData)
    {
        playerStateService.SaveFieldDataToPlayerPrefs(fieldData);
    }
    
    public void ChangeLevel(int value)
    {
        if (!elementsGameController.FieldIsCalm) return;

        _currentLevel = GetNextLevelNumber(value);

        var levelData = levelsParserService.ParseLevel(_currentLevel);
        elementsGameController.UpdateModel(levelData);
    }
    
    private int GetNextLevelNumber(int value)
    {
        int newLevel = _currentLevel + value;

        if (newLevel > levelsParserService.levelCount)
        {
            newLevel = 1;
        }
        else if (newLevel < 1)
        {
            newLevel = levelsParserService.levelCount;
        }

        return newLevel;
    }
}