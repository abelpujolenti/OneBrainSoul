using System;
using System.Collections.Generic;

namespace Utilities
{
    /*public class PriorityQueue<T>
    {
        private List<(T item, float priority)> heap = new List<(T item, float priority)>();

        public int Count => heap.Count;

        public void Enqueue(T item, float priority)
        {
            heap.Add((item, priority));
            HeapifyUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            T root = heap[0].item;
            heap[0] = heap[^1];
            heap.RemoveAt(heap.Count - 1);
            HeapifyDown(0);

            return root;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (!heap[i].item.Equals(item))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public T Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            return heap[0].item;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            int index = heap.FindIndex(x => x.item.Equals(item));

            if (index == -1)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            float oldPriority = heap[index].priority;

            heap[index] = (item, newPriority);

            if (newPriority < oldPriority)
            {
                HeapifyUp(index);
                return;
            }
            
            HeapifyDown(index);
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                if (heap[index].priority >= heap[parentIndex].priority)
                {
                    break;
                }

                (heap[index], heap[parentIndex]) = (heap[parentIndex], heap[index]);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftChildIndex = 2 * index + 1;
                int rightChildIndex = 2 * index + 2;
                int smallestIndex = index;

                if (leftChildIndex < heap.Count && heap[leftChildIndex].priority < heap[smallestIndex].priority)
                {
                    smallestIndex = leftChildIndex;
                }

                if (rightChildIndex < heap.Count && heap[rightChildIndex].priority < heap[smallestIndex].priority)
                {
                    smallestIndex = rightChildIndex;
                }

                if (smallestIndex == index)
                {
                    break;
                }

                (heap[index], heap[smallestIndex]) = (heap[smallestIndex], heap[index]);

                index = smallestIndex;
            }
        }
    }*/
    
    public class PriorityQueue<T>
    {
        private class PriorityComparer : IComparer<(T item, float priority)>
        {
            public int Compare((T item, float priority) x, (T item, float priority) y)
            {
                return x.priority.CompareTo(y.priority);
            }
        }
        
        private List<(T item, float priority)> heap = new List<(T item, float priority)>();

        public int Count => heap.Count;

        public void Enqueue(T item, float priority)
        {
            int index = heap.BinarySearch((item, priority), new PriorityComparer());

            if (index < 0)
            {
                index = ~index;
            }
            
            heap.Insert(index, (item, priority));
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            T item = heap[0].item;
            heap.RemoveAt(0);

            return item;
        }

        public bool Contains(T item)
        {
            for (int i = 0; i < heap.Count; i++)
            {
                if (!heap[i].item.Equals(item))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        public T Peek()
        {
            if (heap.Count == 0)
            {
                throw new InvalidOperationException("The queue is empty.");
            }

            return heap[0].item;
        }

        public void UpdatePriority(T item, float newPriority)
        {
            int index = heap.FindIndex(x => x.item.Equals(item));

            if (index == -1)
            {
                throw new InvalidOperationException("The queue is empty.");
            }
            
            heap.RemoveAt(index);
            
            Enqueue(item, newPriority);
        }
    }
}