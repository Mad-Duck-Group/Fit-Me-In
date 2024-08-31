using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs("width")] [SerializeField] private int column;
    [FormerlySerializedAs("height")] [SerializeField] private int row;
    [SerializeField] private int cellSize;
    [SerializeField] private Vector2 offset;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject cellParent;
    [SerializeField] private Block[] blockPrototypes;
    private Cell[,] _cellArray;
    private List<Cell> _previousValidationCells = new List<Cell>();
    private int[,] _vacantSchema;

    /// <summary>
    /// Get the cell by index
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>A Cell if it exists, null otherwise</returns>
    public Cell GetCellByIndex(int x, int y)
    {
        if (x < 0 || x >= row || y < 0 || y >= column)
        {
            return null;
        }
        return _cellArray[x, y];
    }
    
    /// <summary>
    /// Get the cell by position, it will be rounded to the nearest cell
    /// </summary>
    /// <param name="position">Position to try to get a cell</param>
    /// <returns>A Cell if it exists, null otherwise</returns>
    public Cell GetCellByPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt((offset.y - position.y) / cellSize);
        int y = Mathf.RoundToInt((position.x - offset.x) / cellSize);
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
        _cellArray = new Cell[row, column];
        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < column; y++)
            {
                Vector3 spawnPosition = offset + new Vector2(y, -x) * cellSize;
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
            Vector3 cellPosition = new Vector3(atomPosition.x, atomPosition.y, 0);
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
            Vector3 cellPosition = new Vector3(atomPosition.x, atomPosition.y, 0);
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
    /// Create a schema of the vacant cells, 1 if the cell is vacant, 0 otherwise
    /// </summary>
    private void CreateVacantSchema()
    {
        _vacantSchema = new int[row, column];
        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < column; y++)
            {
                if (_cellArray[x, y].CurrentAtom == null)
                {
                    _vacantSchema[x, y] = 1;
                }
            }
        }
        ArrayHelper.PrintSchema(_vacantSchema);
    }

    /// <summary>
    /// Check which blocks can be placed in the grid
    /// </summary>
    [Button("CheckAvailableShape")]
    private void CheckAvailableShape()
    {
        CreateVacantSchema();
        foreach (var block in blockPrototypes)
        {
            if (CompareSchema(block)) continue;
            Debug.Log("Block " + block.name + " cannot be placed");
        }
    }

    /// <summary>
    /// Compare the schema of the block with the vacant schema
    /// </summary>
    /// <param name="block">Block to compare</param>
    /// <returns>true if the block can be placed, false otherwise</returns>
    private bool CompareSchema(Block block)
    {
        foreach (var schema in block.BlockSchemas)
        {
            if (ArrayHelper.CanBlockFitInVacant(_vacantSchema, schema))
            {
                Debug.Log("Block " + block.name + " can be placed");
                return true;
            }
        }
        return false;
    }
    

    /// <summary>
    /// Draw the grid boundaries in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 realCenter = new Vector3(column, -row) * cellSize * 0.5f;
        Vector3 halfSize = new Vector3(-cellSize, cellSize, 1) * 0.5f;
        Gizmos.DrawWireCube((realCenter + (Vector3)offset) + halfSize, new Vector3(column * cellSize, row * cellSize, 1));
    }
}
