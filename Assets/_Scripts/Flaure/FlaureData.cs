using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlaureData", menuName = "FlaureData")]
public class FlaureData : ScriptableObject
{
    public string planteName;
    public Sprite[] sprites;
    public float growingTime;
    public int growthStages;
   
}
