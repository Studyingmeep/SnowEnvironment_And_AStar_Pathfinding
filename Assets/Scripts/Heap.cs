using System;
using UnityEngine;

// Min heap, available for most data types (template)
public class Heap<T> where T : IHeapItem<T>
{
    private T[] _items;
    private int _currentItemCount;

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }
    public void Clear()
    { 
        _currentItemCount = 0;
    }
    
    // After figuring out max items in heap, we go from 0 to the max size and use that as indices for each item
    public void Add(T item)
    {
        item.HeapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }

    // Remove the first item in the heap, and move the last item to the top of the heap
    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _currentItemCount--;
        _items[0] = _items[_currentItemCount];
        _items[0].HeapIndex = 0;
        SortDown(_items[0]);
        return firstItem;
    }
    
    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public int Count => _currentItemCount;

    public bool Contains(T item)
    {
        if (item.HeapIndex < _currentItemCount)
        {
            return Equals(_items[item.HeapIndex], item);
        } else
        {
            return false;
        }
    }

    void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;
            
            // If we have a left child (if no left child, we have no right child)
            if (childIndexLeft < _currentItemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < _currentItemCount)
                {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;

                    }
                }

                // Compare the item with the child, returning 1, 0 or -1.
                if (item.CompareTo(_items[swapIndex]) < 0)
                {
                    Swap(item, _items[swapIndex]);
                }
                else
                    return;
            }
            else
                return;
        }
    }

    // In SortUp we use maths to find the parentIndex and compare to help move our numbers.
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = _items[parentIndex];
            // Compare the item with the parent, returning 1, 0 or -1.
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);;
            }
            else // break when our current item is the parent
            {
                break;
            }
            
            // Move up the tree
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    void Swap(T itemA, T itemB)
    {
        _items[itemA.HeapIndex] = itemB;
        _items[itemB.HeapIndex] = itemA;
        (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
    }
}

public interface IHeapItem<in T> : IComparable<T>
{
    int HeapIndex { get; set; }
    new int CompareTo(T other);
}
