using UnityEngine.UI;

public class LevelMahjongFullClear : LevelCondition
{
	private BoardController m_boardController;

	private BoardMahjong m_mahjong;

	private int m_boardItem, m_bottomItem, m_bottomSize;

	public override void Setup(float value, Text txt, BoardController board)
	{
		base.Setup(value, txt);

		m_boardController = board;

		m_mahjong = m_boardController.MahjongBoard;
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

		if (m_bottomItem == m_bottomSize)
		{
			OnConditionFailed();
		}
	}

	protected override void UpdateText()
	{
		m_txt.text = $"TILES LEFT:\n{m_boardItem}";
	}

	protected override void OnDestroy()
	{
		if (m_boardController != null) m_boardController.OnMoveEvent -= OnMoveFinished;

		base.OnDestroy();
	}
}