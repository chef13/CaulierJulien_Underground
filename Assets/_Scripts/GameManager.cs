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
        //StartCoroutine(SpawnTraversingMobs());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {Vector2 moussePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (BlopSpawner.Instance != null)
            {
                BlopSpawner.Instance.SpawnBlop(moussePosition);
            }
            else
            {
                Debug.LogWarning("BlopSpawner.Instance is null!");
            }
            
        }
    }

    public IEnumerator SpawnTraversingMobs()
    {
        
            
            Vector3 spawnPosition = roomGenerator.spawnPoints[Random.Range(0, roomGenerator.deadEndsBorders.Count)].transform.position;
            GameObject mob = Instantiate(mobPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Spawned mob at: " + spawnPosition + "at spawnPoint: " + roomGenerator.spawnPoints[Random.Range(0, roomGenerator.deadEndsBorders.Count)].transform.position);
            yield return new WaitForSeconds(5f); // Attendre 1 seconde entre chaque spawn
            StartCoroutine(SpawnTraversingMobs());
            
    }

}
