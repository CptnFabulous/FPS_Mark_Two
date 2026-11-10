using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NonRecursiveSearch<T>
{
    static readonly Stack<T> stack = new Stack<T>();
    static readonly Queue<T> queue = new Queue<T>();

    public static IEnumerator<T> DepthFirst(T root, Func<T, bool> criteria, Func<T, IEnumerable<T>> getChildren)
    {
        return Generic(root, stack.Pop, stack.Push, stack.Clear, () => stack.Count, criteria, getChildren);
    }
    public static IEnumerator<T> DepthFirst(T root, T[] array, Func<T, bool> criteria, Func<T, IEnumerable<T>> getChildren)
    {
        int count = 0;
        return Generic(root, () =>
        {
            count--;
            return array[count];
        }, (t) =>
        {
            array[count] = t;
            count++;
        }, () => count = 0, () => count, criteria, getChildren);
    }
    public static IEnumerator<T> BreadthFirst(T root, Func<T, bool> criteria, Func<T, IEnumerable<T>> getChildren)
    {
        return Generic(root, queue.Dequeue, queue.Enqueue, queue.Clear, () => queue.Count, criteria, getChildren);
    }



    public static IEnumerator<T> Generic(T root, Func<T> takeNext, Action<T> addNew, System.Action clear, Func<int> getCount, Func<T, bool> criteria, Func<T, IEnumerable<T>> getChildren)
    {
        // Ensure collection is entirely empty, except for the root
        clear.Invoke();
        addNew.Invoke(root);

        int maxCount = 0;

        // This loop will not stop until all values are processed, as each entry adds its children from inside the loop
        while (getCount.Invoke() > 0)
        {
            maxCount = Mathf.Max(maxCount, getCount.Invoke());

            // Get the next item from the collection, and confirm it meets criteria
            T next = takeNext.Invoke();
            bool criteriaMet = criteria == null || criteria.Invoke(next);
            if (!criteriaMet) continue;

            // Yield on next result
            //Debug.Log($"Yielding on {}");
            yield return next;

            // Add all non-null children to the collection, so they can be processed next
            IEnumerable<T> children = getChildren.Invoke(next);
            foreach (T child in children)
            {
                if (child == null) continue;
                addNew.Invoke(child);
            }
        }

        //Debug.Log($"Non-recursive search completed, max values in collection = {maxCount}");
    }
}