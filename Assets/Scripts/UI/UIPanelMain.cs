using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnMatch3Timer;

    [SerializeField] private Button btnMatch3Moves;

    [SerializeField] private Button bthMahjongClear;

    [SerializeField] private Button bthMahjongTime;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnMatch3Moves?.onClick.AddListener(OnClickMatch3Moves);
        btnMatch3Timer?.onClick.AddListener(OnClickMatch3Timer);
        bthMahjongClear?.onClick.AddListener(OnClickMahjongClear);
        bthMahjongTime?.onClick.AddListener(OnClickMahjongTime);
    }

    private void OnDestroy()
    {
        if (btnMatch3Moves) btnMatch3Moves.onClick.RemoveAllListeners();
        if (btnMatch3Timer) btnMatch3Timer.onClick.RemoveAllListeners();
        if (bthMahjongClear) bthMahjongClear.onClick.RemoveAllListeners();
        if (bthMahjongTime) bthMahjongTime.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickMatch3Timer()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.MATCH_3_TIMER);
    }

    private void OnClickMatch3Moves()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.MATCH_3_MOVES);
    }

    private void OnClickMahjongClear()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.MAHJONG_FULL_CLEAR);
    }

    private void OnClickMahjongTime()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.MAHJONG_TIME_ATTACK);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
