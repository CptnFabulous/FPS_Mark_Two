using CptnFabulous.MiscUtility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputPriorityHandler : MonoBehaviour
{
    public class InputStack : ICollection<SingleInput>
    {
        public InputStack(InputAction action)
        {
            this.action = action;
        }

        public readonly InputAction action;
        readonly List<SingleInput> list = new List<SingleInput>();

        public SingleInput currentPriority { get; private set; }

        public void Refresh()
        {
            // Sort based on priority
            list.SortListWithOnePredicate((i) => i.priority, false);

            // Figure out the new highest priority. If it's the same as before, do nothing.
            int count = list.Count;
            SingleInput newPriority = (count > 0) ? list[count - 1] : null;
            if (newPriority == currentPriority)
            {
                //Debug.Log($"Refreshing, {currentPriority} is still current priority");
                return;
            }

            SingleInput old = currentPriority;
            //Debug.Log($"Changing {currentPriority} to {newPriority}");

            // Clear listeners related to the old SingleInput
            if (currentPriority != null)
            {
                action.Reset();
                action.started -= currentPriority.onActionStarted.Invoke;
                action.performed -= currentPriority.onActionPerformed.Invoke;
                action.canceled -= currentPriority.onActionCancelled.Invoke;
            }

            // Assign the new one and add the necessary listeners
            currentPriority = newPriority;
            if (currentPriority != null)
            {
                action.Enable();
                action.started += currentPriority.onActionStarted.Invoke;
                action.performed += currentPriority.onActionPerformed.Invoke;
                action.canceled += currentPriority.onActionCancelled.Invoke;
            }

            /*
            string debug = $"Changing from {old}, new priority = ";
            for (int i = 0; i < list.Count; i++)
            {
                debug += $"{list[i].name}, ";
            }
            Debug.Log(debug);
            */
        }

        public void Add(SingleInput item)
        {
            if (item == null) return;
            if (list.Contains(item)) return;
            list.Add(item);
            Refresh();
        }
        public void Clear()
        {
            list.Clear();
            Refresh();
        }
        public bool Contains(SingleInput item) => list.Contains(item);
        public void CopyTo(SingleInput[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);
        public bool Remove(SingleInput item)
        {
            if (list.Remove(item))
            {
                Refresh();
                return true;
            }
            return false;
        }
        public int Count => list.Count;
        public bool IsReadOnly => false;
        public IEnumerator<SingleInput> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
    }

    Dictionary<InputAction, InputStack> activeInputs = new Dictionary<InputAction, InputStack>();

    public void SetInputActive(SingleInput target, bool active)
    {
        //Debug.Log($"Setting {target.name} active state to {active}");

        // Ensure a dictionary exists for this action
        InputAction key = target.action;
        if (activeInputs.ContainsKey(key) == false) activeInputs.Add(key, new InputStack(key));

        // If input is active, ensure just one of itself is present in the list
        if (active)
        {
            activeInputs[key].Add(target);
        }
        else // Otherwise, ensure it is not present in the list.
        {
            activeInputs[key].Remove(target);
        }
    }

    
}