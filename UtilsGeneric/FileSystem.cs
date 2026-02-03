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
	public static string ConcatPaths(string p0, string p1)
	{
		Assert(p0 != null && p1 != null);
		if (p0.Length == 0 || p0 == "/")
			return p1;
		if (p1.Length == 0 || p1 == "/")
			return p0;
		bool p0is = p0.Last() == '/';
		bool p1is = p1.First() == '/';
		if (p0is ^ p1is)
			return p0 + p1;
		if (p0is)
			return p0.Remove(p0.Length - 1) + p1;
		return p0 + "/" + p1;
	}
	public static string ConcatPaths(string p0, string p1, string p2)
	{
		return ConcatPaths(ConcatPaths(p0, p1), p2);
	}
	public static string ConcatPaths(string p0, string p1, string p2, string p3)
	{
		return ConcatPaths(ConcatPaths(p0, p1, p2), p3);
	}

	public static string ConcatPaths(List<string> pl)
	{
		Assert(pl != null && pl.Count > 0);

		if (pl.Count == 1)
			return pl[0];
		int i = 2;
		string s = ConcatPaths(pl[0], pl[1]);
		while (i < pl.Count)
		{
			s = ConcatPaths(s, pl[i]);
			i++;
		}
		return s;
	}
}