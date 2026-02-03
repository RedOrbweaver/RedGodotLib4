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
    public static void Destroy(this Node node)
    {
        DestroyNode(node);
    }
    public static bool HasChild(this Node node, string name)
    {
        return node.GetChildren<Node>().Any(n => n.Name == name);
    }
    public static Node GetChild(this Node node, string name)
    {
        Assert(node.HasChild(name));
        return node.GetChildren<Node>().Find(n => n.Name == name);
    }
    public static List<Node> GetChildren(this Node node, string name)
    {
        return node.GetChildren<Node>().FindAll(n => n.Name == name);
    }
    public static Node RemoveChild(this Node node, string name)
    {
        Assert(node.HasChild(name));
        Node n = node.GetChild(name);
        node.RemoveChild(n);
        return n;
    }
    public static void DestroyChild(this Node node, string name)
    {
        Assert(node.HasChild(name));
        Node n = node.GetChild(name);

        node.RemoveChild(n);
        DestroyNode(n);
    }
    public static List<T> GetChildren<T>(this Node root) where T : Node
    {
        List<T> ret = new List<T>(8);
        foreach(var it in root.GetChildren())
        {
            if(it is T t)
                ret.Add(t);
        }
        return ret;
    }
    public static List<T> GetChildrenRecrusively<T>(this Node root, bool includeinternal = false, bool restrichtomatchingparents = false) where T : Godot.Node
    {
        List<T> ret = new List<T>(8);

        void SearchRecursively(Node n)
        {
            foreach (var it in n.GetChildren(includeinternal))
            {
                Assert(it is Node);
                if (it is T t)
                {
                    ret.Add(t);
                }
                if (!restrichtomatchingparents || it is T)
                {
                    SearchRecursively((Node)it);
                }
            }
        }
        SearchRecursively(root);

        return ret;
    }

    public static T FindChild<T>(this Node parent, string pattern, bool recursive = false, bool owned = true) where T : Node
    {
        var children = parent.FindChildren(pattern, typeof(T).FullName, recursive, owned);
        Assert(children.Count > 0);
        return (T)children[0];
    }
    public static List<T> FindChildrenRecursively<T>(this Node parent, string pattern) where T : Node
    {
        return parent.FindChildren(pattern, typeof(T).FullName, true, true).Convert<T, Node>();
    }
    public static List<T> FindChildren<T>(this Node parent, string pattern, bool recursive = false) where T : Node
    {   
        if(recursive)
            return FindChildrenRecursively<T>(parent, pattern);
        return parent.FindChildren(pattern, typeof(T).FullName, false).Convert<T, Node>();
    }
    public static void Delay(float seconds, Action act)
    {
        UtilsRunner.GUR.AddTimer(seconds, act, false, true);
    }
    public static bool HasParent(this Node node)
    {
        return node.GetParent() != null;
    }
    public static bool LoseParent(this Node node)
    {
        if(node.GetParent() is Node parent)
        {
            parent.RemoveChild(node);
            return true;
        }
        return false;
    }
    public static void StealChild(this Node node, Node other)
    {
        if(other.GetParent() is Node parent)
            parent.RemoveChild(other);
        node.AddChild(other);
    }

}
#endif