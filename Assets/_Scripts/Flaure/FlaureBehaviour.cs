using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
[RequireComponent(typeof(SpriteRenderer))]
public class FlaureBehaviour : MonoBehaviour
{
    public FlaureSpawner flaureSpawner;
    public FlaureType currentFlaureType;
    public SpriteRenderer spriteRenderer;
    public FlaureData flaureData;
    public TileInfo currentTile;
    public Coroutine growthCoroutine;
    public int currentStage = 0;
    public float currentGrowthTime = 0f;
    public bool isEdible = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public FlaureType GetFlaureTypeInstance()
    {
        switch (flaureData.flaureTypeEnum)
        {
            case FlaureData.FlaureTypeEnum.Flower:
                return new FlowerFlaure(this);
            case FlaureData.FlaureTypeEnum.Bush:
                return new BushFlaure(this);
            case FlaureData.FlaureTypeEnum.Champi:
                return new ChampiFlaure(this);
            case FlaureData.FlaureTypeEnum.Jonc:
                return new JoncFlaure(this);
            default:
                return null;
        }
    }

    void OnEnable()
    {
        currentFlaureType = GetFlaureTypeInstance();
        if (growthCoroutine == null)
            growthCoroutine = StartCoroutine(currentFlaureType.Grow());

        if (currentStage >= flaureData.EdibleStage)
        {
            isEdible = true;
        }
        else
        {
            isEdible = false;
        }

        spriteRenderer.sprite = flaureData.sprites[currentStage];

    }
    void Start()
    {
    }

    void OnDisable()
    {
        if (growthCoroutine != null)
        {
            StopCoroutine(growthCoroutine);
            growthCoroutine = null;
        }
        if (currentTile != null)
            currentTile.objects.Remove(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void SwitchType(FlaureType newType)
    {
        currentFlaureType?.Exit();
        currentFlaureType = newType;
        currentFlaureType?.Enter();
    }

    public void GrowingStage()
    {
        currentStage++;

        spriteRenderer.sprite = flaureData.sprites[currentStage];


        if (currentStage >= flaureData.EdibleStage)
        {
            isEdible = true;
        }
    }

    public void IsEaten()
    {
        currentFlaureType?.Eaten();
    }


}
