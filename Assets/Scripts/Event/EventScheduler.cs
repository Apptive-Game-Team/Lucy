using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameEvent
{
    START = 0,
    FLOOR_SECOND_BEFORE_MEET_NPC = 1,
    FLOOR_SECOND_AFTER_MEET_NPC = 2,
    FLOOR_SECOND_START_PUZZLE = 3,
    FLOOR_SECOND_END_PUZZLE = 4,
    FLOOR_THIRD_START_MAZE = 5,
    FLOOR_THIRD_END_MAZE = 6,
}

public interface IGameEvent
{
    void Execute();
}

public static class GameEventFactory
{
    private static readonly Dictionary<GameEvent, Func<IGameEvent>> _eventCreators = new Dictionary<GameEvent, Func<IGameEvent>>();

    static GameEventFactory()
    {
        RegisterEvent(GameEvent.START, () => new StartGameEvent());
        RegisterEvent(GameEvent.FLOOR_SECOND_BEFORE_MEET_NPC, () => new FirstMeetNpcGameEvent());
    }

    public static void RegisterEvent(GameEvent gameEvent, Func<IGameEvent> creator)
    {
        _eventCreators[gameEvent] = creator;
    }

    public static IGameEvent CreateEvent(GameEvent gameEvent)
    {
        if (_eventCreators.TryGetValue(gameEvent, out Func<IGameEvent> creator))
        {
            return creator();
        }
        return null;
    }
}

public class StartGameEvent : IGameEvent
{
    public void Execute()
    {
        EventScheduler.Instance.UpdateGameEvent();
    }
}

public class FirstMeetNpcGameEvent : IGameEvent
{
    public void Execute()
    {
        EventScheduler.Instance.eventObjects["FirstMeetNpcEventObject"].PlaySound();
    }
}

public class AfterNpcGameEvent : IGameEvent
{
    public void Execute()
    {
        EventScheduler.Instance.eventObjects["FirstMeetNpcEventObject"].StopSound();
    }
}

public class EventScheduler : SingletonObject<EventScheduler>
{
    [SerializeField]
    private GameEvent currentGameEvent;

    public Dictionary<string, EventObject> eventObjects = new Dictionary<string, EventObject>();

    private GameEvent LoadCurrentEvent()
    {
        // Save Load System is Required
        return GameEvent.START;
    }
    

    void Start()
    {
        currentGameEvent = LoadCurrentEvent();
    }

    public void UpdateGameEvent()
    {
        currentGameEvent = (GameEvent)(currentGameEvent + 1);
        //GameEventFactory.CreateEvent(currentGameEvent).Execute();

    }
}
