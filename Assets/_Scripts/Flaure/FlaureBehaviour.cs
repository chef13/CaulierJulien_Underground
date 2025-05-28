using UnityEngine;
using System.Collections;
[RequireComponent(typeof(SpriteRenderer))]
public class FlaureBehaviour : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public FlaureData flaureData;
    public int currentStage = 0;
    
    public float currentGrowthTime = 0f;
    public int range = 2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = flaureData.sprites[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator Grow()
    {

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
            }
        }
        yield return new WaitForSeconds(flaureData.growingTime / flaureData.growthStages);
        currentStage++;
    }
}
