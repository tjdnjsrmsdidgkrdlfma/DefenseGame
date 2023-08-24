using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public string current_map_name;

    const string trap_data_file_name = "TrapData.json";
    const string enemy_data_file_name = "EnemyData.json";
    const string wave_data_file_name = "WaveData.csv";

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        BetterStreamingAssets.Initialize();
    }

    public void SaveTrapData(InGameSceneManager.TrapData trap_data)
    {
        string path = Application.streamingAssetsPath + "\\" + trap_data_file_name;
        string json_data = JsonUtility.ToJson(trap_data, true);

        if (File.Exists(path) == false)
        {
            File.Create(path).Close();
        }

        File.WriteAllText(path, json_data);
    }

    public InGameSceneManager.TrapData LoadTrapData()
    {
        string path = trap_data_file_name;
        string json_data = BetterStreamingAssets.ReadAllText(path);
        InGameSceneManager.TrapData tile_data = JsonUtility.FromJson<InGameSceneManager.TrapData>(json_data);

        return tile_data;
    }

    public void SaveEnemyData(EnemyManager.EnemyData enemy_data)
    {
        string path = Application.streamingAssetsPath + "\\" + enemy_data_file_name;
        string json_data = JsonUtility.ToJson(enemy_data, true);

        if (File.Exists(path) == false)
        {
            File.Create(path).Close();
        }

        File.WriteAllText(path, json_data);
    }

    public EnemyManager.EnemyData LoadEnemyData()
    {
        string path = enemy_data_file_name;
        string json_data = BetterStreamingAssets.ReadAllText(path);
        EnemyManager.EnemyData enemy_data = JsonUtility.FromJson<EnemyManager.EnemyData>(json_data);

        return enemy_data;
    }

    public EnemyManager.WaveData[] LoadWaveData()
    {
        string path = current_map_name + wave_data_file_name;

        List<EnemyManager.WaveData> wave_data = new List<EnemyManager.WaveData>();

        string[] raw_data = BetterStreamingAssets.ReadAllLines(path);

        for (int i = 1; i < raw_data.Length; i++)
        {
            raw_data[i] = RemoveSpace(raw_data[i]);
            string[] splited_line_data = raw_data[i].Split(',');
            wave_data.Add(new EnemyManager.WaveData(splited_line_data));
        }

        return wave_data.ToArray();
    }

    string RemoveSpace(string old_string)
    {
        return string.Concat(old_string.Where(c => !char.IsWhiteSpace(c)));
    }
}
