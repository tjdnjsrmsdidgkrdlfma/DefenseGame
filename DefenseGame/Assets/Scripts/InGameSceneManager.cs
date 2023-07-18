using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

public class InGameSceneManager : MonoBehaviour
{
    public static InGameSceneManager instance;

    #region 맵 정보

    public List<List<TileDataSet>> map_data;

    [SerializeField] Transform map_tile_container;

    #endregion

    #region 타일 정보

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

    [SerializeField] TileData tile_data;
    Dictionary<int, TileDataSet> tile_data_dict = new Dictionary<int, TileDataSet>();

    #endregion

    /*#region 함정 정보

    public enum TrapType
    {
        DartTrap
    }

    [Serializable]
    public class TrapData
    {
        public TrapType trap_type;
    }

    public List<List<TrapData>> trap_data;

    #endregion*/

    #region 적 정보

    public class EnemyDataSet
    {
        public string name;
        public int number;
    }

    public List<List<EnemyDataSet>> enemy_data;

    #endregion

    #region 함정 배치 메뉴

    public bool is_showing_trap_place_menu;

    [SerializeField] GameObject trap_place_menu;

    #endregion

    public void OnTouch()
    {
        ReverseTrapPlaceMenuState();
    }

    void ReverseTrapPlaceMenuState()
    {
        is_showing_trap_place_menu = !is_showing_trap_place_menu;

        if (is_showing_trap_place_menu == true)
        {
            ShowTrapPlaceMenu();
        }
        else
        {
            HideTrapPlaceMenu();
        }
    }

    void ShowTrapPlaceMenu()
    {
        trap_place_menu.SetActive(true);
    }

    void HideTrapPlaceMenu()
    {
        trap_place_menu.SetActive(false);
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        is_showing_trap_place_menu = false;
        ControlData();
        DrawMap();
    }

    void ControlData()
    {
        //SaveTileData();
        LoadTileData();
        LoadMapData();
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

    void LoadMapData()
    {
        map_data = DataManager.instance.LoadMapData();
        MapDataLoadTileData();
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
