using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlaureData", menuName = "FlaureData")]
public class FlaureData : ScriptableObject
{
    public string planteName;
    public enum FlaureTypeEnum { Flower, Champi, Bush, Jonc }
    public FlaureTypeEnum factionTypeEnum;
    public Sprite[] sprites;
    public float growingTime;
    public int growthStages;
    public int range;
    public bool needWater;
    public bool needNature;
    public bool likeDeadEnds;
   
}
