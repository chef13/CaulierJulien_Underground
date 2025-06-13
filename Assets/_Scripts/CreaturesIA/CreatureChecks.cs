using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CreatureChecks : MonoBehaviour
{

    public CreatureSpawner creatureSpawner;
    public int creatureBatchSize = 1;
    public int maxCreatureBatchSize = 10; // Maximum number of creatures to check in one batch
    private List<CreatureController> livingCreatures = new List<CreatureController>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Coroutine checkCreatureMasterCoroutine;
    public float coroutineDelay = 0.5f; // Adjust this value as needed
    void Start()
    {
        livingCreatures = creatureSpawner.livingCreatures;
        checkCreatureMasterCoroutine = StartCoroutine(CheckCreatureMaster());
    }

    // Update is called once per frame
    void Update()
    {
        if (checkCreatureMasterCoroutine == null)
        {
            checkCreatureMasterCoroutine = StartCoroutine(CheckCreatureMaster());
        }
    }

    private IEnumerator CheckCreatureMaster()
    {
         yield return new WaitForSeconds(5f);
        int index = 0;
        //Debug.Log("CheckCreatureMaster coroutine started");
        while (true)
        {
            livingCreatures = creatureSpawner.livingCreatures;
            //Debug.Log($"livingCreatures.Count: {livingCreatures.Count}, index: {index}");
            if (livingCreatures.Count > 0)
            {
                int creatureBatch = creatureBatchSize;
                int checkedCount = 0;
                while (creatureBatch > 0 & checkedCount < maxCreatureBatchSize)
                {
                    if (index >= livingCreatures.Count)
                        index = 0;
                        var creature = livingCreatures[index];
                        if (creature != null && creature.coroutineDelay > 0.1f)
                        {
                            index = (index + 1) % livingCreatures.Count;
                        }
                        else if (creature != null && creature.isActiveAndEnabled && creature.coroutineDelay <= 0.1f)
                        {
                            //Debug.Log($"Checking Creature {creature.name} for coroutine for livingCreatures.Count: {livingCreatures.Count}, index: {index}");
                            creature.AllCheckForCoroutine();
                            creature.coroutineDelay = coroutineDelay;
                            creatureBatch--;
                        }

                        index = (index + 1) % livingCreatures.Count;
                    checkedCount++;
                }
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
