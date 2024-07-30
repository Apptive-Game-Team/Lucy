using System;
using System.Collections.Generic;

public class PriorityQueue<TElement, TPriority>
{
    private List<(TElement Element, int Priority)> _elements;

    public PriorityQueue()
    {
        _elements = new List<(TElement, int)>();
    }

    public void Enqueue(TElement element, int priority)
    {
        _elements.Add((element, priority));
        HeapifyUp(_elements.Count - 1);
    }

    public TElement Dequeue()
    {
        if (_elements.Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        var root = _elements[0].Element;
        var lastIndex = _elements.Count - 1;
        _elements[0] = _elements[lastIndex];
        _elements.RemoveAt(lastIndex);
        HeapifyDown(0);

        return root;
    }

    public int Count => _elements.Count;

    private void HeapifyUp(int index)
    {
        var item = _elements[index];
        while (index > 0)
        {
            var parentIndex = (index - 1) / 2;
            var parentItem = _elements[parentIndex];
            if (item.Priority >= parentItem.Priority) 
                break;

            _elements[index] = parentItem;
            index = parentIndex;
        }
        _elements[index] = item;
    }

    private void HeapifyDown(int index)
    {
        var item = _elements[index];
        var lastIndex = _elements.Count - 1;

        while (index <= lastIndex)
        {
            var leftChildIndex = (index * 2) + 1;
            if (leftChildIndex > lastIndex)
                break;

            var rightChildIndex = leftChildIndex + 1;
            if (rightChildIndex <= lastIndex &&
                _elements[rightChildIndex].Priority < _elements[leftChildIndex].Priority)
            {
                leftChildIndex = rightChildIndex;
            }

            if (item.Priority <= _elements[leftChildIndex].Priority)
                break;

            _elements[index] = _elements[leftChildIndex];
            index = leftChildIndex;
        }
        _elements[index] = item;
    }
}
