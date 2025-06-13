
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "CreatureData")]
public class CreatureData : ScriptableObject
{
    public string creatureName;
    public Sprite sprite, deadsprite, skeletonSprite;
    public RuntimeAnimatorController animator;
    public enum CreatureTypeEnum { Goblin, Lezard, Human, Champi, Blop, Skeleton, Avatar }
    public CreatureTypeEnum CreatureType;
    public int maxLife, maxEnergy, maxHunger, attackPower, detectionRange;
    public float attackRange, attackSpeed, speed;

    //public enum FlaureType { Flower, Champi, Bush, Jonc }
    public bool herbivor;
    public bool carnivor;
   
}
