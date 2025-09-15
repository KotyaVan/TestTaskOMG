using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerStateService : MonoBehaviour
{
    private const string UserDataKey = "UserState";
    
    public void SaveFieldDataToPlayerPrefs(FieldData fieldData)
    {
        // Сериализуем FieldData в строку JSON
        string json = JsonConvert.SerializeObject(fieldData, Formatting.Indented);

        // Сохраняем строку JSON в PlayerPrefs
        PlayerPrefs.SetString(UserDataKey, json);

        // Применяем изменения в PlayerPrefs
        PlayerPrefs.Save();
    }

    // Метод для загрузки данных поля из PlayerPrefs
    public FieldData LoadFieldDataFromPlayerPrefs()
    {
        // Проверяем, существует ли сохранённая строка в PlayerPrefs
        if (PlayerPrefs.HasKey(UserDataKey))
        {
            // Читаем строку JSON из PlayerPrefs
            string json = PlayerPrefs.GetString(UserDataKey);

            // Десериализуем строку JSON обратно в FieldData
            FieldData fieldData = JsonConvert.DeserializeObject<FieldData>(json);

            return fieldData;
        }
        else
        {
            Debug.LogWarning("No field data found in PlayerPrefs.");
            return null; // Или возвращаем пустое поле, если данных нет
        }
    }
}