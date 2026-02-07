using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(BotShipPlacementData))]
public class BotShipPlacementEditor : Editor                    //This custom editor helps to load data from scene placed-ships into BotShipPlacementSO
{
    public List<Transform> ships = new();

    private Tilemap tilemap;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Bot Tilemap", EditorStyles.boldLabel);

        tilemap = (Tilemap)EditorGUILayout.ObjectField("Tilemap", tilemap, typeof(Tilemap), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Bot Ships", EditorStyles.boldLabel);

        int removeIndex = -1;

        for (int i = 0; i < ships.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            ships[i] = (Transform)EditorGUILayout.ObjectField($"Ship {i}", ships[i], typeof(Transform), true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
                removeIndex = i;

            EditorGUILayout.EndHorizontal();
        }

        if (removeIndex >= 0)
            ships.RemoveAt(removeIndex);

        if (GUILayout.Button("Add Ship Slot"))
            ships.Add(null);

        EditorGUILayout.Space();

        GUI.enabled = tilemap != null;

        if (GUILayout.Button("Capture Cell Placements"))
        {
            CaptureShips();
        }

        GUI.enabled = true;
    }

    private void CaptureShips()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap missing!");
            return;
        }

        BotShipPlacementData data = (BotShipPlacementData)target;

        List<BotShipPlacementData.Ship> captured = new();

        foreach (var t in ships)
        {
            if (t == null)
                continue;

            BotShipController controller = t.GetComponent<BotShipController>();

            if (controller == null)
            {
                Debug.LogWarning($"No ShipController on {t.name}");
                continue;
            }

            BotShipPlacementData.Ship ship = new BotShipPlacementData.Ship();

            Vector3Int cell = tilemap.WorldToCell(t.position);

            ship.cellPosition = cell;
            ship.size = controller.shipSize;

            ship.rotatedAngle = t.eulerAngles.z;

            captured.Add(ship);
        }

        data.ships = captured.ToArray();

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        Debug.Log($"Captured {captured.Count} ships (CELL positions).");
    }

}

