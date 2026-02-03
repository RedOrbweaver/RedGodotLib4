using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Expression = System.Linq.Expressions.Expression;
using System.Threading.Tasks;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#if GODOT
using Godot;
public static partial class Utils
{
    public static void Assert(Godot.Error b, string msg, [CallerLineNumber] int sourceLineNumber = 0,
        [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
    {
        if (b == Error.Ok)
            return;
        DebugAssert(true, $"ASSERTION FAILURE (Error.{b.ToString()}): \n" + msg, sourceFilePath, sourceLineNumber, memberName);
    }
    public static void Assert(Godot.Error b, [CallerLineNumber] int sourceLineNumber = 0,
        [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
    {
        Assert(b, $"An unspecified assertion failure, error code {b.ToString()}, has been triggered!", sourceLineNumber, memberName, sourceFilePath);
    }
    public static void DestroyNode(Node node)
    {
        if (node.GetParent() is Node parent)
            parent.RemoveChild(node);
        node.QueueFree();
    }
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class LoadFrom : System.Attribute
    {
        static int _registered = 0;
        public string Path;
        public static List<(string path, Action<Resource> onend)> _toLoad;
        public static List<(string path, Action<Resource> onend)> ToLoad
        {
            get
            {
                if(_toLoad == null || _toLoad.Count != _registered)
                {
                    _toLoad = new List<(string path, Action<Resource> onend)>();
                    var props = FindAllPropertiesWithAttribute<LoadFrom>();
                    foreach(var it in props)
                    {
                        var attr = it.GetCustomAttribute<LoadFrom>();
                        Assert(attr != null);
                        var path = attr.Path;
                        var setter = it.GetSetMethod(true);
                        Assert(setter.IsStatic, "All LoadFrom subscribers should be static!");
                        Action<Resource> act = res => setter.Invoke(null, new object[]{res});
                        _toLoad.Add((path, act));
                    }
                    _registered = _toLoad.Count;
                }
                return _toLoad;
            }
        }
        public LoadFrom(string path)
        {
            Assert(FileExists(path), "could not find resource: " + path);
            this.Path = path;
            _registered++;
        }
    }
    public static void SetMainScene(Node node)
    {
        UtilsRunner.GUR.SetMainScene(node); 
    }
    public static Node SetMainScene(PackedScene scene)
    {
        var node = scene.Instantiate();
        Assert(node != null);
        SetMainScene(node);
        return node;
    }
    public static Node SetMainScene(string dir)
    {
        var scene = ResourceLoader.Load<PackedScene>(dir);
        Assert(scene != null);
        return SetMainScene(scene);
    }
    public static T SetMainScene<T>(PackedScene scene) where T : Node
    {
        return (T)SetMainScene(scene);
    }
    public static T SetMainScene<T>(string dir) where T : Node
    {
        return (T)SetMainScene(dir);
    }

    public static void CallDeferred(Action action)
    {
        if(UtilsRunner.GUR == null && Engine.IsEditorHint())
        {
            GD.PrintErr("Missed in-editor action!: " + action.ToString());
            return;
        }
        Assert(UtilsRunner.GUR != null, "GUS is null");
        UtilsRunner.GUR.QueueDeferred(action);
    }

    public static List<T> Convert<T, [MustBeVariant]E>(this Godot.Collections.Array<E> array, Func<Node, T> converter) where T : Node where E : Node
    {
        List<T> ret = new List<T>(array.Count);
        foreach (Node n in array)
        {
            ret.Add(converter(n));
        }
        return ret;
    }
    public static List<T> Convert<T, [MustBeVariant]E>(this Godot.Collections.Array<E> array) where T : Node where E : Node
    {
        List<T> ret = new List<T>(array.Count);
        foreach (Node n in array)
        {
            ret.Add((T)n);
        }
        return ret;
    }
    public static void Defer(Action action) => CallDeferred(action);
    public class UTimer
	{
		public UInt64 Delay;
		public UInt64 StartTime {get; private set;}
		public bool IsRunning {get; private set;}

		public Action OnHit;
		public bool Repeating = false;
		public bool IsOver {get; private set;}
		public void Trigger()
		{
			if(Repeating)
			{
				StartTime = UtilsRunner.GUR.GlobalTime;
			}
			else
				IsOver = true;
			OnHit();
		}
		public void Start(bool pushtoGUS = true)
		{
			IsRunning = true;
			StartTime = UtilsRunner.GUR.GlobalTime;
			if(pushtoGUS)
				UtilsRunner.GUR.AddTimerIfNotAdded(this);
		}
		public void Stop()
		{
			IsRunning = false;
		}
		public UTimer(double delay_seconds, Action onhit, bool repeating = false)
		{
			Delay = (UInt64)(delay_seconds * (double)UtilsRunner.TICKS_PER_SECOND);
			OnHit = onhit;
			Repeating = repeating;
		}
		public UTimer(UInt64 delay, Action onhit, bool repeating = false)
		{
			Delay = delay;
			OnHit = onhit;
			Repeating = repeating;
		}

		public UTimer(TimeSpan span, Action onhit, bool repeating = false)
		{
			Delay = (UInt64)((double)span.Ticks / (double)TimeSpan.TicksPerSecond * (double)UtilsRunner.TICKS_PER_SECOND);
			OnHit = onhit;
			Repeating = repeating;
		}
	}    
}
#endif