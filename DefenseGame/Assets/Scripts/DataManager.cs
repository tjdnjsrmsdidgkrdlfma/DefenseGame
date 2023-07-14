using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    #region 맵 정보

    public List<List<int>> map_data;

    #endregion

    #region 적 정보

    public class EnemyDataSet
    {
        public string name;
        public int number;
    }

    public List<List<EnemyDataSet>> enemy_data;

    #endregion

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        BetterStreamingAssets.Initialize();

        map_data = new List<List<int>>();

        enemy_data = new List<List<EnemyDataSet>>();
    }

    public void LoadMapData(string name)
    {
        string[] temp_map_data = BetterStreamingAssets.ReadAllLines(name + "MapData.txt");

        map_data.Clear();

        for (int map_data_y_index = 0; map_data_y_index < temp_map_data.Length; map_data_y_index++)
        {
            map_data.Add(new List<int>());

            int map_data_x_index = 0;
            while (map_data_x_index < temp_map_data[map_data_y_index].Length)
            {
                if (temp_map_data[map_data_y_index][map_data_x_index] == ',')
                {
                    map_data_x_index++;
                    continue;
                }
                else if (temp_map_data[map_data_y_index][map_data_x_index] == ' ')
                {
                    map_data_x_index++;
                    continue;
                }

                map_data[map_data_y_index].Add(int.Parse(temp_map_data[map_data_y_index][map_data_x_index].ToString()));

                map_data_x_index++;
            }
        }
    }

    public void LoadEnemyData(string name)
    {
        string[] temp_enemy_data = BetterStreamingAssets.ReadAllLines(name + "EnemyData.txt");

        enemy_data.Clear();
        enemy_data.Add(new List<EnemyDataSet>());

        int wave_index = 0;
        int enemy_data_y_index = 0;
        while (enemy_data_y_index < temp_enemy_data.Length)
        {
            enemy_data.Add(new List<EnemyDataSet>());

            wave_index++;
            enemy_data_y_index++;

            int enemy_data_x_index = 0;
            while (true)
            {
                if (enemy_data_y_index >= temp_enemy_data.Length)
                    break;

                string[] temp = temp_enemy_data[enemy_data_y_index].Split(',');

                if (temp.Length == 1)
                    break;

                enemy_data[wave_index].Add(new EnemyDataSet());
                enemy_data[wave_index][enemy_data_x_index].name = temp[0];
                enemy_data[wave_index][enemy_data_x_index].number = int.Parse(temp[1]);

                enemy_data_x_index++;
                enemy_data_y_index++;
            }
        }
    }
}
