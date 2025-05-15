using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
    AbstractDungeonGenerator generator;

    private void Awake()
    {
        generator = (AbstractDungeonGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Create Dungeon"))
        {
            generator.GenerateDungeon();
        }


        RoomFirstDungeonGenerator gen = (RoomFirstDungeonGenerator)target;
        if (GUILayout.Button("Log TileInfoDict"))
        {
            foreach (var kvp in gen.tileInfoDict)
            {
                Debug.Log($"Tile at {kvp.Key}: Water={kvp.Value.isWater}, Nature={kvp.Value.isNature}, DeadEnd={kvp.Value.isDeadEnd}");
            }
        }
    }

    
}
