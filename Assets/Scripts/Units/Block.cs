using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private Atom[] atoms;

    private int[][,] _blockSchemas = new int[4][,];
    
    private Vector3 _originalPosition;
    private Vector3 _originalRotation;
    private Vector3 _originalScale;
    
    public int[][,] BlockSchemas => _blockSchemas;

    public Atom[] Atoms => atoms;

    private void Awake()
    {
        foreach (var atom in atoms)
        {
            atom.ParentBlock = this;
        }
        var blockTransform = transform;
        _originalPosition = blockTransform.position;
        _originalRotation = blockTransform.eulerAngles;
        _originalScale = blockTransform.localScale;
        GenerateSchema();
    }

    /// <summary>
    /// Generate the schema of the block, 1 is an atom, 0 is empty
    /// </summary>
    public void GenerateSchema()
    {
        Atom[] sortByX = Atoms.OrderByDescending(atom => atom.transform.position.x).ToArray();
        Atom[] sortByY = Atoms.OrderByDescending(atom => atom.transform.position.y).ToArray();
        Atom mostRight = sortByX.First();
        Atom mostLeft = sortByX.Last();
        Atom mostUp = sortByY.First();
        Atom mostDown = sortByY.Last();
        int column = Mathf.RoundToInt(mostRight.transform.position.x - mostLeft.transform.position.x) + 1;
        int row = Mathf.RoundToInt(mostUp.transform.position.y - mostDown.transform.position.y) + 1;
        int[,] originalSchema = new int[row, column];
        Debug.Log("row: " + row + " column: " + column);
        foreach (var atom in Atoms)
        {
            int x = Mathf.RoundToInt(mostRight.transform.position.x - atom.transform.position.x);
            int y = Mathf.RoundToInt(atom.transform.position.y - mostDown.transform.position.y);
            originalSchema[y, x] = 1;
        }
        _blockSchemas[0] = ArrayHelper.Rotate180(originalSchema);
        _blockSchemas[1] = ArrayHelper.Rotate90(_blockSchemas[0]);
        _blockSchemas[2] = originalSchema;
        _blockSchemas[3] = ArrayHelper.Rotate270(_blockSchemas[0]);
    }

    /// <summary>
    /// Return the block to its original position, rotation and scale
    /// </summary>
    public void ReturnToOriginal()
    {
        var blockTransform = transform;
        blockTransform.position = _originalPosition;
        blockTransform.eulerAngles = _originalRotation;
        blockTransform.localScale = _originalScale;
        GridManager.Instance.ResetPreviousValidationCells();
    }
    
    /// <summary>
    /// Set the sorting order of atoms
    /// </summary>
    /// <param name="order">Order to render</param>
    public void SetRendererSortingOrder(int order)
    {
        foreach (var atom in atoms)
        {
            atom.SpriteRenderer.sortingOrder = order;
        }
    }
}
