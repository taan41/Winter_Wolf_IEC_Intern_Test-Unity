using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelMahjongTime : LevelCondition
{
    private float m_time;

    private GameManager m_mngr;

	private BoardMahjong m_mahjong;

	private int m_boardItem, m_bottomItem, m_bottomSize;

    public void Setup(float value, Text txt, GameManager mngr, BoardController boardController)
    {
        base.Setup(value, txt, boardController);

        m_mngr = mngr;

        m_time = value;

		m_mahjong = boardController.MahjongBoard;
		m_mahjong.OnAnimationFinished += OnMoveFinished;

		m_bottomSize = m_mahjong.BottomSize;
		m_boardItem = m_mahjong.BoardItemCount;
		m_bottomItem = m_mahjong.BottomItemCount;

        UpdateText();
    }

	private void OnMoveFinished()
	{
		if (m_levelFinished) return;

		m_boardItem = m_mahjong.BoardItemCount;
		m_bottomItem = m_mahjong.BottomItemCount;

		UpdateText();

		if (m_boardItem == 0 && m_bottomItem == 0)
		{
			OnConditionComplete();
		}
	}

    private void Update()
    {
        if (m_levelFinished) return;

        if (m_mngr.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        UpdateText();

        if (m_time <= -1f && m_boardItem + m_bottomItem > 0)
        {
            OnConditionFailed();
        }
    }

    protected override void UpdateText()
    {
        if (m_time < 0f) return;

        m_txt.text = string.Format("TIME:\n{0:00}", m_time);
    }
}
