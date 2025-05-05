using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    public RoomFirstDungeonGenerator roomGenerator;
    public GameObject mobPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnTraversingMobs());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator SpawnTraversingMobs()
    {
        
            Vector2Int deadEndBorder = roomGenerator.deadEndsBorders[Random.Range(0, roomGenerator.deadEndsBorders.Count)];
            Vector2 spawnPosition = new Vector2(deadEndBorder.x, deadEndBorder.y);
            GameObject mob = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);
            Vector2Int deadEndBorderDestination = roomGenerator.deadEndsBorders[Random.Range(0, roomGenerator.deadEndsBorders.Count)];
            Vector2 destinationEnd = new Vector2(deadEndBorderDestination.x, deadEndBorderDestination.y);
            mob.GetComponent<CreatureState>().destination = destinationEnd;
            yield return new WaitForSeconds(1f); // Attendre 1 seconde entre chaque spawn
        
    }
}
