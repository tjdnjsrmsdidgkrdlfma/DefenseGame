using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class InGameSceneManager : MonoBehaviour
{
    public static InGameSceneManager instance;

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
    }
}
