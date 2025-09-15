using System;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

public class LevelsParserService : MonoBehaviour
{
    [HideInInspector]
    public int levelCount;

    private void Awake()
    {
        GetLevelsCount();
    }

    private void GetLevelsCount()
    {
        string loadLevelCountConfig = LoadLevelCountConfig();
        levelCount = ParseFromLevelsCountConfig(loadLevelCountConfig);
    }

    public FieldData ParseLevel(int level)
    {
        string levelConfig = LoadLevelConfig(level);
        var levelData = ParseFromLevelConfig(levelConfig);

        var fieldData = new FieldData
        {
            Level = level,
            FieldHeight = levelData.levelHeight,
            FieldWidth = levelData.levelWidth,
            PieceData = new PieceData[levelData.levelHeight, levelData.levelWidth]
        };

        for (int i = 0; i < levelData.levelHeight; i++)
        {
            for (int j = 0; j < levelData.levelWidth; j++)
            {
                fieldData.PieceData[i, j] = new PieceData
                {
                    PieceType = levelData.level[i][j],
                };
            }
        }

        return fieldData;
    }

    private string LoadLevelConfig(int level)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"Levels/Level_{level}");
        return jsonFile.text;
    }

    private LevelData ParseFromLevelConfig(string config)
    {
        var jsonFileCountFile = JsonConvert.DeserializeObject<LevelData>(config);
        return jsonFileCountFile;
    }

    private string LoadLevelCountConfig()
    {
        TextAsset jsonFileCount = Resources.Load<TextAsset>($"Levels/LevelsCount");
        return jsonFileCount.text;
    }

    private int ParseFromLevelsCountConfig(string config)
    {
        var jsonFileCountFile = JsonConvert.DeserializeObject<LevelsCountData>(config);
        return jsonFileCountFile.levelsCount;
    }
}