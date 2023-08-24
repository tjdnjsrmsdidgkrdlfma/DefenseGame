using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class InGameSceneManager : MonoBehaviour
{
    public static InGameSceneManager instance;
    Camera cam;
    CameraManager camera_controller;

    #region 맵

    public enum TileType
    {
        MoveAbleFloor,
        UnMoveAbleFloor,
        Wall,
        StartPoint,
        EndPoint
    }

    [Header("맵")]
    [SerializeField] Transform grid;

    int map_tile_data_half_x_length; //맵 타일 개수가 세로일 때는 안 될수도
    int map_tile_data_half_y_length;
    TileType[,] map_tile_data;

    #endregion

    #region 함정

    public enum TrapTileType
    {
        MoveAbleFloor = TileType.MoveAbleFloor,
        Wall = TileType.Wall
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
        public string trap_name;

        public TrapTileType trap_tile_type;

        public GameObject trap_prefab;
    }

    [Header("함정")]
    [SerializeField] TrapData trap_data;

    List<int> trap_indexes_place_on_move_able_floor = new List<int>();
    List<int> trap_indexes_place_on_wall = new List<int>();
    TrapTileType[,] map_trap_data;

    #endregion

    #region 함정 메뉴

    [Header("함정 메뉴")]
    public bool showing_trap_menu;

    [SerializeField] float distance_between_center_and_button;
    [SerializeField] float time_to_set_trap_menu;
    [SerializeField] float[] time_scale;

    [SerializeField] Color[] background_color;
    [SerializeField] Vector3[] button_scale;

    [SerializeField] GameObject trap_button_prefab;
    [SerializeField] GameObject trap_menu;
    [SerializeField] GameObject trap_menu_background;
    [SerializeField] Transform move_able_floor_trap_button_container;
    [SerializeField] Transform wall_trap_button_container;

    bool setting_trap_menu;

    GameObject last_touched_button_container;

    #endregion

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cam = FindObjectOfType<Camera>();
        camera_controller = FindObjectOfType<CameraManager>();
        camera_controller.OnTouch += ReverseTrapPlaceMenuState;

        ControlData();
        GetTileData();
    }

    void ControlData()
    {
#if UNITY_EDITOR
        DataManager.instance.SaveTrapData(trap_data);
#endif
        trap_data = DataManager.instance.LoadTrapData();
        InitializeTrapVariables();
    }

    void InitializeTrapVariables()
    {
        for (int i = 0; i < trap_data.trap_data.Count; i++)
        {
            if (trap_data.trap_data[i].trap_tile_type == TrapTileType.MoveAbleFloor)
            {
                trap_indexes_place_on_move_able_floor.Add(i);
            }
            else if (trap_data.trap_data[i].trap_tile_type == TrapTileType.Wall)
            {
                trap_indexes_place_on_wall.Add(i);
            }
        }
    }

    void GetTileData()
    {
        List<Tilemap> tilemaps = new List<Tilemap>();
        for (int i = 0; i < grid.childCount; i++)
        {
            tilemaps.Add(grid.GetChild(i).GetComponent<Tilemap>());
        }

        BoundsInt bounds = new BoundsInt();
        for (int i = 0; i < tilemaps.Count; i++)
        {
            if (bounds.size.x + bounds.size.y < tilemaps[i].cellBounds.size.x + tilemaps[i].cellBounds.size.y)
                bounds = tilemaps[i].cellBounds;
        }

        map_tile_data = new TileType[bounds.size.y, bounds.size.x];
        map_tile_data_half_x_length = map_tile_data.GetLength(1) / 2;
        map_tile_data_half_y_length = map_tile_data.GetLength(0) / 2;
        map_trap_data = new TrapTileType[bounds.size.y, bounds.size.x];

        for (int i = 0; i < tilemaps.Count; i++)
        {
            TileBase[] tile_bases = tilemaps[i].GetTilesBlock(bounds);
            TileType tilemap_tile_type = (TileType)Enum.Parse(typeof(TileType), tilemaps[i].tag);

            for (int y = 0; y < bounds.size.y; y++)
            {
                for (int x = 0; x < bounds.size.x; x++)
                {
                    TileBase tile_base = tile_bases[x + y * bounds.size.x];

                    if (tile_base != null)
                    {
                        map_tile_data[y, x] = tilemap_tile_type;
                    }
                }
            }
        }
    }

    void Update()
    {
        /*Vector3Int index = tilemaps[0].WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        tilemaps[0].SetTile(index, null);
        Debug.Log(index);*/
    }

    void ReverseTrapPlaceMenuState()
    {
        if (setting_trap_menu == true)
            return;

        showing_trap_menu = !showing_trap_menu;

        if (showing_trap_menu == true)
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
        if (CheckTileType(Enum.GetValues(typeof(TrapTileType)).Cast<TileType>().ToArray()) == false)
        {
            showing_trap_menu = false;
            return;
        }

        TileType tile_type = GetTileTypeFromTouchPosition();
        SetTrapButtons(tile_type);
        StartCoroutine(FadeInOutTrapMenu(tile_type, true));
    }

    bool CheckTileType(TileType[] tile_type_to_check)
    {
        for (int i = 0; i < grid.childCount; i++)
        {
            Tilemap tilemap = grid.GetChild(i).GetComponent<Tilemap>();

            if (tile_type_to_check.Contains((TileType)Enum.Parse(typeof(TileType), tilemap.tag)) == false)
                continue;

            if (tilemap.HasTile(tilemap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition))) == true)
            {
                return true;
            }
        }

        return false;
    }

    TileType GetTileTypeFromTouchPosition()
    {
        Vector3Int tile_index = grid.GetChild(0).GetComponent<Tilemap>().WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
        TileType tile_type = map_tile_data[tile_index.y + map_tile_data_half_y_length, tile_index.x + map_tile_data_half_x_length];
        return tile_type;
    }

    void SetTrapButtons(TileType tile_type)
    {
        switch (tile_type)
        {
            case (TileType)TrapTileType.MoveAbleFloor:
                if (move_able_floor_trap_button_container.childCount == trap_indexes_place_on_move_able_floor.Count)
                    return;
                break;
            case (TileType)TrapTileType.Wall:
                if (wall_trap_button_container.childCount == trap_indexes_place_on_wall.Count)
                    return;
                break;
        }

        int trap_number = 0;

        List<int> trap_indexes = null;

        Transform trap_button_container = null;

        if (tile_type == TileType.MoveAbleFloor)
        {
            trap_number = trap_indexes_place_on_move_able_floor.Count;

            trap_indexes = trap_indexes_place_on_move_able_floor;

            trap_button_container = move_able_floor_trap_button_container;
        }
        else if (tile_type == TileType.Wall)
        {
            trap_number = trap_indexes_place_on_wall.Count;

            trap_indexes = trap_indexes_place_on_wall;

            trap_button_container = wall_trap_button_container;
        }

        float angle_between_buttons = 360 * Mathf.Deg2Rad / trap_number;
        for (int i = 0; i < trap_number; i++)
        {
            Vector3 button_position = new Vector3(Mathf.Cos(angle_between_buttons * i) * distance_between_center_and_button,
                                                  Mathf.Sin(angle_between_buttons * i) * distance_between_center_and_button,
                                                  0);
            GameObject trap_button = Instantiate(trap_button_prefab, Vector3.zero, Quaternion.identity, trap_button_container.transform);
            trap_button.GetComponent<RectTransform>().anchoredPosition = button_position;
            trap_button.GetComponent<Image>().sprite = trap_data.trap_data[trap_indexes[i]].trap_prefab.GetComponent<SpriteRenderer>().sprite;
        }
    }

    IEnumerator FadeInOutTrapMenu(TileType tile_type, bool fade_in)
    {
        setting_trap_menu = true;

        GameObject trap_button_container = null;
        
        switch (tile_type)
        {
            case TileType.MoveAbleFloor:
                trap_button_container = move_able_floor_trap_button_container.gameObject;
                break;
            case TileType.Wall:
                trap_button_container = wall_trap_button_container.gameObject;
                break;
        }

        if (fade_in == true)
        {
            trap_menu.SetActive(true);
            trap_button_container.SetActive(true);
            last_touched_button_container = trap_button_container;
        }

        Image background_image = trap_menu_background.GetComponent<Image>();
        RectTransform[] trap_buttons = trap_button_container.GetComponentsInChildren<RectTransform>();

        float time = 0;
        while (time < time_to_set_trap_menu)
        {
            time += Time.deltaTime;

            if (time > time_to_set_trap_menu)
                time = time_to_set_trap_menu;

            float t = time / time_to_set_trap_menu;

            if (fade_in == true)
            {
                Time.timeScale = Mathf.Lerp(time_scale[0], time_scale[1], t);

                background_image.color = Color.Lerp(background_color[0], background_color[1], t);

                Vector3 temp_scale = Vector3.Lerp(button_scale[0], button_scale[1], t);
                for (int i = 0; i < trap_buttons.Length; i++)
                {
                    trap_buttons[i].localScale = temp_scale;
                }
            }
            else
            {
                Time.timeScale = Mathf.Lerp(time_scale[1], time_scale[0], t);

                background_image.color = Color.Lerp(background_color[1], background_color[0], t);

                Vector3 temp_scale = Vector3.Lerp(button_scale[1], button_scale[0], t);
                for (int i = 0; i < trap_buttons.Length; i++)
                {
                    trap_buttons[i].localScale = temp_scale;
                }
            }

            yield return null;
        }

        if (fade_in == false)
        {
            trap_menu.SetActive(false);
            last_touched_button_container.SetActive(false);
        }
            
        setting_trap_menu = false;
    }

    void HideTrapPlaceMenu()
    {
        TileType tile_type = GetTileTypeFromTouchPosition();

        StartCoroutine(FadeInOutTrapMenu(tile_type, false));
    }
}