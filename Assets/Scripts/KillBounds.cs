using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBounds : MonoBehaviour
{
    public Bounds levelBounds;
    public float delayBetweenSweeps = 5;
    float lastTimeSweeped;

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastTimeSweeped >= delayBetweenSweeps)
        {
            Entity[] allEntitiesInScene = FindObjectsOfType<Entity>();
            for (int i = 0; i < allEntitiesInScene.Length; i++)
            {
                if (levelBounds.Contains(allEntitiesInScene[i].transform.position) == false)
                {
                    Debug.Log(allEntitiesInScene[i].name + " is out of bounds, deleting");
                    allEntitiesInScene[i].Delete();
                }
            }


            lastTimeSweeped = Time.time;
        }
    }
}
