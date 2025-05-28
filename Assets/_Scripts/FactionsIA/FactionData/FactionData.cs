using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="FactionData",menuName = "factions/FactionData")]
public class FactionData : ScriptableObject
{
    public string factionName;
    public FactionType factionType;
    public GameObject[] prefabCreature;
    public int startingMembers;
   
}
