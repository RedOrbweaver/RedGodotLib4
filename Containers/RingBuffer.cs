using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static partial class Utils
{
    public class RingBuffer<T>(int Length) : IEnumerable<T>, IEnumerable where T : new()
    {
        T[] _values = new T[Length];
        public int Length { get; protected set; } = Length;
        public int Position {get; protected set;} = 0;
        public IEnumerator<T> GetEnumerator()
		{
			return (IEnumerator<T>)_values.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)_values.GetEnumerator();
		}

		public int PushRange(IEnumerable<T> values)
		{
			if(values.Count() == Length)
			{
				_values = values.ToArray();
				return Position;
			}
			foreach(var it in values)
			{
				Push(it);
			}
			return Position;
		}
		public int Push(T value)
		{
			_values[Position] = value;
			Position++;
			Position %= Length;
			return Position;
		}
		public T Pop()
		{
			T v =  _values[Position];
			Position--;
			if(Position < 0)
				Position = Length-1;
			return v;
		}

		public T GetValue(int indx)
		{
			if(indx < 0)
				indx = Length + indx;
			Assert<IndexOutOfRangeException>(indx < Length && indx > 0);
			indx = (Position + indx) % Length;
			return _values[indx];
		}
		public void SetValue(int indx, T val)
		{
			if(indx < 0)
				indx = Length + indx;
			Assert<IndexOutOfRangeException>(indx < Length && indx > 0);
			indx = (Position + indx) % Length;
			_values[indx] = val;
		}

		public T GetValueMod(int indx)
		{
			if(indx < 0)
				indx = Length + indx;
			indx %= Length;
			return GetValue(indx);
		}
		public void SetValueMod(int indx, T val)
		{
			if(indx < 0)
				indx = Length + indx;
			indx %= Length;
			SetValue(indx, val);
		}
		public T this[int indx]
		{
			get
			{
				return GetValue(indx);
			}
			set
			{
				SetValue(indx, value);
			}
		}
    }
}