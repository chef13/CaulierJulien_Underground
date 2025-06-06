using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CreatureChecks : MonoBehaviour
{

    public CreatureSpawner creatureSpawner;
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
        int index = 0;
        //Debug.Log("CheckCreatureMaster coroutine started");
        while (true)
        {
            livingCreatures = creatureSpawner.livingCreatures;
            //Debug.Log($"livingCreatures.Count: {livingCreatures.Count}, index: {index}");
            if (livingCreatures.Count > 0)
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
                    yield return new WaitForEndOfFrame();
                }

                index = (index + 1) % livingCreatures.Count;
            }

            yield return new WaitForEndOfFrame();
        }
    }
}
