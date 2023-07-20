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
        public GameObject tile_sprite;
    }

    [Header("타일")]
    [SerializeField] TileData tile_data;

    Dictionary<int, TileDataSet> tile_data_dict = new Dictionary<int, TileDataSet>();

    #endregion

    #region 맵

    [Header("맵")]
    public int x_map_size;
    public int y_map_size;

    [SerializeField] Transform map_tile_container;

    List<List<TileDataSet>> map_data;

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
        public TrapType trap_type;
        public TrapPlaceType trap_place_type;
    }

    [Header("함정")]
    public int number_of_trap_place_on_floor;
    public int number_of_trap_place_on_wall;

    [SerializeField] TrapData trap_data;

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
        number_of_trap_place_on_floor = 0;
        number_of_trap_place_on_wall = 0;

        for (int i = 0; i < Enum.GetNames(typeof(TrapType)).Length; i++)
        {
            if (trap_data.trap_data[i].trap_place_type == TrapPlaceType.Floor)
                number_of_trap_place_on_floor++;
            else if (trap_data.trap_data[i].trap_place_type == TrapPlaceType.Wall)
                number_of_trap_place_on_wall++;
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
                Instantiate(map_data[y][x].tile_sprite, top_left_position + new Vector3(x, -y, 0), Quaternion.identity, map_tile_container);
            }
        }
    }
}
