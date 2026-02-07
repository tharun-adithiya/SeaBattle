using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public static int placedShipCount = 0;
    public static event Action OnPlacedAllShips;

    [SerializeField] private Tilemap playerbaseTilemap;
    [SerializeField] private BoxCollider2D boardBounds;
    [SerializeField] private GameObject highlightTilePrefab;

    private List<GameObject> placedShips = new();
    private List<GameObject> highlightPool = new();

    // OLD
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    // NEW
    private List<ShipData> ships = new();

    private void TriggerOnPlacedAllShips()
    {
        Debug.Log("Fired " + OnPlacedAllShips);
        OnPlacedAllShips?.Invoke();
    }

    public Vector3Int GetStartCell(Vector3 worldPosition, int shipSize, bool isHorizontal)
    {
        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        return playerbaseTilemap.WorldToCell(worldPosition - offsetVector);
    }

    public bool CanPlaceShip(Vector3Int startCell, int shipSize, bool isHorizontal, out List<Vector3Int> cells)
    {
        cells = new();

        if (shipSize <= 0)
            return false;

        for (int i = 0; i < shipSize; i++)
        {
            Vector3Int cell = startCell +
                (isHorizontal ? new Vector3Int(i, 0, 0) : new Vector3Int(0, i, 0));

            if (playerbaseTilemap.GetTile(cell) == null)
                return false;

            Vector3 worldPos = playerbaseTilemap.GetCellCenterWorld(cell);

            if (!boardBounds.bounds.Contains(worldPos))
                return false;

            if (occupiedCells.Contains(cell))
                return false;

            cells.Add(cell);
        }

        return true;
    }

    // MODIFIED
    public void OccupyCells(List<Vector3Int> cells)
    {
        foreach (var cell in cells)
            occupiedCells.Add(cell);

        ShipData ship = new ShipData();
        ship.cells.AddRange(cells);

        ships.Add(ship);
    }

    public void RegisterShipsOnBoard(ShipController ship)
    {
        if (placedShipCount <= 2)
        {
            placedShipCount++;
            placedShips.Add(ship.gameObject);
        }

        if (placedShipCount >= 2)
            TriggerOnPlacedAllShips();
    }

    public void LockShips()
    {
        foreach (GameObject ship in placedShips)
        {
            ship.GetComponent<ShipController>().enabled = false;
            ship.GetComponent<BoxCollider2D>().enabled = true;

            Color c = ship.GetComponent<SpriteRenderer>().color;
            c.a = 0.57f;
            ship.GetComponent<SpriteRenderer>().color = c;
        }

        GameManager.Instance.SetGameState(GameState.BotPlacementTurn);
    }

    public void FreeCells(List<Vector3Int> cells)
    {
        foreach (var cell in cells)
            occupiedCells.Remove(cell);
    }

    public Vector3 GetWorldFromCell(Vector3Int cell)
    {
        return playerbaseTilemap.GetCellCenterWorld(cell);
    }

    public void UpdateHighlights(Vector3Int startCell, int shipSize, bool isHorizontal)
    {
        ClearHighlights();

        if (CanPlaceShip(startCell, shipSize, isHorizontal, out List<Vector3Int> shipCells))
        {
            for (int i = 0; i < shipCells.Count; i++)
            {
                GameObject highlight = GetHighlight(i);
                highlight.transform.position = GetWorldFromCell(shipCells[i]);
                highlight.SetActive(true);
            }
        }
    }

    GameObject GetHighlight(int index)
    {
        if (index >= highlightPool.Count)
        {
            GameObject obj = Instantiate(highlightTilePrefab);
            highlightPool.Add(obj);
        }

        return highlightPool[index];
    }

    public void ClearHighlights()
    {
        foreach (var h in highlightPool)
            h.SetActive(false);
    }

    public BoxCollider2D GetBounds() => boardBounds;

    public ShipData GetShipAt(Vector3Int cell)
    {
        foreach (var ship in ships)
        {
            if (ship.Contains(cell))
                return ship;
        }

        return null;
    }

    public void RegisterHit(Vector3Int cell)
    {
        ShipData ship = GetShipAt(cell);

        if (ship == null)
            return;

        ship.RegisterHit();
        occupiedCells.Remove(cell);

        if (ship.IsSunk())
        {
            Debug.Log("Ship sunk!");

            if (ship.WasShipDestroyedInATurn())
            {
                Debug.Log("Explosive Shot unlocked for bot!");
                // powerup here
                ShootController.isPowerShotActivatedForBot = true;
            }
        }
    }
    public void ResetTurnHits()
    {
        foreach (var ship in ships)
            ship.ResetTurnHits();
    }
}
