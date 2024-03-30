using UnityEngine;
using System.IO;

public class LevelFileLoader : MonoBehaviour
{

    private const string levelProgressFilePath = "Assets/LevelProgress/LevelProgress.txt";

    public static LevelData LoadLevel(string fileName)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        TextAsset levelTextAsset = Resources.Load<TextAsset>("Levels/" + fileNameWithoutExtension);

        if (levelTextAsset != null)
        {
            return JsonUtility.FromJson<LevelData>(levelTextAsset.text);
        }
        else
        {
            Debug.LogError("Level file not found: " + fileName);
            return null;
        }
    }

    public static int GetCurrentLevelNumber()
    {
        if (File.Exists(levelProgressFilePath))
        {
            string json = File.ReadAllText(levelProgressFilePath);

            try
            {
                LevelProgressData levelProgressData = JsonUtility.FromJson<LevelProgressData>(json);
                return levelProgressData.current_level_number;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing LevelProgress.txt: " + e.Message);
                return -1; 
            }
        }
        else
        {
            Debug.LogError("LevelProgress.txt file not found.");
            return -1; 
        }
    }

    public static void UpdateCurrentLevelNumber(int newLevelNumber)
    {
        LevelProgressData levelProgressData = new LevelProgressData();
        levelProgressData.current_level_number = newLevelNumber;

        string json = JsonUtility.ToJson(levelProgressData);

        File.WriteAllText(levelProgressFilePath, json);
    }


}