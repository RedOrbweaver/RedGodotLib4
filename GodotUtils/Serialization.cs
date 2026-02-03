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
    public static T DeserializeFromFile<T>(string path, bool returnnullonfail = false) where T : class
    {
        Assert(FileExists(path));
        return DeserializeFromString<T>(ReadFile(path));
    }
    public static void SerializeToFile<T>(string path, T o, bool humanreadible = false, bool typedata = true) where T : class
    {
        Assert(o != null);

        var data = SerializeToString<T>(o, humanreadible, typedata);
        WriteFile(path, data);
    }
}
#endif