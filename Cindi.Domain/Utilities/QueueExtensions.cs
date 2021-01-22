using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Cindi.Domain.Utilities
{
    public static class QueueExtensions
    {
        public static IEnumerable<T> DequeueChunk<T>(this ConcurrentQueue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                T item;
                queue.TryDequeue(out item);
                yield return item;
            }
        }
    }
}
