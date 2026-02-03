using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

[Tool]
public partial class UtilsRunner : Node
{
    public static UtilsRunner GUR;
    public const UInt64 TICKS_PER_SECOND = 1000 * 1000 * 1000;
    public UInt64 GlobalTime { get; private set; } = 0;
    public UInt64 PhysicsTime { get; private set; } = 0;
    public double CurrentTimeUSEC => (double)(GlobalTime / (TICKS_PER_SECOND / 1000 / 10)) / 100.0;
    public double CurrentTimeMSEC => CurrentTimeUSEC * 1000;
    public double CurrentTimeSEC => CurrentTimeMSEC * 1000;
    public double CurrentPhysicsTimeSEC => PhysicsTime / (TICKS_PER_SECOND / 1000000);
    public double CurrentPhysicsTimeMSEC => CurrentPhysicsTimeSEC / 1000.0;
    public double CurrentPhysicsTimeUSEC => CurrentPhysicsTimeMSEC / 1000.0;
    List<UTimer> _timers = new List<UTimer>();
    ConcurrentQueue<Action> _toExec = new ConcurrentQueue<Action>();
    void DequeDeferred()
    {
        System.Action action;
        Assert(_toExec.TryDequeue(out action), "Error: queue desynchronization");
        action();
    }
    public void QueueDeferred(System.Action action)
    {
        Assert(action != null);
        _toExec.Enqueue(action);
        CallDeferred(nameof(DequeDeferred));
    }
    Node _currentSceneRoot;
    public void SetMainScene(Node node)
    {
        var root = GetTree().Root;
        if (_currentSceneRoot != null)
        {
            Assert(_currentSceneRoot.GetParent() == root);
            DestroyNode(_currentSceneRoot);
        }
        _currentSceneRoot = node;
        QueueDeferred(() => GetTree().Root.AddChild(_currentSceneRoot));
    }
    void ProcessTimers()
    {
        List<UTimer> ended = new List<UTimer>(_timers.Count / 2);
        foreach (var timer in _timers)
        {
            if (timer.IsRunning && ((timer.StartTime + timer.Delay) <= GlobalTime))
            {
                timer.Trigger();
                if (timer.IsOver)
                    ended.Add(timer);
            }
        }
        lock (_timers)
        {
            foreach (var e in ended)
                _timers.Remove(e);
        }
    }
    public void RemoveTimer(UTimer timer)
    {
        lock (_timers)
        {
            _timers.Remove(timer);
        }
    }
    public UTimer AddTimer(UTimer timer, bool start = false)
    {
        if (start)
            timer.Start(false);
        lock (_timers)
        {
            _timers.Add(timer);
        }
        return timer;
    }
    public void AddTimerIfNotAdded(UTimer timer)
    {
        lock (_timers)
        {
            if (!_timers.Contains(timer))
                _timers.Add(timer);
        }
    }
    public UTimer AddTimer(UInt64 span, Action onhit, bool repeating = false, bool start = false)
    {
        return AddTimer(new UTimer(span, onhit, repeating), start);
    }
    public UTimer AddTimer(TimeSpan span, Action onhit, bool repeating = false, bool start = false)
    {
        return AddTimer(new UTimer(span, onhit, repeating), start);
    }
    public UTimer AddTimer(double delay_seconds, Action onhit, bool repeating = false, bool start = false)
    {
        return AddTimer(new UTimer(delay_seconds, onhit, repeating), start);
    }
    public UTimer StartTimer(UInt64 span, Action onhit, bool repeating = false)
    {
        return AddTimer(new UTimer(span, onhit, repeating), true);
    }
    public UTimer StartTimer(TimeSpan span, Action onhit, bool repeating = false, bool start = false)
    {
        return AddTimer(new UTimer(span, onhit, repeating), true);
    }
    public UTimer StartTimer(double delay_seconds, Action onhit, bool repeating = false, bool start = false)
    {
        return AddTimer(new UTimer(delay_seconds, onhit, repeating), true);
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
        GlobalTime += (UInt64)(delta * TICKS_PER_SECOND);
        ProcessTimers();
    }
    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        PhysicsTime += (UInt64)(delta * TICKS_PER_SECOND);
    }
    public override void _Ready()
    {
        base._Ready();
        GUR = this;
    }
    public UtilsRunner()
    {
        GUR = this;
    }

}