using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FactionData", menuName = "factions/FactionData")]
public class FactionData : ScriptableObject
{
    public string factionName;
    public enum FactionTypeEnum { Goblin, Lezard, Human, Dungeon, Wanderer }
    public FactionTypeEnum factionTypeEnum;
    public GameObject[] prefabCreature;
    public int startingMembers;
    
    public int maxMembersPerHQ;
   
}


