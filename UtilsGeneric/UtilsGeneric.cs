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

public static partial class Utils
{
    public static void RepeatN(int n, Action act)
	{
		for (int i = 0; i < n; i++)
		{
			act();
		}
	}
	public static void RepeatN(int n, Action<int> act)
	{
		for (int i = 0; i < n; i++)
		{
			act(i);
		}
	}
	public static List<T> RepeatN<T>(int n, Func<int, T> act)
	{
		var ret = new List<T>();
		for (int i = 0; i < n; i++)
		{
			ret.Add(act(i));
		}
		return ret;
	}
	public static T EnumNext<T>(T e) where T : Enum
	{
		var l = (T[])Enum.GetValues(typeof(T));
		bool f = false;
		foreach (var it in l)
		{
			if (f)
				return (T)it;
			if (it.Equals(e))
			{
				f = true;
			}
		}
		return l[0];
	}
	public static T EnumPrev<T>(T e) where T : Enum
	{
		var l =(T[]) Enum.GetValues(typeof(T));
		T prev = l[l.Length-1];
		foreach(var it in l)
		{
			if(it.Equals(e))
				return prev;
			prev = (T)it;
		}
		throw new Exception("Invalid enum value");
	}

	// Python-style division remainder, for instance: AbsMod(-1, 4) == 3
	public static int AbsMod(int v, int d)
	{
		return (v % d + d) % d;
	}
	public static long AbsMod(long v, long d)
	{
		return (v % d + d) % d;
	}
	[System.Serializable]
	public class AssertionFailureException : System.Exception
	{
		public AssertionFailureException() { }
		public AssertionFailureException(string message) : base(message) { }
		public AssertionFailureException(string message, System.Exception inner) : base(message, inner) { }
		protected AssertionFailureException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
	[Conditional("DEBUG")]
	static void DebugAssert(bool b, string msg, string sourceFilePath, int sourceLineNumber, string memberName)
	{
		if (b)
			return;
		string topmsg = $"ASSERTION FAILURE ({sourceFilePath}:{sourceLineNumber}->{memberName}):";
#if GODOT
		GD.PrintErr(topmsg);
		GD.PrintErr(msg);
#else
		Console.WriteLine(topmsg);
		Console.WriteLine(msg);
#endif
		//Debugger.Break();
		throw new AssertionFailureException(topmsg + " " + msg);
	}
	public static void Assert(bool b, string msg, 
		[CallerLineNumber] int sourceLineNumber = 0, [CallerMemberName] string memberName = "", 
		[CallerFilePath] string sourceFilePath = "")
	{
		DebugAssert(b, msg, sourceFilePath, sourceLineNumber, memberName);
	}
	public static void Assert(bool b, [CallerLineNumber] int sourceLineNumber = 0,
		[CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
	{
		Assert(b, "An unspecified assertion failure has been triggered!", sourceLineNumber, memberName, sourceFilePath);
	}
	public static void AssertNot(bool b, string msg)
	{
		Assert(!b, msg);
	}
	public static void AssertNot(bool b)
	{
		Assert(!b);
	}
	public static void Assert<T>(bool b, string msg) where T : Exception
	{
		if (!b)
			throw (Exception)GetActivator<T>()(msg);
	}
	public static void Assert<T>(bool b) where T : Exception
	{
		if (!b)
			throw (Exception)GetActivator<T>()();
	}
	static uint ID = 0;
	public static uint CreateID()
	{
		return ++ID;
	}
	public static uint LastID()
	{
		return ID - 1;
	}
	public delegate object ObjectActivator(params object[] args);
	public static ObjectActivator GetActivator<T>(ConstructorInfo ctor)
	{
		Type type = ctor.DeclaringType;
		ParameterInfo[] paramsInfo = ctor.GetParameters();

		//create a single param of type object[]
		ParameterExpression param =
			Expression.Parameter(typeof(object[]), "args");

		Expression[] argsExp =
			new Expression[paramsInfo.Length];

		//pick each arg from the params array 
		//and create a typed expression of them
		for (int i = 0; i < paramsInfo.Length; i++)
		{
			Expression index = Expression.Constant(i);
			Type paramType = paramsInfo[i].ParameterType;

			Expression paramAccessorExp =
				Expression.ArrayIndex(param, index);

			Expression paramCastExp =
				Expression.Convert(paramAccessorExp, paramType);

			argsExp[i] = paramCastExp;
		}

		//make a NewExpression that calls the
		//ctor with the args we just created
		NewExpression newExp = Expression.New(ctor, argsExp);

		//create a lambda with the New
		//Expression as body and our param object[] as arg
		LambdaExpression lambda =
			Expression.Lambda(typeof(ObjectActivator), newExp, param);

		//compile it
		ObjectActivator compiled = (ObjectActivator)lambda.Compile();
		return compiled;
	}
	public static ObjectActivator GetActivator<T>()
	{
		return GetActivator<T>(typeof(T).GetConstructors().First());
	}
	public static List<PropertyInfo> FindAllPropertiesWithAttribute<T>() where T : Attribute
	{
		List<PropertyInfo> ret = new List<PropertyInfo>();
		foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
		{
			// Check each method for the attribute.
			var props = type.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).ToList();
			//props.AddRange(type.GetRuntimeProperties());
			foreach (var property in props)
			{
				if(property.GetCustomAttribute<T>() != null)
					ret.Add(property);
			}
		}
		return ret;
	}
	// Returns a value copy of v.
	public static T CloneStruct<T>(T v) where T : struct => v;
	// Returns a value copy of v.
	public static T Clone<T>(this T v) where T : struct => CloneStruct(v);
	public static async void QueueUserWorkItemAsync(Action act)
	{
		SemaphoreSlim ss = new SemaphoreSlim(0, 1);
		ThreadPool.QueueUserWorkItem(o => 
		{
			act();
			ss.Release();
		});
		await ss.WaitAsync();
	}
	public static async Task<T> QueueUserWorkItemAsync<T>(Func<T> func)
	{
		SemaphoreSlim ss = new SemaphoreSlim(0, 1);
		T ret = default(T);
		ThreadPool.QueueUserWorkItem(o => 
		{
			ret = func();
			ss.Release();
		});
		await ss.WaitAsync();
		return ret;
	}

	public static bool PropertyHasAttribute(object o, string property_name, Type type)
	{
		Assert(o.GetType().GetProperty(property_name) != null, "object does not have a property \"" + property_name + "\"");
		return o.GetType().GetProperty(property_name).CustomAttributes.Any(ca => ca.AttributeType == type);
	}
	public static bool ObjectHasAttribute(object o, Type type)
	{
		return o.GetType().GetCustomAttributesData().Any(at => at.AttributeType == type);
	}

	public interface ISelectPropertyChanged : INotifyPropertyChanged
	{
		public void OnPropertyChanged(string property_name, PropertyChangedEventHandler handler)
		{
			Assert(this.GetType().GetProperty(property_name) != null, $"Property {property_name} does not exist for type {this.GetType().Name}");
			PropertyChanged += (o, e) => 
			{
				if(e.PropertyName == property_name)
					handler(o, e);
			};
		}
		public void OnPropertyChanged<T>(string property_name, Action<T> handler)
		{
			PropertyInfo prop = this.GetType().GetProperty(property_name);
			Assert(prop != null, $"Property {property_name} does not exist for type {this.GetType().Name}");
			Assert(prop.GetType() == typeof(T), $"Handler does not match property type {property_name} for type {this.GetType().Name}");
			PropertyChanged += (o, e) => 
			{
				if(e.PropertyName == property_name)
					handler((T)prop.GetValue(o));
			};
		}
	}

	public static T[] Subset<T>(this T[] array, int start, int end)
	{
		Assert(start >= 0 && start < end && end-start <= array.Length);
		return array.Skip(start).Take(end-start).ToArray();
	}
	public static T[] GetRange<T>(this T[] array, int start, int end)
	{
		return array.Subset(start, end);
	}
	public static void ForEach<T>(this T[] array, Action<T> f)
	{
		foreach(var v in array)
		{
			f(v);
		}
	}
	public static RT[] ForEach<T, RT>(this T[] array, Func<T, RT> f)
	{
		RT[] rt = new RT[array.Length];
		for(int i = 0; i < array.Length; i++)
			rt[i] = f(array[i]);
		return rt;
	}
	public static T[] Combine<T>(this T[] array, params T[][] arrays)
	{
		Assert(arrays.Length > 0);
		int totlen = 0;
		arrays.ForEach(a => totlen += a.Length);
		T[] ret = new T[totlen];
		int pos = 0;
		for(int i = 0; i < arrays.Length; i++)
		{
			if(arrays[i].Length == 0)
				continue;
			arrays[i].CopyTo(ret, pos);
			pos += arrays[i].Length;
		}
		return ret;
	}
}