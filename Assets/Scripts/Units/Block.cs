using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;


public enum BlockTypes
{
    Topten,
    Jelly,
    Pan,
    Sankaya
}
public class Block : MonoBehaviour
{
    [SerializeField] private BlockTypes blockType;
    [SerializeField] private Atom[] atoms;
    [SerializeField] private bool allowPickUpAfterPlacement = false;

    private List<int[,]> _blockSchemas = new List<int[,]>();
    private Vector3 _originalPosition;
    private Vector3 _originalRotation;
    private Vector3 _originalScale;
    private bool _isPlaced;
    private int _spawnIndex;
    private float _previousRotation;
    private Tween _transformTween;
    private Coroutine _rotateCoroutine;

    public BlockTypes BlockType => blockType;
    public Vector3 OriginalPosition {get => _originalPosition; set => _originalPosition = value;}
    public Vector3 OriginalRotation {get => _originalRotation; set => _originalRotation = value;}
    public Vector3 OriginalScale {get => _originalScale; set => _originalScale = value;}
    
    public List<int[,]> BlockSchemas => _blockSchemas;
    public Atom[] Atoms => atoms;
    public bool AllowPickUpAfterPlacement => allowPickUpAfterPlacement;
    public bool IsPlaced {get => _isPlaced; set => _isPlaced = value;}
    public int SpawnIndex {get => _spawnIndex; set => _spawnIndex = value;}
    public Coroutine RotateCoroutine => _rotateCoroutine;

    private void Start()
    {
        foreach (var atom in atoms)
        {
            atom.ParentBlock = this;
        }
        _previousRotation = transform.eulerAngles.z;
    }

    
    /// <summary>
    /// Generate the schema of the block, 1 is an atom, 0 is empty
    /// </summary>
    [Button("Test Schema")]
    public void GenerateSchema()
    {
        Vector3 currentScale = transform.localScale;
        transform.localScale = Vector3.one;
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
        _blockSchemas.Add(ArrayHelper.Rotate180(originalSchema));
        _blockSchemas.Add(ArrayHelper.Rotate90(_blockSchemas[0]));
        _blockSchemas.Add(originalSchema);
        _blockSchemas.Add(ArrayHelper.Rotate270(_blockSchemas[0]));
        _blockSchemas = _blockSchemas.Distinct().ToList(); //Remove duplicates
        transform.localScale = currentScale;
    }
    
    public void RotateBlock(float angle)
    {
        if (_rotateCoroutine != null)
        {
            return;
        }
        _rotateCoroutine = StartCoroutine(Rotate(angle));
    }

    private IEnumerator Rotate(float angle)
    {
        if (IsPlaced) yield break;
        Vector3 currentRotation = transform.eulerAngles;
        float newAngle = currentRotation.z + angle;
        // if (_rotationTween.IsActive())
        // {
        //     _rotationTween.Kill();
        //     transform.eulerAngles = new Vector3(0, 0, _previousRotation);
        // }
        // _rotationTween = transform.DORotate(new Vector3(0, 0, newAngle), 0.2f);
        float timer = 0;
        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            transform.eulerAngles = Vector3.Lerp(currentRotation, new Vector3(0, 0, newAngle), timer / 0.1f);
            yield return null;
        }
        _previousRotation = newAngle;
        _rotateCoroutine = null;
    }

    public void PickUpBlock()
    {
        //Tween the block to (1, 1, 1) scale
        if (_transformTween.IsActive())
        {
            _transformTween.Kill();
        }
        _transformTween = transform.DOScale(Vector3.one, 0.2f);
    }

    /// <summary>
    /// Return the block to its original position, rotation and scale
    /// </summary>
    public void ReturnToOriginal()
    {
        var blockTransform = transform;
        //Tween the block to the original position
        if (_transformTween.IsActive())
        {
            _transformTween.Kill();
        }
        _transformTween = blockTransform.DOMove(_originalPosition, 0.2f).OnComplete(() => SetRendererSortingOrder(1));
        //Tween the block to the original rotation
        blockTransform.DORotate(_originalRotation, 0.2f);
        //Tween the block to the original scale
        blockTransform.DOScale(_originalScale, 0.2f);
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
