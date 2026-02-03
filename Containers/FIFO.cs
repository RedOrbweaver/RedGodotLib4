using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static partial class Utils
{
	public class FIFO<T> : IEnumerable<T>, IEnumerable
	{
		Queue<T> queue;
		public int Capacity { get; protected set; }
		public int Count => queue.Count;
		public bool Full => Count == Capacity;
		public bool Empty => Count == 0;
		public IEnumerator<T> GetEnumerator()
		{
			return queue.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return queue.GetEnumerator();
		}
		public T this[int indx]
		{
			get => queue.ElementAt(indx);
		}
		public T Peek() => queue.Peek();
		public T Dequeue() => queue.Dequeue();
		public void Enqueue(T val)
		{
			if (queue.Count == Capacity)
				queue.Dequeue();
			queue.Enqueue(val);
		}
		public FIFO(int capacity)
		{
			this.Capacity = capacity;
			this.queue = new Queue<T>(capacity);
		}
	}
}