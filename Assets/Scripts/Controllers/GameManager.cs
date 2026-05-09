using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };
    
    public enum eLevelMode
    {
        MATCH_3_TIMER,
        MATCH_3_MOVES,
        MAHJONG_FULL_CLEAR,
        MAHJONG_TIME_ATTACK,
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
        GAME_WIN,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }


    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.ManualUpdate();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if(State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode levelMode)
    {
        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, levelMode);

        switch (levelMode)
        {
            case eLevelMode.MATCH_3_TIMER:
                m_levelCondition = this.gameObject.AddComponent<LevelMatch3Time>();
                m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), this);
                break;

            case eLevelMode.MATCH_3_MOVES:
                m_levelCondition = this.gameObject.AddComponent<LevelMatch3Moves>();
                m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
                break;
            
            case eLevelMode.MAHJONG_FULL_CLEAR:
                m_levelCondition = this.gameObject.AddComponent<LevelMahjongFullClear>();
                m_levelCondition.Setup(m_gameSettings.LevelMoves, m_uiMenu.GetLevelConditionView(), m_boardController);
                break;
            
            case eLevelMode.MAHJONG_TIME_ATTACK:
                m_levelCondition = this.gameObject.AddComponent<LevelMahjongTime>();
                ((LevelMahjongTime)m_levelCondition).Setup(m_gameSettings.MahjongTimeAttackDuration, m_uiMenu.GetLevelConditionView(), this, m_boardController);
                break;
        }

        m_levelCondition.ConditionCompleteEvent += GameWin;
        m_levelCondition.ConditionFailedEvent += GameOver;

        State = eStateGame.GAME_STARTED;
    }

    public void GameWin()
    {
        StartCoroutine(WaitBoardController(true));
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController(false));
    }

    internal void ClearLevel()
    {
        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }
    }

    private IEnumerator WaitBoardController(bool won)
    {
        while (m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = won ? eStateGame.GAME_WIN : eStateGame.GAME_OVER;

        if (m_levelCondition != null)
        {
            m_levelCondition.ConditionCompleteEvent -= GameWin;

            Destroy(m_levelCondition);
            m_levelCondition = null;
        }
    }

    public void StartMahjongAutoPlay(bool tryWin)
    {
        if (tryWin)
        {
            m_boardController.MahjongBoard?.StartAutoWin();
        }
        else
        {
            m_boardController.MahjongBoard?.StartAutoLose();
        }
    }
}
