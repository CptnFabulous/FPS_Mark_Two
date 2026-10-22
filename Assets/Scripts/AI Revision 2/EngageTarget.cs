using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EngageTarget : AIStateFunction
{
    [Header("Attack")]
    [SerializeField] AIGunAttack currentAttack;

    [Header("Seeking target")]
    [SerializeField] float maxMoveDistance = 100;
    int maxNumberOfChecks = 100;
    //bool findNewPositionIfCompromised = true;
    [SerializeField] float minTargetDistance = 5;
    [SerializeField] float idealTargetDistance = 10;
    [SerializeField] float maxTargetDistance = 15;
    [SerializeField, Min(0), Tooltip("How close does the AI want to stay to cover? If set to zero, they won't bother seeking cover.")]
    float distanceToCover = 3;
    [SerializeField] float distanceFromAllies = 5;
    [SerializeField] public DetectionProfile terrainDetection;

    [Header("Alternate states")]
    [SerializeField] AIStateFunction onTargetEliminated;

    [Header("On target lost")]
    public float timeAfterSightLossUntilSearch = 5;
    [SerializeField] InvestigateLocations searchForMissingTarget;
    [SerializeField] AIStateFunction onTargetLost;

    NavMeshPath calculatedPath;
    float lastTimePathWasValid = Mathf.NegativeInfinity;
    static readonly SortWithOnePredicate<AIGridPoints.GridPoint> predicate = new SortWithOnePredicate<AIGridPoints.GridPoint>();

    public Character target => targetManager.target;
    public Vector3 lastSightedPoint => targetManager.lastValidHit.point;

    private void OnDisable()
    {
        currentAttack.CancelAttack();
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        calculatedPath ??= new NavMeshPath();
        TryAcquirePath(ref calculatedPath, out _);
    }
    public void Update()
    {
        #region Check if enemy has been killed/deleted

        if (targetManager.targetExists == false)
        {
            controller.SwitchToState(onTargetEliminated);
            return;
        }

        #endregion

        #region If the AI has been unable to get line of sight on the target for an extended period, try searching more proactively.

        float timeSinceTargetLost = Time.time - targetManager.lastTimeSeenTarget;
        if (timeSinceTargetLost >= timeAfterSightLossUntilSearch)
        {
            // If AI can't or won't search, proceed straight to 'on target lost'
            bool willSearch = searchForMissingTarget != null && searchForMissingTarget.TrySearchForNewPosition(lastSightedPoint, 0, onTargetLost, 1, true, true);
            if (!willSearch)
            {
                controller.SwitchToState(onTargetLost);
                return;
            }
        }

        #endregion






        if (navMeshAgent != null)
        {
            // Keep track of last time destination was valid.
            // If current path is invalid, try setting a new one.

            // If AI goes for too long without being able to find a new path, proceed to an alternate state?




            Vector3 destination = navMeshAgent.destination;

            rootAI.StartDebug();

            if (VantagePointIsValid(destination))
            {
                // Existing destination is fine
                lastTimePathWasValid = Time.time;
                rootAI.DebugLog($"{destination} still valid");
            }
            else if (TryAcquirePath(ref calculatedPath, out Vector3 newDestination))
            {
                // Old destination is bad, but we found a new one
                rootAI.DebugLog($"{destination} invalid, {newDestination} found");
            }
            /*
            else if (Time.time - lastTimePathWasValid > )
            {
                // Take action for if AI is unable to damage target
            }
            */
            else
            {
                // Cannot find a valid path. If this 
                rootAI.DebugLog($"{destination} invalid, no new position found");
            }

            rootAI.StopDebug();















        }

        #region Run attack routines

        bool canPerformAttackRoutine = currentAttack.RunAttackRoutine();
        if (!canPerformAttackRoutine)
        {
            // If AI can't see the target, keep its sight trained on the last position.
            aim.RotateFreeLookTowards(lastSightedPoint);
        }

        #endregion
    }

    bool TryAcquirePath(ref NavMeshPath path, out Vector3 destination)
    {
        destination = navMeshAgent.transform.position;
        if (target == null) return false;
        
        #region Get initial spread of points, using simple checks to weed out the obviously bad positions

        Vector3 targetPosition = target.bounds.center;

        // Get initial sample of grid points, using simple checks to remove the vast majority before sorting
        int sampledPointCount = AIGridPoints.Current.GetPointsNonAlloc(ref AIGridPoints.sampleArray, (point) =>
        {
            // Use hypothetical look origin, to make sure distance is valid
            Vector3 lookOrigin = AIAction.HypotheticalLookOrigin(rootAI, point.position);
            bool withinRadius = AIGridPoints.IsWithinRadius(lookOrigin, targetPosition, minTargetDistance, maxTargetDistance);
            if (!withinRadius) return false;

            // Do a simple raycast check for stuff that will obviously block the attack, but isn't as accurate (and expensive) as our proper line of sight check
            if (Physics.Linecast(lookOrigin, targetPosition, out RaycastHit rh, terrainDetection.mask)) return false;

            return true;
        });

        rootAI.DebugLog($"Number of sampled points = {sampledPointCount}");
        /*
        for (int i = 0; i < sampledPointCount; i++)
        {
            Debug.DrawRay(AIGridPoints.sampleArray[i].position, Vector3.up, Color.magenta, 5);
        }

        EditorApplication.isPaused = true;
        throw new System.NotImplementedException();
        */



        #endregion

        #region Sort array using necessary criteria

        float idealDistanceSquared = idealTargetDistance * idealTargetDistance;

        predicate.reverse = false;
        predicate.obtainValue = (p) =>
        {
            float cost = 1;
            // Get distance to target, and figure out how close it is to the ideal distance
            float distanceToTargetSquared = (targetPosition - p.position).sqrMagnitude;
            cost *= Mathf.Abs(distanceToTargetSquared - idealDistanceSquared);
            // Also sort based on how far it is from the player's current position
            cost *= (p.position - rootAI.transform.position).sqrMagnitude;
            // Use square magnitudes to reduce performance cost, while keeping values proportional for accurate comparisons
            return cost;
        };
        System.Array.Sort(AIGridPoints.sampleArray, 0, sampledPointCount, predicate);

        #endregion

        // Now the values are sorted, try the complex checks on each value until a valid one is found
        int count = Mathf.Min(sampledPointCount, maxNumberOfChecks);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = AIGridPoints.sampleArray[i].position;

            // Check that the AI can realistically attack the player from this position.
            if (!VantagePointIsValid(position, false)) continue;
            // Check that the AI's navmeshagent can form a valid path to this point.
            if (!AIPathing.CanMoveToDestination(rootAI, position, maxMoveDistance, ref path)) continue;
            // Check that the path can be assigned properly
            if (!navMeshAgent.SetPath(calculatedPath)) continue;

            // Final data assignment for confirmed position and path
            destination = position;
            lastTimePathWasValid = Time.time;
            return true;
        }

        // No valid points were found
        return false;
    }

    public bool VantagePointIsValid(Vector3 position, bool checkDistance = true)
    {
        // Check if distance is not too close or too far
        if (checkDistance)
        {
            Vector3 targetPosition = target.bounds.center;
            float distanceToTarget = Vector3.Distance(position, targetPosition);
            if (distanceToTarget < minTargetDistance)
            {
                rootAI.DebugLog($"{distanceToTarget} < {minTargetDistance}");
                return false;
            }
            if (distanceToTarget > maxTargetDistance)
            {
                rootAI.DebugLog($"{distanceToTarget} > {maxTargetDistance}");
                return false;
            }
        }
        
        // Ensure line of sight is not compromised
        Vector3 lookOriginFromVantagePoint = AIAction.HypotheticalLookOrigin(rootAI, position);
        bool lineOfSight = currentAttack.AttackNotBlocked(lookOriginFromVantagePoint, out RaycastHit rh);
        if (lineOfSight == false)
        {
            rootAI.DebugLog($"Line of sight blocked by {rh.collider}");
            return false;
        }

        // Check that the path isn't too close to any points that allies are moving to
        if (memory.AreEnoughAlliesInvestigating(position, distanceFromAllies, 1, out _))
        {
            rootAI.DebugLog("Too many enemies investigating same spot");
            return false;
        }

        // Check if the position is close to cover
        if (distanceToCover > 0)
        {
            bool foundCover = AIPathing.FindCover(rootAI, position, target.attackOrigin, distanceToCover, out _);
            if (!foundCover)
            {
                rootAI.DebugLog("Too far from any cover");
                return false;
            }
        }

        return true;
    }
}
