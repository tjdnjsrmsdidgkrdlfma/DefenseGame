using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class InGameSceneManager : MonoBehaviour
{
    public static InGameSceneManager instance;

    #region 타일

    public enum TileType
    {
        MoveAbleFloor = 0,
        UnMoveAbleFloor = 64,
        Wall = 128,
        StartPoint = 254,
        EndPoint = 255
    }

    [Serializable]
    public class TileData
    {
        public List<TileDataSet> tile_data;
    }

    [Serializable]
    public class TileDataSet
    {
        public int tile_id;

        public TileType tile_type;

        public GameObject tile_prefab;
    }

    [Header("타일")]
    [SerializeField] TileData tile_data;

    Dictionary<int, TileDataSet> tile_data_dict = new Dictionary<int, TileDataSet>();

    #endregion

    #region 맵

    [Header("맵")]
    public int x_map_size;
    public int y_map_size;

    public List<List<TileDataSet>> map_data;

    [SerializeField] Transform map_tile_container;

    #endregion

    #region 함정

    public enum TrapType
    {
        DartTrap
    }

    public enum TrapPlaceType
    {
        Floor,
        Wall
    }

    [Serializable]
    public class TrapData
    {
        public List<TrapDataSet> trap_data;
    }

    [Serializable]
    public class TrapDataSet
    {
        public int trap_id;

        public TrapType trap_type;
        public TrapPlaceType trap_place_type;

        public GameObject trap_prefab;
    }

    [Header("함정")]
    public int trap_place_on_floor_number;
    public int trap_place_on_wall_number;

    public List<TrapDataSet> trap_place_on_floor_prefabs;
    public List<TrapDataSet> trap_place_on_wall_prefabs;

    [SerializeField] TrapData trap_data; //위의 두 변수와 중복되는 부분이 너무 많음 위의 두 변수는 id와 프리팹 정도만 가지고 있으면 될 듯

    #endregion

    #region 함정 맵

    List<List<TrapDataSet>> trap_map_data;

    #endregion

    #region 적

    public class EnemyDataSet
    {
        public string name;
        public int number;
    }

    List<List<EnemyDataSet>> enemy_data;

    #endregion

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ControlData();
        DrawMap();
    }

    void ControlData()
    {
        SaveTileData();
        LoadTileData();

        LoadMapData();

        SaveTrapData();
        LoadTrapData();

        LoadEnemyData();
    }

    void SaveTileData()
    {
        DataManager.instance.SaveTileData(tile_data);
    }

    void LoadTileData()
    {
        tile_data = DataManager.instance.LoadTileData();
        DictionarizeTileData();
    }

    void DictionarizeTileData()
    {
        for (int i = 0; i < tile_data.tile_data.Count; i++)
        {
            tile_data_dict.Add(tile_data.tile_data[i].tile_id, tile_data.tile_data[i]);
        }
    }

    void SaveTrapData()
    {
        DataManager.instance.SaveTrapData(trap_data);
    }

    void LoadTrapData()
    {
        trap_data = DataManager.instance.LoadTrapData();
        InitializeTrapVariables();
    }

    void InitializeTrapVariables()
    {
        trap_place_on_floor_number = 0;
        trap_place_on_wall_number = 0;

        for (int i = 0; i < Enum.GetNames(typeof(TrapType)).Length; i++)
        {
            if (trap_data.trap_data[i].trap_place_type == TrapPlaceType.Floor)
            {
                trap_place_on_floor_prefabs.Add(trap_data.trap_data[i]);
                trap_place_on_floor_number++;
            }
            else if (trap_data.trap_data[i].trap_place_type == TrapPlaceType.Wall)
            {
                trap_place_on_wall_prefabs.Add(trap_data.trap_data[i]);
                trap_place_on_wall_number++;
            }
                
        }
    }

    void LoadMapData()
    {
        map_data = DataManager.instance.LoadMapData();
        MapDataLoadTileData();
        x_map_size = map_data[0].Count;
        y_map_size = map_data.Count;
    }

    void MapDataLoadTileData()
    {
        for (int i = 0; i < map_data.Count; i++)
        {
            for (int j = 0; j < map_data[i].Count; j++)
            {
                map_data[i][j] = tile_data_dict[map_data[i][j].tile_id];
            }
        }
    }

    void LoadEnemyData()
    {
        enemy_data = DataManager.instance.LoadEnemyData();
    }

    void DrawMap()
    {
        int map_x_size = map_data[0].Count;
        int map_y_size = map_data.Count;

        Vector3 top_left_position = new Vector3((map_x_size / 2 - 0.5f) * -1, 
                                                map_y_size / 2 - 0.5f, 
                                                0);

        for (int y = 0; y < map_y_size; y++)
        {
            for (int x = 0; x < map_x_size; x++)
            {
                Instantiate(map_data[y][x].tile_prefab, top_left_position + new Vector3(x, -y, 0), Quaternion.identity, map_tile_container);
            }
        }
    }
}
