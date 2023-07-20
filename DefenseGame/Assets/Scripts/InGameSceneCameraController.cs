using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class InGameSceneCameraController : MonoBehaviour
{
    #region 카메라

    Camera cam;

    float default_camera_size;
    float current_camera_size;

    #endregion

    #region 터치

    [Tooltip("이 시간보다 더 길게 터치가 지속되면 카메라 이동이나 줌인아웃으로 처리")]
    [SerializeField] float maximum_touch_time;

    bool is_it_touch;

    Coroutine check_touch_coroutine;
    WaitForSeconds wait_for_maximum_touch_time;

    #endregion

    #region 함정 배치 메뉴

    [Header("함정 배치 메뉴")]
    public bool is_showing_trap_place_menu;

    [SerializeField] float distance_from_center;

    [SerializeField] GameObject trap_place_menu;
    [SerializeField] GameObject floor_trap_button_container;
    [SerializeField] GameObject wall_trap_button_container;
    [SerializeField] GameObject trap_button_prefab;

    #endregion

    #region 카메라 이동

    Vector2 move_current_position;
    Vector2 move_previous_position;

    //화면의 끝을 터치하고 반대쪽 끝까지 드래그할 때
    //오브젝트들도 화면의 끝에서 끝까지 이동하게 하는 값
    const float camera_move_x_multiplier = -22.222f;
    const float camera_move_y_multiplier = -10f;

    #endregion

    #region 카메라 줌인아웃

    Vector2[] zoom_in_out_previous_position = new Vector2[2];
    Vector2[] zoom_in_out_current_position = new Vector2[2];

    #endregion

    void Start()
    {
        cam = GetComponent<Camera>();
        default_camera_size = cam.orthographicSize;
        current_camera_size = default_camera_size;

        wait_for_maximum_touch_time = new WaitForSeconds(maximum_touch_time);

        is_showing_trap_place_menu = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            InitializeVariables();
            check_touch_coroutine = StartCoroutine(CheckTouch());
        }
        if (Input.GetMouseButton(0))
        {
            if (is_it_touch == false || is_showing_trap_place_menu == false)
            {
                if (Input.touchCount == 1)
                {
                    CameraMove();
                }
                else if (Input.touchCount >= 2)
                {
                    CameraZoomInOut();
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            StopCoroutine(check_touch_coroutine);

            if (is_it_touch == true)
            {
                OnTouch();
            }
        }
    }

    void InitializeVariables()
    {
        move_previous_position = Vector2.zero;
        move_current_position = Vector2.zero;

        zoom_in_out_previous_position[0] = Vector2.zero;
        zoom_in_out_previous_position[1] = Vector2.zero;
        zoom_in_out_current_position[0] = Vector2.zero;
        zoom_in_out_current_position[1] = Vector2.zero;
    }

    IEnumerator CheckTouch()
    {
        is_it_touch = true;

        yield return wait_for_maximum_touch_time;

        is_it_touch = false;
    }

    void CameraMove()
    {
        move_previous_position = move_current_position;
        move_current_position = Input.mousePosition;

        if (move_previous_position == Vector2.zero)
        {
            return;
        }

        Vector2 different = new Vector2(move_current_position.x - move_previous_position.x,
                                        move_current_position.y - move_previous_position.y);

        Vector3 move = new Vector3(different.x / Screen.width * camera_move_x_multiplier + cam.transform.position.x,
                                   different.y / Screen.height * camera_move_y_multiplier + cam.transform.position.y,
                                   -10);

        cam.transform.position = move;
    }

    void CameraZoomInOut()
    {
        zoom_in_out_previous_position[0] = zoom_in_out_current_position[0];
        zoom_in_out_previous_position[1] = zoom_in_out_current_position[1];
        zoom_in_out_current_position[0] = Input.touches[0].position;
        zoom_in_out_current_position[1] = Input.touches[1].position;

        if (zoom_in_out_previous_position[0] == Vector2.zero)
        {
            return;
        }

        float[] different = new float[2];
        different[0] = Vector2.Distance(zoom_in_out_previous_position[0], zoom_in_out_previous_position[1]);
        different[1] = Vector2.Distance(zoom_in_out_current_position[0], zoom_in_out_current_position[1]);

        //손가락 사이의 거리가 작아짐 -> 줌 아웃
        //손가락 사이의 거리가 커짐 -> 줌 인
        current_camera_size *= (different[0] / different[1]);
        cam.orthographicSize = current_camera_size;
    }

    void OnTouch()
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
        Vector2 touched_tile_index = GetIndex(cam.ScreenToWorldPoint(Input.mousePosition));

        if (touched_tile_index.x < 0 || touched_tile_index.x >= InGameSceneManager.instance.map_data[0].Count
         || touched_tile_index.y < 0 || touched_tile_index.y >= InGameSceneManager.instance.map_data.Count)
            return;

        InGameSceneManager.TileType tile_type = InGameSceneManager.instance.map_data[(int)touched_tile_index.y][(int)touched_tile_index.x].tile_type;
        switch (tile_type)
        {
            case InGameSceneManager.TileType.MoveAbleFloor:
            case InGameSceneManager.TileType.Wall:
                SetTrapButtonContainer(tile_type);
                break;
            default:
                return;
        }

        trap_place_menu.SetActive(true);
    }

    void SetTrapButtonContainer(InGameSceneManager.TileType tile_type)
    {
        if (tile_type == InGameSceneManager.TileType.MoveAbleFloor)
        {
            floor_trap_button_container.SetActive(true);
            if (floor_trap_button_container.transform.childCount != InGameSceneManager.instance.trap_place_on_floor_number)
                SetTrapButtons(InGameSceneManager.TileType.MoveAbleFloor);

            return;
        }
        else if (tile_type == InGameSceneManager.TileType.Wall)
        {
            wall_trap_button_container.SetActive(true);

            if (wall_trap_button_container.transform.childCount != InGameSceneManager.instance.trap_place_on_wall_number)
                SetTrapButtons(InGameSceneManager.TileType.Wall);

            return;
        }
    }

    void SetTrapButtons(InGameSceneManager.TileType tile_type)
    {
        int trap_number = 0;

        GameObject trap_button_container = null;
        List<InGameSceneManager.TrapDataSet> trap_datas = null;

        if (tile_type == InGameSceneManager.TileType.MoveAbleFloor)
        {
            trap_number = InGameSceneManager.instance.trap_place_on_floor_number;

            trap_button_container = floor_trap_button_container;
            trap_datas = InGameSceneManager.instance.trap_place_on_floor_prefabs;
        }
        else if (tile_type == InGameSceneManager.TileType.Wall)
        {
            trap_number = InGameSceneManager.instance.trap_place_on_wall_number;

            trap_button_container = wall_trap_button_container;
            trap_datas = InGameSceneManager.instance.trap_place_on_wall_prefabs;
        }
            
        for (int i = 0; i < trap_number; i++)
        {
            GameObject trap_button = Instantiate(trap_button_prefab, Vector3.zero, Quaternion.identity, trap_button_container.transform);
            trap_button.GetComponent<Image>().sprite = trap_datas[i].trap_prefab.GetComponent<SpriteRenderer>().sprite;
        }
    }

    void HideTrapPlaceMenu()
    {
        trap_place_menu.SetActive(false);
        floor_trap_button_container.SetActive(false);
        wall_trap_button_container.SetActive(false);
    }

    Vector2 GetIndex(Vector2 touch_position) //화면 좌표 더해서 화면 움직인 상태에서도 정상 작동 하게
    {
        int x_half_map_size = InGameSceneManager.instance.x_map_size / 2;
        int y_half_map_size = InGameSceneManager.instance.y_map_size / 2;

        Vector2 result = new Vector2();

        if (touch_position.x > 0)
            result.x = (int)touch_position.x + x_half_map_size;
        else
            result.x = (int)touch_position.x + x_half_map_size - 1;

        if (touch_position.y > 0)
            result.y = y_half_map_size - (int)touch_position.y - 1;
        else
            result.y = y_half_map_size - (int)touch_position.y;

        if (InGameSceneManager.instance.x_map_size % 2 != 0)
        {
            if (touch_position.x < 0)
                result.x++;

            if (touch_position.y > 0)
                result.y++;

            result.x--;
            result.y--;
        }
        
        return result;
    }
}