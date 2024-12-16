using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCmanager : MonoBehaviour
{
    public static NPCmanager Instance = new NPCmanager();

    private Dictionary<string, Vector3> npcStates = new Dictionary<string, Vector3>();

    public void SetNPCPosition(string npcID, Vector3 position)
    {
        npcStates[npcID] = position;
    }

    public Vector3 GetNPCPosition(string npcID)
    {
        return npcStates.ContainsKey(npcID) ? npcStates[npcID] : Vector3.zero;
    }
}
