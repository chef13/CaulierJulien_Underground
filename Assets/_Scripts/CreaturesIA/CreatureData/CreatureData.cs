using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "CreatureData")]
public class CreatureData : ScriptableObject
{
    public string creatureName;
    public Sprite skeletonSprite;
    public enum CreatureTypeEnum { Goblin, Lezard, Human, Mushroom, Skeleton }
    public CreatureTypeEnum CreatureType;
    public int maxLife, maxEnergy, maxHunger, attackPower, detectionRange;
    public float attackRange, attackSpeed, speed;

    //public enum FlaureType { Flower, Champi, Bush, Jonc }
    public bool herbivor;
    public bool carnivor;
   
}
