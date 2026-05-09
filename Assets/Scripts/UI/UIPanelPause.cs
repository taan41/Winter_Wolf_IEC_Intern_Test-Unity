using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelPause : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    [SerializeField] private Button btnAutoWin;

    [SerializeField] private Button btnAutoLose;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
        btnAutoWin.onClick.AddListener(OnClickClose);
        btnAutoLose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
        if (btnAutoWin) btnAutoWin.onClick.RemoveAllListeners();
        if (btnAutoLose) btnAutoLose.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickClose()
    {
        m_mngr.ShowGameMenu();
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
