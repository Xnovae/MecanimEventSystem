using UnityEngine;
using System.Collections.Generic;

public enum MecanimEventParamTypes
{
    None,
    Int32,
    Float,
    String,
    Boolean,
}

public enum EventConditionParamTypes
{
    Int,
    Float,
    Boolean,
}

public enum EventConditionModes
{
    Equal = 0,
    NotEqual = 1,
    GreaterThan = 2,
    LessThan = 3,
    GreaterEqualThan = 4,
    LessEqualThan = 5,
}

[System.Serializable]
public class MecanimEvent
{
    public static EventContext Context { protected set; get; }

    public MecanimEvent()
    {
        condition = new EventCondition();
    }

    public MecanimEvent(MecanimEvent other)
    {
        normalizedTime = other.normalizedTime;
        functionName = other.functionName;
        paramType = other.paramType;
        skillType = other.skillType;

        switch (paramType)
        {
            case MecanimEventParamTypes.Int32:
                intParam = other.intParam;
                break;
            case MecanimEventParamTypes.Float:
                floatParam = other.floatParam;
                break;
            case MecanimEventParamTypes.String:
                stringParam = other.stringParam;
                break;
            case MecanimEventParamTypes.Boolean:
                boolParam = other.boolParam;
                break;
        }

        condition = new EventCondition();
        condition.conditions = new List<EventConditionEntry>(other.condition.conditions);

        critical = other.critical;

        isEnable = other.isEnable;
    }

    public string functionName;
    public float normalizedTime;
    public MecanimEventParamTypes paramType;
    public SkillActionType skillType;

    public object parameter
    {
        get
        {
            switch (paramType)
            {
                case MecanimEventParamTypes.Int32:
                    return intParam;
                case MecanimEventParamTypes.Float:
                    return floatParam;
                case MecanimEventParamTypes.String:
                    return stringParam;
                case MecanimEventParamTypes.Boolean:
                    return boolParam;
                default:
                    return null;
            }
        }
    }

    public int intParam;
    public float floatParam;
    public string stringParam;
    public bool boolParam;

    public EventCondition condition;
    public bool critical = false;

    public bool isEnable = true;

    private EventContext context;

    public void SetContext(EventContext context)
    {
        this.context = context;
        this.context.current = this;
    }

    public static void SetCurrentContext(MecanimEvent e)
    {
        MecanimEvent.Context = e.context;
    }
}

[System.Serializable]
public class EventCondition
{
    public List<EventConditionEntry> conditions = new List<EventConditionEntry>();

    public bool Test(Animator animator)
    {
        if (conditions.Count == 0)
            return true;

        foreach (EventConditionEntry entry in conditions)
        {
            if (string.IsNullOrEmpty(entry.conditionParam))
                continue;

            switch (entry.conditionParamType)
            {
                case EventConditionParamTypes.Int:
                    int intTestValue = animator.GetInteger(entry.conditionParam);
                    switch (entry.conditionMode)
                    {
                        case EventConditionModes.Equal:
                            if (intTestValue != entry.intValue)
                                return false;

                            break;
                        case EventConditionModes.NotEqual:
                            if (intTestValue == entry.intValue)
                                return false;

                            break;
                        case EventConditionModes.GreaterThan:
                            if (intTestValue <= entry.intValue)
                                return false;

                            break;
                        case EventConditionModes.LessThan:
                            if (intTestValue >= entry.intValue)
                                return false;

                            break;
                        case EventConditionModes.GreaterEqualThan:
                            if (intTestValue < entry.intValue)
                                return false;

                            break;
                        case EventConditionModes.LessEqualThan:
                            if (intTestValue > entry.intValue)
                                return false;

                            break;
                    }
                    break;

                case EventConditionParamTypes.Float:
                    float floatTestValue = animator.GetFloat(entry.conditionParam);

                    switch (entry.conditionMode)
                    {
                        case EventConditionModes.GreaterThan:
                            if (floatTestValue <= entry.floatValue)
                                return false;

                            break;
                        case EventConditionModes.LessThan:
                            if (floatTestValue >= entry.floatValue)
                                return false;

                            break;
                    }

                    break;

                case EventConditionParamTypes.Boolean:
                    bool boolTestValue = animator.GetBool(entry.conditionParam);

                    if (boolTestValue != entry.boolValue)
                        return false;

                    break;
            }
        }

        return true;
    }
}

[System.Serializable]
public class EventConditionEntry
{
    public string conditionParam;
    public EventConditionParamTypes conditionParamType;
    public EventConditionModes conditionMode;
    public float floatValue;
    public int intValue;
    public bool boolValue;
}

public struct EventContext
{
    public int controllerId;
    public int layer;
    public int stateHash;
    public int tagHash;
    public MecanimEvent current;
}

/// <summary>
/// Skill type of players and monsters
/// </summary>
public enum SkillActionType
{
    None = 0,           
    GreyScreen = 1,         // 灰屏
    CameraMotion = 2,       // 移动相机
    ChangeTimeScale = 3,    // 改变时间，慢镜头/快镜头
    CharacterMotion = 4,    // 人物运动，规划一条人物运动的曲线，这样打斗起来人物上蹿下跳、辗转腾挪。
    Debuffs = 5,            // 伤害减免
    Buffs,                  // 伤害增益
    HitedEvent,             // 受击事件，人物受击，则触发此事件，在此事件中，做各种受击需要做的处理。
    NormalEffect,           // 释放特效，支持挂在任意地方，播放时跟随、脱离等等。
    PlayAudio,              // 播放音效
    ShakeCamera,            // 抖屏
    ShootEvent,             // 射击事件（针对远程角色，射手/法师）
    SkillEndControl,        // 技能结束事件
    SprintEvent,            // 冲刺技能，冲刺？这和Character Motion有什么区别呢？因为不论朝哪个方向冲刺，冲刺过程中是带攻击判定的，更加消耗效率，所以做了区分。
    TriggerSequence,        // 在技能释放过程中，可以触发一套演示，CG动画等
    WeaponAttach,           // 加武器，不是添加在手上，而是添加在指定点，这样可以做出头上插了把刀的效果。
    WeaponShower,           // 显示或隐藏武器，比如将武器扔出武器，这时需要隐藏武器，再将武器从怪物身上拔下，这时需要显示武器。
    ChangeSpeed,            // 改变动画速度，比如加速角色动作
    Movement,               // 改变角色位置，瞬移、冲刺、击退、浮空、击飞等
}