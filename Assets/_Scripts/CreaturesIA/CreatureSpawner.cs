using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.AI;
public class CreatureSpawner : MonoBehaviour
{
    public static CreatureSpawner Instance;
    
    public List<GameObject> creaturesGarbage = new List<GameObject>(); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator SpawnCreatureInRoom(Vector2 pos, GameObject creaturePrefab, FactionBehaviour faction)
    {

        if (creaturesGarbage.Count == 0)
        {
            GameObject creatureGO = GameObject.Instantiate(creaturePrefab, pos, Quaternion.identity);
            creatureGO.GetComponent<NavMeshAgent>().enabled = true;
            creatureGO.SetActive(false);
            creatureGO.transform.SetParent(faction.transform);
            CreatureController controller = creatureGO.GetComponent<CreatureController>();
            controller.currentFaction = faction;
            faction.members.Add(creatureGO); // optional: track units
            yield return new WaitForSeconds(0.1f); // wait a bit to ensure the creature is fully initialized
            creatureGO.SetActive(true);
        }
        else
        {
            GameObject creatureGO = creaturesGarbage[0];
            creatureGO.GetComponent<NavMeshAgent>().enabled = true;
            creaturesGarbage.RemoveAt(0);
            creatureGO.transform.position = pos;
            creatureGO.SetActive(false);
            creatureGO.transform.SetParent(faction.transform);
            CreatureController controller = creatureGO.GetComponent<CreatureController>();
            controller.currentFaction = faction;
            faction.members.Add(creatureGO); // optional: track units
            yield return new WaitForSeconds(0.1f); // wait a bit to ensure the creature is fully initialized
            creatureGO.SetActive(true);
        }
    }
}
