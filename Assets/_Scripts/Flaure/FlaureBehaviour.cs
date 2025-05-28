using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(SpriteRenderer))]
public class FlaureBehaviour : MonoBehaviour
{
    public FlaureType currentFlaureType;
    private SpriteRenderer spriteRenderer;
    public FlaureData flaureData;
    public int currentStage = 0;

    public float currentGrowthTime = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
         GetFlaureTypeInstance();
    } 

     public FlaureType GetFlaureTypeInstance()
    {
        switch (flaureData.factionTypeEnum)
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
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = flaureData.sprites[currentStage];
    }

    void OnEnable()
    {
        StartCoroutine(currentFlaureType.Grow());
    }

    void OnDisable()
    {
        StopAllCoroutines();
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

    private IEnumerator Grow()
    {
        currentGrowthTime++;
        List<TileInfo> waterTiles = new List<TileInfo>();
        List<TileInfo> emptyTiles = new List<TileInfo>();
        List<TileInfo> emptyNatureTiles = new List<TileInfo>();
        for (int dx = -flaureData.range; dx <= flaureData.range; dx++)
        {
            for (int dy = -flaureData.range; dy <= flaureData.range; dy++)
            {
                if (DungeonGenerator.Instance.dungeonMap.TryGetValue(new Vector3Int(dx, dy, 0), out TileInfo tile))
                {
                    if (tile.isWater)
                    {
                        waterTiles.Add(tile);
                    }
                    if (tile.isNature && tile.tile == null)
                    {
                        emptyNatureTiles.Add(tile);
                    }
                    if (tile.tile == null && !tile.isWater && !tile.isNature)
                    {
                        emptyTiles.Add(tile);
                    }
                }
            }
        }
        if (flaureData.needWater)
        {
            currentGrowthTime += waterTiles.Count;
        }
        if (flaureData.needNature)
        {
            currentGrowthTime += emptyNatureTiles.Count;
        }

        if (currentGrowthTime >= flaureData.growingTime)
        {
            currentGrowthTime = 0f;
            if (currentStage < flaureData.growthStages)
            {
                GrowingStage();
            }
            else
            {
                Expand();
            }

        }
        else
        {
            // Not enough time to grow, wait and try again
            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(Grow());
    }




    void GrowingStage()
    {
        currentStage++;
        spriteRenderer.sprite = flaureData.sprites[currentStage];
    }




    void Expand()
    {
        List<TileInfo> deadEndTiles = new List<TileInfo>();
        List<TileInfo> waterTiles = new List<TileInfo>();
        List<TileInfo> emptyTiles = new List<TileInfo>();
        List<TileInfo> natureTiles = new List<TileInfo>();
        for (int dx = -flaureData.range; dx <= flaureData.range; dx++)
        {
            for (int dy = -flaureData.range; dy <= flaureData.range; dy++)
            {
                if (DungeonGenerator.Instance.dungeonMap.TryGetValue(new Vector3Int(dx, dy, 0), out TileInfo tile))
                {
                    if (tile.isWater)
                    {
                        waterTiles.Add(tile);
                    }
                    if (tile.isNature)
                    {
                        natureTiles.Add(tile);
                    }
                    if (tile.tile == null && !tile.isWater && !tile.isNature)
                    {
                        emptyTiles.Add(tile);
                    }
                    if (tile.isDeadEnd)
                    {
                        deadEndTiles.Add(tile);
                    }
                }
            }
        }

        if (flaureData.needWater && flaureData.needNature)
        {
            List<TileInfo> validTiles = new List<TileInfo>();
            for (int i = 0; i < waterTiles.Count; i++)
            {
                for (int j = 0; j < natureTiles.Count; j++)
                {
                    {
                        validTiles.Add(natureTiles[j]);
                    }
                }
            }
        }
    }
}
