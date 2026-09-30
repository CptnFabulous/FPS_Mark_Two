using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BehaviourTreeBranch : BehaviourTreeNode, IReadOnlyList<BehaviourTreeNode>
{
    public enum BehaviourTreeType
    {
        /// <summary>
        /// Performs each function in sequence until all complete or one fails.
        /// </summary>
        Sequential,
        /// <summary>
        /// Runs down the list of options until one is successful.
        /// </summary>
        Troubleshooting,
    }

    public abstract BehaviourTreeType type { get; }

    BehaviourTreeNode currentNode;
    //Coroutine currentCoroutine;
    bool success;
    bool cancelledPrematurely = false;

    WaitForCoroutineOrCancel coroutineRunner;

    public abstract int Count { get; }
    public abstract BehaviourTreeNode this[int index] { get; }
    public abstract IEnumerator<BehaviourTreeNode> GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Should I make it so behaviour trees don't retain any data at all, and rely entirely on the controller to be a link to other objects?

    // Idea:
    // Have a variant that specified a base node and a series of 'solution' nodes.
    // Automatically generates sequential branches of 'perform solution' and 'retry original action'
    // Or should I just set up code to reset the current state?
    // E.g. an AI moving along a route and dealing with obstacles
    // Once an obstacle is dealt with, the AI should reset its troubleshooting priority (in case they encounter another obstacle they previously tried to solve)




    private void Awake()
    {
        // Create a coroutine runner
        coroutineRunner = new WaitForCoroutineOrCancel(this, null);
        // Ensure all nodes are disabled except for the ones that are meant to be running
        OnDisable();
    }
    private void OnDisable()
    {
        for (int i = 0; i < Count; i++) this[i].enabled = false;
    }

    protected override IEnumerator PerformTask()
    {
        for (int i = 0; i < Count; i++)
        {
            // Start running the next node, and wait until completion
            yield return RunNewNode(this[i]);

            // Use DidTaskSucceed() to determine succes (unless task was cancelled prematurely)
            if (!cancelledPrematurely) success = currentNode.Result();

            // Either continue or break depending on function
            switch (success, type)
            {
                // If sequential, continue on success and break on failure
                case (true, BehaviourTreeType.Sequential):
                    continue;
                    break;

                case (false, BehaviourTreeType.Sequential):
                    yield break;
                    break;

                // If troubleshooting, break on success and continue on failure
                case (true, BehaviourTreeType.Troubleshooting):
                    yield break;
                    break;

                case (false, BehaviourTreeType.Troubleshooting):
                    continue;
                    break;
            }
        }

        // All nodes have been explored.

        // If sequential, that means we've completed the entire sequence.
        if (type == BehaviourTreeType.Sequential)
        {
            success = true;
            yield break;
        }

        // If troubleshooting, it means we're out of options.
        success = false;
    }
    public override IEnumerator OnCancelTask()
    {
        // Cancel current state's actions and perform all its exit functions
        yield return RunNewNode(null);
    }
    public override bool Result() => success;

    IEnumerator RunNewNode(BehaviourTreeNode node)
    {
        // End actions of current node
        if (currentNode != null)
        {
            //StopCoroutine(currentCoroutine);
            coroutineRunner.CancelImmediately();
            yield return currentNode.OnCancelTask();
            currentNode.enabled = false;
        }

        // Assign new node
        currentNode = node;
        cancelledPrematurely = false;

        // Start running new node
        if (node != null)
        {
            // Run the coroutine through coroutineRunner, so it can be cancelled prematurely
            yield return coroutineRunner.RunCoroutine(currentNode.StartTask(this));
            //currentCoroutine = StartCoroutine(currentNode.PerformTask());
        }
    }

    protected void EndCurrentTaskPrematurely(bool success)
    {
        coroutineRunner.CancelImmediately();
        cancelledPrematurely = true;
        this.success = success;
    }

}

public class CustomBehaviourTreeBranch : BehaviourTreeBranch
{

    [SerializeField] BehaviourTreeType _type;
    [SerializeField] public List<BehaviourTreeNode> nodes;

    public override BehaviourTreeType type => _type;
    public override BehaviourTreeNode this[int index]
    {
        get => nodes[index];
    }
    public override int Count => nodes.Count;


    public override IEnumerator<BehaviourTreeNode> GetEnumerator() => nodes.GetEnumerator();
}
