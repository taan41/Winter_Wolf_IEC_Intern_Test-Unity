using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

using Random = UnityEngine.Random;
using IEnumerator = System.Collections.IEnumerator;

public class BoardMahjong
{
    public event Action OnAnimationFinished;

    private int boardSizeX;

    private int boardSizeY;

    private CellMahjong[,] m_cells;

	private CellMahjong[] m_bottomCells;

    private int m_matchMin;

    private Transform m_root;

	public int BottomSize { get; private set; }

    public int BoardItemCount { get; private set; }

    public int BottomItemCount { get; private set; } = 0;

    public BoardMahjong(Transform transform, GameSettings gameSettings, GameManager.eLevelMode levelMode)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.MahjongSizeX;
        this.boardSizeY = gameSettings.MahjongSizeY;
		this.BottomSize = gameSettings.MahjongSizeBottom;

        m_cells = new CellMahjong[boardSizeX, boardSizeY];
		m_bottomCells = new CellMahjong[BottomSize];

        CreateBoard(levelMode == GameManager.eLevelMode.MAHJONG_TIME_ATTACK);
    }
	
    private void CreateBoard(bool allowClickingOnBottom)
    {
        Vector3 boardPos = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.3f + 0.5f, 0f);
		Vector3 bottomPos = new Vector3(-BottomSize * 0.5f + 0.5f, -boardSizeY * 0.5f - 1f, 0f);

        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND_MAHJONG);

        for (int y = 0; y < boardSizeY; y++)
        {
            for (int x = 0; x < boardSizeX; x++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = boardPos + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                CellMahjong cell = go.GetComponent<CellMahjong>();
                cell.Setup(x, y, true, false);

                m_cells[x, y] = cell;
            }
        }

		for (int i = 0; i < BottomSize; i++)
		{
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = bottomPos + new Vector3(i, 0, 0f);
            go.transform.SetParent(m_root);

            CellMahjong cell = go.GetComponent<CellMahjong>();
            cell.Setup(i, 0, allowClickingOnBottom, true);

            m_bottomCells[i] = cell;
		}
    }

    internal void Fill()
    {
        var itemTypes = Utils.NormalItemTypes;

        // fill to biggest multiple of matchMin below board size
        int setCount = boardSizeX * boardSizeY / m_matchMin;
        int setWithExtra = setCount % itemTypes.Length;
        int baseAmount = setCount / itemTypes.Length * m_matchMin;

        BoardItemCount = setCount * m_matchMin;

        // assign amount to each item type
        List<(NormalItem.eNormalType type, int amount)> amountByTypes = new List<(NormalItem.eNormalType type, int amount)>();

        for (int i = 0; i < itemTypes.Length; i++)
        {
            int amount = baseAmount + (setWithExtra > 0 ? m_matchMin : 0);
            setWithExtra--;

            amountByTypes.Add((itemTypes[i], amount));
        }

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int ind = Random.Range(0, amountByTypes.Count);
                int amountAfter = amountByTypes[ind].amount - 1;

                while (amountAfter < 0)
                {
                    amountByTypes.RemoveAt(ind);

                    if (amountByTypes.Count == 0) return;

                    ind = Random.Range(0, amountByTypes.Count);
                    
                    amountAfter = amountByTypes[ind].amount - 1;
                }
                
                NormalItem.eNormalType type = amountByTypes[ind].type;
                amountByTypes[ind] = (type, amountAfter);

                Cell cell = m_cells[x, y];
                NormalItem item = new NormalItem();

                item.SetType(type);
                item.SetView();
                item.SetViewRoot(m_root);
                item.OriginCellPos = new Vector2Int(x, y);

                cell.Assign(item);
                cell.ApplyItemPosition(false);
            }
        }
    }

    internal bool OnCellClicked(CellMahjong cell, Action callback)
    {
        if (cell.IsEmpty || !cell.AllowClicking) return false;

        if (!cell.IsBottom && BottomItemCount < BottomSize)
        {
            NormalItem item = cell.Item as NormalItem;

            cell.Free();

            for (int i = 0; i < BottomSize; i++)
            {
                Cell targetBottomCell = m_bottomCells[i];
                bool assign = false;

                if (targetBottomCell.IsEmpty)
                {
                    assign = true;
                }
                else
                {
                    NormalItem bottomItem = targetBottomCell.Item as NormalItem;

                    // shift higher ordered items to the right
                    if (bottomItem.CompareType(item) > 0)
                    {
                        for (int j = BottomSize - 1; j > i; j--)
                        {
                            if (!m_bottomCells[j - 1].IsEmpty)
                            {
                                Item movingItem = m_bottomCells[j - 1].Item;
                                m_bottomCells[j - 1].Free();

                                m_bottomCells[j].Assign(movingItem);

                                movingItem.View.DOMove(m_bottomCells[j].transform.position, 0.3f);
                            }
                        }

                        assign = true;
                    }
                }

                if (assign)
                {
                    targetBottomCell.Assign(item);

                    BoardItemCount--;
                    BottomItemCount++;

                    item.View.DOMove(targetBottomCell.transform.position, 0.3f).OnComplete(() =>
                    {
                        FindMatchesAndShift(callback);
                    });


                    break;
                }
            }
        }
        else if (cell.IsBottom && cell.AllowClicking)
        {
            // return item from bottom row to its original position
            Item item = cell.Item;
            Cell ogCell = m_cells[item.OriginCellPos.x, item.OriginCellPos.y];

            cell.Free();

            ogCell.Assign(item);
            
            BoardItemCount++;
            BottomItemCount--;

            item.View.DOMove(ogCell.transform.position, 0.3f).OnComplete(() =>
            {
                FindMatchesAndShift(callback);
            });
        }

        return true;
    }

    internal void FindMatchesAndShift(Action callback)
    {
        void Explode(int start, int endExclusive)
        {
            for (int i = start; i < endExclusive; i++)
            {
                if (!m_bottomCells[i].IsEmpty)
                {
                    m_bottomCells[i].Item.ExplodeView();
                    m_bottomCells[i].Free();

                    BottomItemCount--;
                }
            }
        }

        // matching
        NormalItem.eNormalType? lastType = null;
        int matches = 0;

        for (int i = 0; i < BottomSize; i++)
        {
            if (m_bottomCells[i].IsEmpty)
            {
                if (matches >= m_matchMin) Explode(i - matches, i);

                lastType = null;
                matches = 0;
            }
            else
            {
                NormalItem item = m_bottomCells[i].Item as NormalItem;  
                
                if (item.ItemType == lastType)
                {
                    matches++;

                    if (matches >= m_matchMin) Explode(i - matches + 1, i + 1);
                }
                else
                {
                    if (matches >= m_matchMin) Explode(i - matches, i);

                    lastType = item.ItemType;
                    matches = 1;
                }
            }
        }

        // shifting
        int shift = 0;

        for (int i = 0; i < BottomSize; i++)
        {
            if (m_bottomCells[i].IsEmpty)
            {
                shift++;
            }
            else if (shift > 0)
            {
                Item item = m_bottomCells[i].Item;
                
                m_bottomCells[i].Free();

                m_bottomCells[i - shift].Assign(item);

                item.View.DOMove(m_bottomCells[i - shift].transform.position, 0.3f);
            }
        }

        m_bottomCells[0].transform.DOMove(m_bottomCells[0].transform.position, 0.3f).OnComplete(() =>
        {
            callback?.Invoke();
            OnAnimationFinished?.Invoke();
        });
    }
	
    internal void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }
    }

    internal void StartAutoLose()
    {
        IEnumerator AutoLoseProcess()
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                for (int x = 0; x < boardSizeX; x++)
                {
                    if (m_cells == null || m_cells[0, 0] == null) yield break;

                    OnCellClicked(m_cells[x, y], null);

                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        Coroutiner.Instance.Run(AutoLoseProcess());
    }

    internal void StartAutoWin()
    {
        IEnumerator AutoWinProcess()
        {
            NormalItem.eNormalType? typePrio = null;

            if (BottomSize - BottomItemCount <= 2)
            {
                if (m_bottomCells[0].AllowClicking)
                {
                    while (BottomItemCount > 0)
                    {
                        OnCellClicked(m_bottomCells[0], null);

                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else
                {
                    Dictionary<NormalItem.eNormalType, int> typeCounts = new Dictionary<NormalItem.eNormalType, int>();

                    for (int i = 0; i < BottomSize; i++)
                    {
                        if (!m_bottomCells[i].IsEmpty)
                        {
                            NormalItem item = m_bottomCells[i].Item as NormalItem;

                            if (typeCounts.ContainsKey(item.ItemType))
                            {
                                typeCounts[item.ItemType]++;
                            }
                            else
                            {
                                typeCounts[item.ItemType] = 1;
                            }
                        }
                    }

                    int maxCount = 0;

                    foreach (var kvp in typeCounts)
                    {
                        if (kvp.Value > maxCount)
                        {
                            maxCount = kvp.Value;
                            typePrio = kvp.Key;
                        }
                    }
                }
            }

            var itemTypes = Utils.NormalItemTypes;

            typePrio ??= itemTypes[0];

            while (BoardItemCount + BottomItemCount > 0)
            {
                for (int y = 0; y < boardSizeY; y++)
                {
                    for (int x = 0; x < boardSizeX; x++)
                    {
                        if (m_cells == null || m_cells[0, 0] == null) yield break;

                        Cell cell = m_cells[x, y];

                        if (!cell.IsEmpty && (cell.Item as NormalItem).ItemType == typePrio)
                        {
                            OnCellClicked(cell as CellMahjong, null);

                            yield return new WaitForSeconds(0.5f);
                        }
                    }
                }

                typePrio++;

                if (typePrio > itemTypes[itemTypes.Length - 1]) typePrio = itemTypes[0];
            }
        }

        Coroutiner.Instance.Run(AutoWinProcess());
    }
}