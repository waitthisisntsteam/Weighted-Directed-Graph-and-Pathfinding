using System;
using System.Collections.Generic;
using System.Text;

namespace Weighted_Directed_Graph
{
    // next time change Heap<T> to work with IComparer<T>
    public class Heap<T> where T : IComparable<T>
    {
        public T[] items;
        private IComparer<T> comparer;
        public int count = 0;

        public Heap(int capacity, IComparer<T> c) : this(capacity)
        {
            comparer = c;
        }

        public Heap(int capacity)
        {
            count = 0;
            items = new T[capacity];
            comparer = Comparer<T>.Default;
        }

        public void Print()
        {
            Console.Clear();
            int x = Console.WindowWidth / 2;
            int y = 0;
            int xIncrement = x;
            int numsOnLine = 1;

            int currIndex = 0;

            while (currIndex < count)
            {
                for (int j = 0; j < numsOnLine; j++)
                {
                    if (currIndex < count)
                    {
                        Console.SetCursorPosition(x + xIncrement * 2 * j, y);
                        Console.Write(items[currIndex]);
                        currIndex++;
                    }
                }
                x /= 2;
                y++;
                xIncrement = x;
                numsOnLine *= 2;
            }
        }

        public void Insert(T value)
        {
            if (count <= 0)
            {
                items[0] = value;
            }
            else if (count < items.Length)
            {
                items[count] = value;
                HeapifyUp(count);
            }
            count++;
        }

        public void HeapifyUp(int index)
        {
            if (index == 0 || count < 2)
            {
                return;
            }

            int parentIndex = (index - 1) / 2;
            if (comparer.Compare(items[index], items[parentIndex]) < 0)
            {
                var temp = items[index];
                items[index] = items[parentIndex];
                items[parentIndex] = temp;
            }
            HeapifyUp(parentIndex);
        }

        public T Pop()
        {
            T root = items[0];
            count--;
            items[0] = items[count];
            HeapifyDown(0);

            

            return root;
        }

        public bool Contains(T mightContain)
        {
            for (int i = 0; i < count; i += 1)
            {
                if (items[i].Equals(mightContain))
                    return true;
            }

            return false;
        }

        private void HeapifyDown(int index)
        {
            int leftChildIndex = index * 2 + 1;
            int rightChildIndex = index * 2 + 2;

            if (leftChildIndex < count)
            {
                if (rightChildIndex >= count || comparer.Compare(items[leftChildIndex], items[rightChildIndex]) < 0)
                {
                    if (comparer.Compare(items[index], items[leftChildIndex]) > 0)
                    {
                        var temp = items[index];
                        items[index] = items[leftChildIndex];
                        items[leftChildIndex] = temp;
                        index = leftChildIndex;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (comparer.Compare(items[index], items[rightChildIndex]) > 0)
                    {
                        var temp = items[index];
                        items[index] = items[rightChildIndex];
                        items[rightChildIndex] = temp;

                        index = rightChildIndex;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                return;
            }

            HeapifyDown(index);
        }
    }
}
