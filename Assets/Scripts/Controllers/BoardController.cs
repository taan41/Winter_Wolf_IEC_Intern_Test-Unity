using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static GameManager;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private BoardMahjong m_mahjong;

    private GameManager m_gameManager;

    private eLevelMode m_level;

    private bool m_isDragging;

    private Camera m_cam;

    private Collider2D m_hitCollider;

    private GameSettings m_gameSettings;

    private List<Cell> m_potentialMatch;

    private float m_timeAfterFill;

    private bool m_hintIsShown;

    private bool m_gameOver;

    public BoardMahjong MahjongBoard => m_mahjong;

    public void StartGame(GameManager gameManager, GameSettings gameSettings, eLevelMode levelMode)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_level = levelMode;

        m_cam = Camera.main;

        switch (m_level)
        {
            case eLevelMode.MATCH_3_TIMER:
            case eLevelMode.MATCH_3_MOVES:
                m_board = new Board(this.transform, gameSettings);
                FillMatch3();
                break;
            
            case eLevelMode.MAHJONG_FULL_CLEAR:
            case eLevelMode.MAHJONG_TIME_ATTACK:
                m_mahjong = new BoardMahjong(this.transform, gameSettings, levelMode);
                m_mahjong.OnAnimationFinished += OnMoveEvent;
                FillMahjong();
                break;
        }
    }

    private void FillMatch3()
    {
        m_board.Fill();
        FindMatchesAndCollapse();
    }

    private void FillMahjong()
    {
        m_mahjong.Fill();
    }

    private void OnGameStateChange(eStateGame state)
    {
        switch (state)
        {
            case eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case eStateGame.PAUSE:
                IsBusy = true;
                break;
            case eStateGame.GAME_OVER:
                m_gameOver = true;
                StopHints();
                break;
        }
    }

    public void ManualUpdate()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (IsMatch3() && !m_hintIsShown)
        {
            m_timeAfterFill += Time.deltaTime;
            if (m_timeAfterFill > m_gameSettings.TimeForHint)
            {
                m_timeAfterFill = 0f;
                ShowHint();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                m_isDragging = true;
                m_hitCollider = hit.collider;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (IsMahjong() && m_hitCollider != null)
            {
                if (m_hitCollider.TryGetComponent<CellMahjong>(out var cell))
                {
                    IsBusy = true;

                    m_mahjong.OnCellClicked(cell, () =>
                    {
                        IsBusy = false;
                    });
                }
            }
            
            ResetRayCast();
        }

        if (IsMatch3() && Input.GetMouseButton(0) && m_isDragging)
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                if (m_hitCollider != null && m_hitCollider != hit.collider)
                {
                    StopHints();

                    Cell c1 = m_hitCollider.GetComponent<Cell>();
                    Cell c2 = hit.collider.GetComponent<Cell>();
                    if (AreItemsNeighbor(c1, c2))
                    {
                        IsBusy = true;
                        SetSortingLayer(c1, c2);
                        m_board.Swap(c1, c2, () =>
                        {
                            FindMatchesAndCollapse(c1, c2);
                        });

                        ResetRayCast();
                    }
                }
            }
            else
            {
                ResetRayCast();
            }
        }
    }

    private void ResetRayCast()
    {
        m_isDragging = false;
        m_hitCollider = null;
    }

    private void FindMatchesAndCollapse(Cell cell1, Cell cell2)
    {
        if (cell1.Item is BonusItem)
        {
            cell1.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else if (cell2.Item is BonusItem)
        {
            cell2.ExplodeItem();
            StartCoroutine(ShiftDownItemsCoroutine());
        }
        else
        {
            List<Cell> cells1 = GetMatches(cell1);
            List<Cell> cells2 = GetMatches(cell2);

            List<Cell> matches = new List<Cell>();
            matches.AddRange(cells1);
            matches.AddRange(cells2);
            matches = matches.Distinct().ToList();

            if (matches.Count < m_gameSettings.MatchesMin)
            {
                m_board.Swap(cell1, cell2, () =>
                {
                    IsBusy = false;
                });
            }
            else
            {
                OnMoveEvent();

                CollapseMatches(matches, cell2);
            }
        }
    }

    private void FindMatchesAndCollapse()
    {
        List<Cell> matches = m_board.FindFirstMatch();

        if (matches.Count > 0)
        {
            CollapseMatches(matches, null);
        }
        else
        {
            m_potentialMatch = m_board.GetPotentialMatches();
            if (m_potentialMatch.Count > 0)
            {
                IsBusy = false;

                m_timeAfterFill = 0f;
            }
            else
            {
                //StartCoroutine(RefillBoardCoroutine());
                StartCoroutine(ShuffleBoardCoroutine());
            }
        }
    }

    private List<Cell> GetMatches(Cell cell)
    {
        List<Cell> listHor = m_board.GetHorizontalMatches(cell);
        if (listHor.Count < m_gameSettings.MatchesMin)
        {
            listHor.Clear();
        }

        List<Cell> listVert = m_board.GetVerticalMatches(cell);
        if (listVert.Count < m_gameSettings.MatchesMin)
        {
            listVert.Clear();
        }

        return listHor.Concat(listVert).Distinct().ToList();
    }

    private void CollapseMatches(List<Cell> matches, Cell cellEnd)
    {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ExplodeItem();
        }

        if(matches.Count > m_gameSettings.MatchesMin)
        {
            m_board.ConvertNormalToBonus(matches, cellEnd);
        }

        StartCoroutine(ShiftDownItemsCoroutine());
    }

    private IEnumerator ShiftDownItemsCoroutine()
    {
        m_board.ShiftDownItems();

        yield return new WaitForSeconds(0.2f);

        m_board.FillGapsWithNewItems();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator RefillBoardCoroutine()
    {
        m_board.ExplodeAllItems();

        yield return new WaitForSeconds(0.2f);

        m_board.Fill();

        yield return new WaitForSeconds(0.2f);

        FindMatchesAndCollapse();
    }

    private IEnumerator ShuffleBoardCoroutine()
    {
        m_board.Shuffle();

        yield return new WaitForSeconds(0.3f);

        FindMatchesAndCollapse();
    }


    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        if (IsMatch3()) m_board.Clear();
        if (IsMahjong()) m_mahjong.Clear();
    }

    private void ShowHint()
    {
        if (!IsMatch3()) return;

        m_hintIsShown = true;
        foreach (var cell in m_potentialMatch)
        {
            cell.AnimateItemForHint();
        }
    }

    private void StopHints()
    {
        if (!IsMatch3()) return;

        m_hintIsShown = false;
        foreach (var cell in m_potentialMatch)
        {
            cell.StopHintAnimation();
        }

        m_potentialMatch.Clear();
    }

    private bool IsMatch3() => m_level == eLevelMode.MATCH_3_MOVES || m_level == eLevelMode.MATCH_3_TIMER;

    private bool IsMahjong() => m_level == eLevelMode.MAHJONG_FULL_CLEAR || m_level == eLevelMode.MAHJONG_TIME_ATTACK;
}
