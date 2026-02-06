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

public static partial class Utils
{
    public static string SerializeToString<T>(T o, bool humanreadible = false, bool typedata = true, bool ignore_errors = false) where T : class
    {
        List<string> errors = new List<string>();
        void ErrorHandler(object sender, ErrorEventArgs e)
        {
            errors.Add(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
        }
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = (typedata) ? TypeNameHandling.All : TypeNameHandling.None,
            Formatting = (humanreadible)? Formatting.Indented : Formatting.None,
            Error = ErrorHandler,
        };
        var ret = JsonConvert.SerializeObject(o, settings);
        if(!ignore_errors && errors.Count > 0)
        {
            Console.WriteLine($"{errors.Count} errors during serialization of {o}:");
			foreach (var err in errors)
			{
				Console.WriteLine(err.ToString());
			}	
            throw new Exception($"{errors.Count} errors during serialization of {o}");
        }
        return ret;
    }
    public static object DeserializeFromString(Type type, string data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling =  TypeNameHandling.Auto,
        };
        return JsonConvert.DeserializeObject(data, type);
    }
    public static T DeserializeFromString<T>(string data, bool returnnullonfail = false) where T : class
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling =  TypeNameHandling.Auto,
        };
        T o = null;
        try
        {
            o = JsonConvert.DeserializeObject<T>(data, settings);
        }
        catch (JsonException jex)
        {
            if (!returnnullonfail)
                throw jex;
            return null;
        }

        if (!returnnullonfail)
            Assert(o != null);

        return o;
    }
	[System.Serializable]
	public class SerializationFailedException : System.Exception
	{
		public SerializationFailedException() { }
		public SerializationFailedException(string message) : base(message) { }
		public SerializationFailedException(string message, System.Exception inner) : base(message, inner) { }
		protected SerializationFailedException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
	[System.Serializable]
	public class DeserializationFailedException : System.Exception
	{
		public DeserializationFailedException() { }
		public DeserializationFailedException(string message) : base(message) { }
		public DeserializationFailedException(string message, System.Exception inner) : base(message, inner) { }
		protected DeserializationFailedException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
	public unsafe static byte[] StructToBytes<T>(T s) where T : struct
	{
		int size = Marshal.SizeOf(s);
		byte[] dt = new byte[size];
		fixed(byte* dtptr = dt)
		{
			IntPtr ptr = (IntPtr)dtptr;
			try
			{
				Marshal.StructureToPtr(s, ptr, false);
			}
			catch (Exception ex)
			{
				throw new SerializationFailedException("Serialization failure", ex);
			}
		}
		return dt;
	}
	public unsafe static T BytesToStruct<T>(byte[] dt) where T : struct
	{
		IntPtr buffer = Marshal.AllocCoTaskMem(Marshal.SizeOf<T>());
		Marshal.Copy(dt, 0, buffer, dt.Length);
		T ret; 
		try
		{
			ret = (T)Marshal.PtrToStructure(buffer, typeof(T));
			// fixed (byte* bptr = dt)
			// {
			// 	return (T)Marshal.PtrToStructure((IntPtr)bptr, typeof(T));
			// }
		}
		catch (ArgumentException ex)
		{
			throw new DeserializationFailedException("Deserialization failure", ex);
		}
		finally
		{
			Marshal.FreeCoTaskMem(buffer);
		}
		return ret;
	}
}