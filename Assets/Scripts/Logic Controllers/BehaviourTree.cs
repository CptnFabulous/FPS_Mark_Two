using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : BehaviourTreeNode
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
    
    public BehaviourTreeNode[] nodes;
    public BehaviourTreeType type;

    BehaviourTreeNode currentNode;
    //Coroutine currentCoroutine;
    bool success;
    bool cancelledPrematurely = false;

    WaitForCoroutineOrCancel coroutineRunner;

    private void Awake()
    {
        // Create a coroutine runner
        coroutineRunner = new WaitForCoroutineOrCancel(this, null);
        // Ensure all nodes are disabled except for the ones that are meant to be running
        OnDisable();
    }
    private void OnDisable()
    {
        for (int i = 0; i < nodes.Length; i++) nodes[i].enabled = false;
    }

    protected override IEnumerator PerformTask()
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            // Start running the next node, and wait until completion
            yield return RunNewNode(nodes[i]);

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
