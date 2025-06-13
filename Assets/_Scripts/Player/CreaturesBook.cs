using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreaturesBook : MonoBehaviour
{

    public static CreaturesBook Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    FactionBehaviour dungeonFaction;
    FactionType dungeonAssignator;
    bool init;

    public TMP_Text unassignedGobs, recolterGobs, patrolerGobs, escorterGobs;
    public int unassignedGobsCount, recolterGobsCount, patrolerGobsCount, escorterGobsCount;

    public Button recolterGobsPlus, recolterGobsMinus, patrolerGobsPlus, patrolerGobsMinus, escorterGobsPlus, escorterGobsMinus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (FactionSpawner.instance != null && FactionSpawner.instance.dungeonFaction != null && !init)
        {
            init = true;

                    recolterGobsPlus.onClick.AddListener(() => ReassignMembers(recolterGobsPlus));
                    recolterGobsMinus.onClick.AddListener(() => ReassignMembers(recolterGobsMinus));
                    patrolerGobsPlus.onClick.AddListener(() => ReassignMembers(patrolerGobsPlus));
                    patrolerGobsMinus.onClick.AddListener(() => ReassignMembers(patrolerGobsMinus));
                    escorterGobsPlus.onClick.AddListener(() => ReassignMembers(escorterGobsPlus));
                    escorterGobsMinus.onClick.AddListener(() => ReassignMembers(escorterGobsMinus));
        }
        if (init)
        {
            dungeonFaction = FactionSpawner.instance.dungeonFaction;
            dungeonAssignator = dungeonFaction.GetFactionTypeInstance();

            unassignedGobsCount = dungeonFaction.gobelins.Count - (dungeonFaction.membersEscort.Count + dungeonFaction.membersRecoltFood.Count + dungeonFaction.membersPatrol.Count);
            unassignedGobs.text = unassignedGobsCount.ToString();

            recolterGobsCount = dungeonFaction.membersRecoltFood.Count;
            recolterGobs.text = recolterGobsCount.ToString();

            patrolerGobsCount = dungeonFaction.membersPatrol.Count;
            patrolerGobs.text = patrolerGobsCount.ToString();

            escorterGobsCount = dungeonFaction.membersEscort.Count;
            escorterGobs.text = escorterGobsCount.ToString();
        }
    }

    public void ReassignMembers(Button button)
    {
        switch (button.name)
        {
            case "recolterPLUS":
            if (unassignedGobsCount > 0)
                {
                    unassignedGobsCount--;
                    recolterGobsCount++;
                    dungeonAssignator.AssignGoblinToRecolter();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    recolterGobs.text = recolterGobsCount.ToString();
                }
                break;
            case "recolterMOINS":
                if (recolterGobsCount > 0)
                {
                    unassignedGobsCount++;
                    recolterGobsCount--;
                    dungeonAssignator.UnAssignGoblinToEscorter();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    recolterGobs.text = recolterGobsCount.ToString();
                }
                break;
            case "patrolerPLUS":
            if (unassignedGobsCount > 0)
                {
                    unassignedGobsCount--;
                    patrolerGobsCount++;
                    dungeonAssignator.AssignGoblinToPatroler();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    patrolerGobs.text = patrolerGobsCount.ToString();
                }
                break;
            case "patrolerMOINS":
                if (patrolerGobsCount > 0)
                {
                    unassignedGobsCount++;
                    patrolerGobsCount--;
                    dungeonAssignator.UnAssignGoblinToPatroler();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    patrolerGobs.text = patrolerGobsCount.ToString();
                }
                break;
            case "escorterPLUS":
            if (unassignedGobsCount > 0)
                {
                    unassignedGobsCount--;
                    escorterGobsCount++;
                    dungeonAssignator.AssignGoblinToEscorter();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    escorterGobs.text = escorterGobsCount.ToString();
                }
                break;
            case "escorterMOINS":
                if (escorterGobsCount > 0)
                {
                    unassignedGobsCount++;
                    escorterGobsCount--;
                    dungeonAssignator.UnAssignGoblinToEscorter();
                    unassignedGobs.text = unassignedGobsCount.ToString();
                    escorterGobs.text = escorterGobsCount.ToString();
                }
                break;
        }

    }
}
