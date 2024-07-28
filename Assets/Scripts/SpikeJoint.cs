using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpikeJoint : MonoBehaviour
{
    public LayerMask impalable = ~0;
    public float minVelocityToImpale = 10;
    public float angleForImpalement = 45;
    public UnityEvent<Rigidbody> onImpaled;
    public UnityEvent<Rigidbody> onRemoved;

    [Header("Pinning and removing")]
    [SerializeField] Collider collider;
    //[SerializeField] ConfigurableJoint jointPrefab;
    [SerializeField] ConfigurableJoint jointScriptPrefab;
    public Vector3 connectedAnchorLocalOffset = new Vector3(0, 0, -0.5f);

    //List<ConfigurableJoint> activeJoints = new List<ConfigurableJoint>();
    List<Tuple<Collider, ConfigurableJoint>> activeJoints = new List<Tuple<Collider, ConfigurableJoint>>();
    List<ConfigurableJoint> jointPool = new List<ConfigurableJoint>();

    private void Awake()
    {
        jointPool.Add(jointScriptPrefab);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (enabled == false) return;

        if (collision.relativeVelocity.magnitude < minVelocityToImpale) return;

        Vector3 perfectImpaleDirection = -transform.forward;
        Vector3 impactDirection = collision.relativeVelocity;
        bool inAngle = Vector3.Angle(perfectImpaleDirection, impactDirection) < angleForImpalement;
        Debug.DrawRay(collider.bounds.center, -perfectImpaleDirection, Color.cyan, 5);
        Debug.DrawRay(collider.bounds.center, -impactDirection, inAngle ? Color.green : Color.red, 5);
        if (inAngle == false) return;

        TryImpale(collision.collider);
        // Restore incoming object's velocity prior to collision
        collision.rigidbody.velocity = collision.relativeVelocity;
    }
    //private void OnTriggerEnter(Collider other) => TryImpale(other);
    private void FixedUpdate()
    {
        if (activeJoints.Count <= 0) return;

        Bounds bounds = collider.bounds;
        
        for (int i = activeJoints.Count - 1; i >= 0; i--)
        {
            // If a pinned object no longer intersects with the collider, unpin it
            
            Rigidbody rb = activeJoints[i].Item1.attachedRigidbody;
            //Rigidbody rb = activeJoints[i].connectedBody;

            // If the closest rigidbody bounds point to the centre of the bounds is actually inside the bounds
            bool intersects = bounds.Contains(rb.ClosestPointOnBounds(bounds.center));
            if (intersects == false) TryRemove(rb);
        }
    }

    void TryImpale(Collider target)
    {
        // Check if the collider is already pinned
        Rigidbody rb = target.attachedRigidbody;
        if (activeJoints.Find((x) => x.Item1 == target) != null) return;
        //if (activeJoints.Find((x) => x.connectedBody == rb) != null) return;

        if (MiscFunctions.IsLayerInLayerMask(impalable, target.gameObject.layer) == false) return;

        // Check that a rigidbody is present, and not already pinned to this joint
        if (rb == null) return;

        Debug.Log("Impaling " + rb);

        // Commence impaling!

        // Spawn and orient a joint, attach the collider to it, and register it in the list of pinned objects.
        ConfigurableJoint joint = GetJoint();
        joint.connectedBody = rb;
        joint.connectedAnchor += connectedAnchorLocalOffset;
        activeJoints.Add(new Tuple<Collider, ConfigurableJoint>(target, joint));
        //activeJoints.Add(joint);

        // Disable and re-enable collider, to reset collisions and ensure the pinned object no longer collides
        target.enabled = false;
        target.enabled = true;

        // Play cosmetic effects
        onImpaled.Invoke(rb);

        foreach (Collider c in PhysicsCache.GetChildColliders(PhysicsCache.GetRootRigidbody(rb)))
        {
            TryImpale(c);
        }
    }
    void TryRemove(Rigidbody rb)
    {
        // Check if the exiting rigidbody is pinned to this object. If not, don't do anything.
        //Rigidbody rb = toRemove.attachedRigidbody;
        ConfigurableJoint joint = activeJoints.Find((x) => x.Item1.attachedRigidbody == rb).Item2;
        //ConfigurableJoint joint = activeJoints.Find((x) => x.connectedBody == rb);

        // Unpin the collider from the joint
        joint.connectedBody = null;
        // Clear references and return the joint to the pool
        activeJoints.RemoveAll((x) => x.Item2 == joint);
        //activeJoints.Remove(joint);
        jointPool.Add(joint);

        // Play cosmetic effects
        onRemoved.Invoke(rb);
    }
    
    





    ConfigurableJoint GetJoint()
    {
        ConfigurableJoint j = null;
        if (jointPool.Count > 0)
        {
            j = jointPool[0];
            jointPool.Remove(j);
        }
        else
        {
            j = gameObject.AddComponent<ConfigurableJoint>();
            CopyJointValues(j, jointScriptPrefab);
        }

        return j;
    }
    static void CopyJointValues(ConfigurableJoint target, ConfigurableJoint original)
    {
        target.anchor = original.anchor;
        target.angularXDrive = original.angularXDrive;
        target.angularXLimitSpring = original.angularXLimitSpring;
        target.angularXMotion = original.angularXMotion;
        target.angularYLimit = original.angularYLimit;
        target.angularYMotion = original.angularYMotion;
        target.angularYZDrive = original.angularYZDrive;
        target.angularYZLimitSpring = original.angularYZLimitSpring;
        target.angularZLimit = original.angularZLimit;
        target.angularZMotion = original.angularZMotion;
        target.autoConfigureConnectedAnchor = original.autoConfigureConnectedAnchor;
        target.axis = original.axis;

        target.breakForce = original.breakForce;
        target.breakTorque = original.breakTorque;

        target.configuredInWorldSpace = original.configuredInWorldSpace;
        target.connectedAnchor = original.connectedAnchor;
        target.connectedArticulationBody = original.connectedArticulationBody;
        
        //target.connectedBody = original.connectedBody; Don't copy the body in case it does something weird if that body is already connected to any joints on this object
        target.connectedMassScale = original.connectedMassScale;

        target.enableCollision = original.enableCollision;
        target.enablePreprocessing = original.enablePreprocessing;

        target.highAngularXLimit = original.highAngularXLimit;

        target.linearLimit = original.linearLimit;
        target.linearLimitSpring = original.linearLimitSpring;
        target.lowAngularXLimit = original.lowAngularXLimit;

        target.massScale = original.massScale;

        target.projectionAngle = original.projectionAngle;
        target.projectionDistance = original.projectionDistance;
        target.projectionMode = original.projectionMode;

        target.rotationDriveMode = original.rotationDriveMode;

        target.secondaryAxis = original.secondaryAxis;
        target.slerpDrive = original.slerpDrive;
        target.swapBodies = original.swapBodies;

        target.targetAngularVelocity = original.targetAngularVelocity;
        target.targetPosition = original.targetPosition;
        target.targetRotation = original.targetRotation;
        target.targetVelocity = original.targetVelocity;

        target.xDrive = original.xDrive;
        target.xMotion = original.xMotion;

        target.yDrive = original.yDrive;
        target.yMotion = original.yMotion;

        target.zDrive = original.zDrive;
        target.zMotion = original.zMotion;
    }






    /*
    Dictionary<Collider, ConfigurableJoint> pinned = new Dictionary<Collider, ConfigurableJoint>();



    private void OnCollisionEnter(Collision collision)
    {
        Vector3 velocity = collision.rigidbody.velocity;
        OnTriggerEnter(collision.collider);
        collision.rigidbody.velocity = velocity;
    }
    private void OnCollisionExit(Collision collision) => OnTriggerExit(collision.collider);
    private void OnTriggerEnter(Collider other)
    {
        // Check that a rigidbody is present, and not already pinned to this joint
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;
        if (pinned.ContainsKey(other)) return;

        // Commence impaling!

        // Spawn a joint and orient it correctly
        Debug.Log($"Impaling {other}");
        ConfigurableJoint joint = GetJoint();
        

        // Register rigidbody and joint in dictionary
        joint.connectedBody = rb;
        pinned[other] = joint;

        // Play cosmetic effects!
        onImpaled.Invoke(other);
    }
    private void OnTriggerExit(Collider c)
    {
        // Check if the exiting rigidbody is pinned to this object. If not, don't do anything.
        Rigidbody rb = c.attachedRigidbody;
        if (pinned.TryGetValue(c, out ConfigurableJoint joint) == false) return;

        // Unpin the collider from the joint
        joint.connectedBody = null;
        // Clear the dictionary reference and dismiss the joint
        pinned.Remove(c);
        jointPool.Add(joint);
        //ObjectPool.DismissObject(joint);

        // Play cosmetic effects
        onRemoved.Invoke(c);
    }
    */
}