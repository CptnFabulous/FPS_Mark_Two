using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : DoorBase
{
    [Header("Hinge")]
    public Rigidbody doorBody;
    public HingeJoint joint;
    public float angleForOpen = 30;
    public Vector3 openForce = new Vector3(0, 100, 0);
    public Transform openDirection;
    public Vector3 lockRotation = Vector3.zero;

    [Header("GUI")]
    [SerializeField] string pushPrompt = "Push";
    [SerializeField] string pullPrompt = "Pull";
    //[SerializeField] string openPrompt = "Open";
    //[SerializeField] string closePrompt = "Close";

    public override bool isOpen => joint.angle > angleForOpen;
    
    public override string OpenMessage(Player player, bool isOpen)
    {
        // If player and open direction are same, player pushes open and pulls shut.
        // If player and open direction are opposite, player pulls open and pushes shut.
        bool playerAndOpenDirectionAreSame = Vector3.Dot(player.aimDirection, openDirection.forward) > 0;
        string openPrompt = playerAndOpenDirectionAreSame ? pushPrompt : pullPrompt;
        string closePrompt = playerAndOpenDirectionAreSame ? pullPrompt : pushPrompt;

        return isOpen ? closePrompt : openPrompt;
    }
    protected override void Open() => doorBody.AddTorque(openForce);
    protected override void Close() => doorBody.AddTorque(-openForce);
    protected override bool IsLocked() => doorBody.isKinematic;
    protected override void SetLock(bool locked)
    {
        // Set ability for door to move
        doorBody.isKinematic = locked;
        // If locked, force to shut position
        if (locked) doorBody.transform.localRotation = Quaternion.Euler(lockRotation);
    }
}