using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    public event Action ConditionCompleteEvent = delegate { };
    public event Action ConditionFailedEvent = delegate { };

    protected Text m_txt;

    protected bool m_levelFinished = false;

    public virtual void Setup(float value, Text txt)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, GameManager mngr)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, BoardController board)
    {
        m_txt = txt;
    }

    protected virtual void UpdateText() { }

    protected void OnConditionComplete()
    {
        m_levelFinished = true;

        ConditionCompleteEvent();
    }

    protected void OnConditionFailed()
    {
        m_levelFinished = true;

        ConditionFailedEvent();
    }

    protected virtual void OnDestroy()
    {

    }
}
