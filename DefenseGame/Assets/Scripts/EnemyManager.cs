using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    #region 적

    [Serializable]
    public class EnemyData
    {
        public List<EnemyDataSet> enemy_data;
    }

    [Serializable]
    public class EnemyDataSet
    {
        public int enemy_id;
        public string enemy_name;

        public GameObject enemy_prefab;
    }

    [Header("적")]
    [SerializeField] EnemyData enemy_data;

    Dictionary<int, EnemyDataSet> enemy_id_2_enemy_data = new Dictionary<int, EnemyDataSet>();

    #endregion

    #region 웨이브

    [Serializable]
    public class WaveData
    {
        public WaveData(string[] splited_line_data)
        {
            index = int.Parse(splited_line_data[0]);
            wave = int.Parse(splited_line_data[1]);
            enemy_id = int.Parse(splited_line_data[2]);
            enemy_count = int.Parse(splited_line_data[3]);
            time_between_enemy = float.Parse(splited_line_data[4]);
        }

        public int index;
        public int wave;
        public int enemy_id;
        public int enemy_count;
        public float time_between_enemy;
    }

    [Header("웨이브")]
    [SerializeField] float time_between_wave;

    WaveData[] wave_data;

    #endregion

    #region 적 오브젝트 풀

    //[Header("적 오브젝트 풀")]
    List<List<GameObject>> object_pool_objects = new List<List<GameObject>>();
    Dictionary<int, List<GameObject>> enemy_id_2_object_pool = new Dictionary<int, List<GameObject>>();

    #endregion

    void Start()
    {
#if UNITY_EDITOR
        DataManager.instance.SaveEnemyData(enemy_data);
#endif
        enemy_data = DataManager.instance.LoadEnemyData();
        InitializeEnemyVariables();

        wave_data = DataManager.instance.LoadWaveData();

        InitializeEnemyObjectPool();
        StartCoroutine(SpawnEnemyLoop());
    }

    void InitializeEnemyVariables()
    {
        for (int i = 0; i < enemy_data.enemy_data.Count; i++)
        {
            enemy_id_2_enemy_data.Add(enemy_data.enemy_data[i].enemy_id, enemy_data.enemy_data[i]);
        }
    }

    void InitializeEnemyObjectPool()
    {
        for (int i = 0; i < enemy_data.enemy_data.Count; i++)
        {
            GameObject enemy_container = new GameObject(enemy_data.enemy_data[i].enemy_name);
            enemy_container.transform.parent = transform;

            object_pool_objects.Add(new List<GameObject>());
            enemy_id_2_object_pool.Add(enemy_data.enemy_data[i].enemy_id, object_pool_objects[i]);
        }
    }

    IEnumerator SpawnEnemyLoop()
    {
        int current_wave;
        int last_wave = wave_data[wave_data.Length - 1].wave;
        int wave_data_index = 0;

        WaitForSeconds time_between_wave = new WaitForSeconds(this.time_between_wave);

        for (current_wave = 1; current_wave <= last_wave; current_wave++)
        {
            int target_wave_data_index = wave_data_index;
            while (target_wave_data_index < wave_data.Length && wave_data[target_wave_data_index].wave == current_wave)
                target_wave_data_index++;

            for (; wave_data_index < target_wave_data_index; wave_data_index++)
            {
                WaitForSeconds time_between_enemy = new WaitForSeconds(wave_data[wave_data_index].time_between_enemy);

                for (int i = 0; i < wave_data[wave_data_index].enemy_count; i++)
                {
                    GameObject enemy_object = GetObject(wave_data[wave_data_index].enemy_id);
                    enemy_object.SetActive(true);

                    yield return time_between_enemy;
                }
            }

            yield return time_between_wave;
        }
    }

    GameObject GetObject(int enemy_id)
    {
        List<GameObject> object_list = enemy_id_2_object_pool[enemy_id];
        for (int i = 0; i < object_list.Count; i++)
        {
            if (object_list[i].activeSelf == false)
                return object_list[i];
        }

        return CreateNewObject(enemy_id);
    }

    GameObject CreateNewObject(int enemy_id)
    {
        EnemyDataSet enemy_data = enemy_id_2_enemy_data[enemy_id];
        Transform object_container = transform.Find(enemy_data.enemy_name);
        List<GameObject> object_list = enemy_id_2_object_pool[enemy_id];

        GameObject created_object = Instantiate(enemy_data.enemy_prefab, object_container);
        object_list.Add(created_object);

        return created_object;
    }
}
