using System.Collections.Generic;

public class PriorityQueue<T>
{
    private List<(T item, float priority)> elements = new();

    public int Count => elements.Count;

    public void Enqueue(T item, float priority)
    {
        elements.Add((item, priority));
        int childIndex = elements.Count - 1;
        while (childIndex > 0)
        {
            int parentIndex = (childIndex - 1) / 2;
            if (elements[childIndex].priority >= elements[parentIndex].priority)
                break;

            var temp = elements[childIndex];
            elements[childIndex] = elements[parentIndex];
            elements[parentIndex] = temp;

            childIndex = parentIndex;
        }
    }

    public T Dequeue()
    {
        if (elements.Count == 0)
            throw new System.InvalidOperationException("Queue is empty");

        var result = elements[0].item;
        elements[0] = elements[elements.Count - 1];
        elements.RemoveAt(elements.Count - 1);

        if (elements.Count > 0)
        {
            int parentIndex = 0;
            while (true)
            {
                int leftChild = 2 * parentIndex + 1;
                int rightChild = 2 * parentIndex + 2;
                int smallest = parentIndex;

                if (leftChild < elements.Count && elements[leftChild].priority < elements[smallest].priority)
                    smallest = leftChild;

                if (rightChild < elements.Count && elements[rightChild].priority < elements[smallest].priority)
                    smallest = rightChild;

                if (smallest == parentIndex)
                    break;

                var temp = elements[parentIndex];
                elements[parentIndex] = elements[smallest];
                elements[smallest] = temp;

                parentIndex = smallest;
            }
        }

        return result;
    }
}