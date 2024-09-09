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

public enum BlockFaces
{
    Tricky,
    Anxious,
    Trio,
    Aweary,
    Handsome,
    Pretty,
    Silly,
    Overflow,
    Madness,
    Mike
}
public class Block : MonoBehaviour
{
    [SerializeField] private BlockTypes blockType;
    [SerializeField] private BlockFaces blockFace;
    [SerializeField] private Atom[] atoms;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private bool allowPickUpAfterPlacement = false;

    private List<int[,]> _blockSchemas = new List<int[,]>();
    private Vector3 _originalPosition;
    private Vector3 _originalRotation;
    private Vector3 _originalScale;
    private bool _isPlaced;
    private int _spawnIndex;
    private Tween _transformTween;
    private Coroutine _rotateCoroutine;
    private bool _isDragging;

    public BlockTypes BlockType => blockType;
    public Vector3 OriginalPosition {get => _originalPosition; set => _originalPosition = value;}
    public Vector3 OriginalRotation {get => _originalRotation; set => _originalRotation = value;}
    public Vector3 OriginalScale {get => _originalScale; set => _originalScale = value;}

    public List<int[,]> BlockSchemas => _blockSchemas;
    public Atom[] Atoms => atoms;
    public bool AllowPickUpAfterPlacement => allowPickUpAfterPlacement;
    public int SpawnIndex {get => _spawnIndex; set => _spawnIndex = value;}

    private void Start()
    {
        foreach (var atom in atoms)
        {
            atom.ParentBlock = this;
        }
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
        if (_isPlaced) yield break;
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
        if (!spriteRenderer)
        {
            foreach (var atom in atoms)
            {
                atom.SpriteRenderer.sortingOrder = order;
            }
            return;
        }
        spriteRenderer.sortingOrder = order;
    }
    
    private void OnMouseEnter()
    {
        if (_isPlaced && !AllowPickUpAfterPlacement) return;
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) return;
        if (_isDragging) return;
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockHover, out _);
    }

    private void OnMouseDrag()
    {
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver)
        {
            StartCoroutine(OnMouseUp());
            return;
        }
        if (_isPlaced && !AllowPickUpAfterPlacement) return;
        HandleBlockManipulation();
        GridManager.Instance.ValidatePlacement(this);
        if (_isDragging) return; //Prevent unnecessary calculations
        PointerManager.Instance.SelectBlock(this);
        PickUpBlock();
        GridManager.Instance.RemoveBlock(this);
        SoundManager.Instance.PlayBlockFaceFX(blockFace, out _);
        _isDragging = true;
    }

    /// <summary>
    /// Handle rotation of the block
    /// </summary>
    private void HandleBlockManipulation()
    {
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     _parentBlock.transform.Rotate(0, 0, 90);
        // }
        if (Input.GetMouseButtonDown(1))
        {
            RotateBlock(-90);
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockRotate, out _);
        }
        // if (Input.GetKeyDown(KeyCode.F))
        // {
        //     var blockTransform = _parentBlock.transform;
        //     var localScale = blockTransform.localScale;
        //     localScale = new Vector3(localScale.x * -1,
        //         localScale.y, localScale.z);
        //     blockTransform.localScale = localScale;
        // }
    }
    
    private IEnumerator OnMouseUp()
    {
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) yield break;
        if (!_isDragging) yield break;
        PointerManager.Instance.DeselectBlock();
        if (_rotateCoroutine != null)
            yield return new WaitUntil(() => _rotateCoroutine == null);
        if (GridManager.Instance.PlaceBlock(this))
        {
            _isPlaced = true;
            SetRendererSortingOrder(1);
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockPlaced, out _);
            RandomBlock.Instance.FreeSpawnPoint(_spawnIndex);
            RandomBlock.Instance.DestroyBlock();
            RandomBlock.Instance.SpawnRandomBlock();
            if (GameManager.Instance.CurrentReRoll <= 0)
            {
                RandomBlock.Instance.GameOverCheck();
            }
        }
        else
        {
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockCancel, out _);
            ReturnToOriginal();
            _isPlaced = false;
        }
        _isDragging = false;
    }
}
