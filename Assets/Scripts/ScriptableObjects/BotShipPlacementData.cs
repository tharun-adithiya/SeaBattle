using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BotShipPlacementData", menuName = "Scriptable Objects/BotShipPlacementData")]
public class BotShipPlacementData : ScriptableObject
{
    public Ship[] ships;
    [Serializable]
    public class Ship
    {
        public Vector3Int cellPosition;
        public float rotatedAngle;
        public int size;
        public bool isHorizontal;
    }
}
