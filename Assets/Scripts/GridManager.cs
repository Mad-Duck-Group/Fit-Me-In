using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    private static GridManager _instance;
    public static GridManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Grid Manager is null");
            }
            return _instance;
        }
    }
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int cellSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellParent;
    private Cell[,] _cellArray;
    private List<Cell> _previousValidationCells = new List<Cell>();

    /// <summary>
    /// Get the cell by index
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>Cell</returns>
    public Cell GetCellByIndex(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }
        return _cellArray[x, y];
    }
    
    /// <summary>
    /// Get the cell by position, it will be rounded to the nearest cell
    /// </summary>
    /// <param name="position">Position to try to get a cell</param>
    /// <returns>Cell</returns>
    public Cell GetCellByPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt((position.x - offset.x) / cellSize);
        int y = Mathf.RoundToInt((position.y - offset.y) / cellSize);
        return GetCellByIndex(x, y);
    }
    
    private void Awake()
    {
        _instance = this;
    }
    
    void Start()
    {
        CreateGrid();
    }
    
    /// <summary>
    /// Create the grid
    /// </summary>
    private void CreateGrid()
    {
        _cellArray = new Cell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 spawnPosition = offset + new Vector2(x, y) * cellSize;
                _cellArray[x, y] = Instantiate(cellPrefab, spawnPosition, Quaternion.identity).GetComponent<Cell>();
                _cellArray[x, y].transform.localScale = Vector3.one * cellSize;
                _cellArray[x, y].name = "Cell " + x + ", " + y;
                _cellArray[x, y].transform.parent = cellParent.transform;
                
                //Chessboard Pattern
                if (x % 2 == 0)
                {
                    _cellArray[x, y].SpriteRenderer.color =
                        y % 2 == 0 ? new Color32(0, 0, 0, 255) : new Color32(255, 255, 255, 255);
                    _cellArray[x, y].OriginalColor = _cellArray[x, y].SpriteRenderer.color;
                }
                else
                {
                    _cellArray[x, y].SpriteRenderer.color =
                        y % 2 == 0 ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 255);
                    _cellArray[x, y].OriginalColor = _cellArray[x, y].SpriteRenderer.color;
                }
            }
        }
    }
    
    /// <summary>
    /// Validate the placement of the block and change the color of the cells
    /// </summary>
    /// <param name="block">Block to validate</param>
    /// <returns>true if the placement is valid, false otherwise</returns>
    public bool ValidatePlacement(Block block)
    {
        List<Cell> cells = new List<Cell>();
        foreach (var atom in block.Atoms)
        {
            Vector3 atomPosition = atom.transform.position;
            Debug.Log(atomPosition);
            Vector3 cellPosition = new Vector3(atomPosition.x, atomPosition.y, 0);
            Debug.Log(cellPosition);
            Cell cell = GetCellByPosition(cellPosition);
            if (cell == null || cell.CurrentAtom != null)
            {
                continue;
            }
            cells.Add(cell);
        }
        if (_previousValidationCells.Count > 0)
        {
            ResetPreviousValidationCells();
        }
        _previousValidationCells = cells;
        if (cells.Count < block.Atoms.Length)
        {
            cells.ForEach(cell => cell.SpriteRenderer.color = new Color32(255, 0, 0, 255));
            return false;
        }
        cells.ForEach(cell => cell.SpriteRenderer.color = new Color32(0, 255, 0, 255));
        return true;
    }
    
    /// <summary>
    /// Place the block in the grid
    /// </summary>
    /// <param name="block">Block to place</param>
    /// <returns>true if the placement is valid, false otherwise</returns>
    public bool PlaceBlock(Block block)
    {
        Vector3 atomPositionBeforePlacement = block.Atoms[0].transform.position;
        List<Cell> cells = new List<Cell>();
        foreach (var atom in block.Atoms)
        {
            Vector3 atomPosition = atom.transform.position;
            Debug.Log(atomPosition);
            Vector3 cellPosition = new Vector3(atomPosition.x, atomPosition.y, 0);
            Debug.Log(cellPosition);
            Cell cell = GetCellByPosition(cellPosition);
            if (cell == null || cell.CurrentAtom != null)
            {
                return false;
            }
            cells.Add(cell);
        }
        for (var i = 0; i < block.Atoms.Length; i++)
        {
            var atom = block.Atoms[i];
            cells[i].SetAtom(atom);
            atom.SpriteRenderer.sortingOrder = 1;
        }
        Vector3 atomPositionAfterPlacement = cells[0].transform.position;
        Vector3 blockPositionRelativeToAtom = atomPositionAfterPlacement - atomPositionBeforePlacement;
        block.transform.position += blockPositionRelativeToAtom;
        return true;
    }
    
    /// <summary>
    /// Remove the block from the grid
    /// </summary>
    /// <param name="block">Block to remove</param>
    public void RemoveBlock(Block block)
    {
        foreach (var atom in block.Atoms)
        {
            Cell cell = GetCellByPosition(atom.transform.position);
            if (cell == null || cell.CurrentAtom != atom)
            {
                continue;
            }
            cell.SetAtom(null);
            atom.SpriteRenderer.sortingOrder = 2;
        }
    }
    
    /// <summary>
    /// Reset the color of the previous validation cells
    /// </summary>
    public void ResetPreviousValidationCells()
    {
        _previousValidationCells.ForEach(cell => cell.SpriteRenderer.color = cell.OriginalColor);
        _previousValidationCells.Clear();
    }

    /// <summary>
    /// Draw the grid boundaries in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 realCenter = new Vector3(width, height) * cellSize * 0.5f;
        Gizmos.DrawWireCube((realCenter + (Vector3)offset) - transform.localScale * 0.5f, new Vector3(width * cellSize, height * cellSize, 1));
    }
}
