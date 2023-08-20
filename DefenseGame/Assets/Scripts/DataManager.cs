using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public string current_map_name;

    const string trap_data_file_name = "TrapData.json";
    const string enemy_data_file_name = "EnemyData.txt";

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

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

    public List<List<InGameSceneManager.EnemyDataSet>> LoadEnemyData()
    {
        List<List<InGameSceneManager.EnemyDataSet>> enemy_data = new List<List<InGameSceneManager.EnemyDataSet>>();

        string[] raw_enemy_data = BetterStreamingAssets.ReadAllLines(current_map_name + enemy_data_file_name);

        enemy_data.Add(new List<InGameSceneManager.EnemyDataSet>());

        int wave_index = 0;
        int enemy_data_y_index = 0;
        while (enemy_data_y_index < raw_enemy_data.Length)
        {
            enemy_data.Add(new List<InGameSceneManager.EnemyDataSet>());

            wave_index++;
            enemy_data_y_index++;

            int enemy_data_x_index = 0;
            while (true)
            {
                if (enemy_data_y_index >= raw_enemy_data.Length)
                    break;

                string[] temp = raw_enemy_data[enemy_data_y_index].Split(',');

                if (temp.Length == 1)
                    break;

                enemy_data[wave_index].Add(new InGameSceneManager.EnemyDataSet());
                enemy_data[wave_index][enemy_data_x_index].name = temp[0];
                enemy_data[wave_index][enemy_data_x_index].number = int.Parse(temp[1]);

                enemy_data_x_index++;
                enemy_data_y_index++;
            }
        }

        return enemy_data;
    }
}
