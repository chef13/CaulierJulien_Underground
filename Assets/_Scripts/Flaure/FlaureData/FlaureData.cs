using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlaureData", menuName = "FlaureData")]
public class FlaureData : ScriptableObject
{
    public string planteName;
    public enum FlaureTypeEnum { Flower, Champi, Bush, Jonc }
    public FlaureTypeEnum flaureTypeEnum;
    public Sprite[] sprites;
    public float growingTime;
    public int growthStages;
    public int EdibleStage;
    public int edibleAmount;
    public int range;
    public float waterFactor = 1f;
   
}
