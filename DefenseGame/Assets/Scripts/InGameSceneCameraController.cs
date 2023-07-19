using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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

    [SerializeField] GameObject trap_place_menu;

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
        trap_place_menu.SetActive(true);
    }

    void HideTrapPlaceMenu()
    {
        trap_place_menu.SetActive(false);
    }
}