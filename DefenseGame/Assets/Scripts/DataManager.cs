using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InGameSceneManager;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    public string current_map_name;

    const string tile_data_file_name = "TileData.json";
    const string map_data_file_name = "MapData.txt";
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

    public void SaveTileData(TileData tile_data)
    {
        string path = Application.streamingAssetsPath + "\\" + tile_data_file_name;
        string json_data = JsonUtility.ToJson(tile_data, true);

        if (File.Exists(path) == false)
        {
            File.Create(path).Close();
        }

        File.WriteAllText(path, json_data);
    }

    public TileData LoadTileData()
    {
        string path = tile_data_file_name;
        string json_data = BetterStreamingAssets.ReadAllText(path);
        TileData tile_data = JsonUtility.FromJson<TileData>(json_data);

        return tile_data;
    }

    public List<List<TileDataSet>> LoadMapData()
    {
        List<List<TileDataSet>> map_data = new List<List<TileDataSet>>();

        string[] raw_map_data = BetterStreamingAssets.ReadAllLines(current_map_name + map_data_file_name);

        for (int map_data_y_index = 0; map_data_y_index < raw_map_data.Length; map_data_y_index++)
        {
            map_data.Add(new List<TileDataSet>());

            int map_data_x_index = 0;
            while (map_data_x_index < raw_map_data[map_data_y_index].Length)
            {
                if (raw_map_data[map_data_y_index][map_data_x_index] == ',')
                {
                    map_data_x_index++;
                    continue;
                }
                else if (raw_map_data[map_data_y_index][map_data_x_index] == ' ')
                {
                    map_data_x_index++;
                    continue;
                }

                map_data[map_data_y_index].Add(new TileDataSet());
                map_data[map_data_y_index][map_data[map_data_y_index].Count - 1].tile_id = int.Parse(raw_map_data[map_data_y_index][map_data_x_index].ToString());

                map_data_x_index++;
            }
        }

        return map_data;
    }

    public void SaveTrapData(TrapData trap_data)
    {
        string path = Application.streamingAssetsPath + "\\" + trap_data_file_name;
        string json_data = JsonUtility.ToJson(trap_data, true);

        if (File.Exists(path) == false)
        {
            File.Create(path).Close();
        }

        File.WriteAllText(path, json_data);
    }

    public TrapData LoadTrapData()
    {
        string path = trap_data_file_name;
        string json_data = BetterStreamingAssets.ReadAllText(path);
        TrapData tile_data = JsonUtility.FromJson<TrapData>(json_data);

        return tile_data;
    }

    public List<List<EnemyDataSet>> LoadEnemyData()
    {
        List<List<EnemyDataSet>> enemy_data = new List<List<EnemyDataSet>>();

        string[] raw_enemy_data = BetterStreamingAssets.ReadAllLines(current_map_name + enemy_data_file_name);

        enemy_data.Add(new List<EnemyDataSet>());

        int wave_index = 0;
        int enemy_data_y_index = 0;
        while (enemy_data_y_index < raw_enemy_data.Length)
        {
            enemy_data.Add(new List<EnemyDataSet>());

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

                enemy_data[wave_index].Add(new EnemyDataSet());
                enemy_data[wave_index][enemy_data_x_index].name = temp[0];
                enemy_data[wave_index][enemy_data_x_index].number = int.Parse(temp[1]);

                enemy_data_x_index++;
                enemy_data_y_index++;
            }
        }

        return enemy_data;
    }
}
