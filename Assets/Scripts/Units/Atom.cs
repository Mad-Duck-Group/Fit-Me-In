using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Atom : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Block _parentBlock;
    private bool _isDragging;
    public Block ParentBlock {get => _parentBlock; set => _parentBlock = value;}
    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseEnter()
    {
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) return;
        SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockHover, out _);
    }

    private void OnMouseDrag()
    {
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) return;
        if (GameManager.Instance.IsGameOver)
        {
            OnMouseUp();
            return;
        }
        if (_parentBlock.IsPlaced && !_parentBlock.AllowPickUpAfterPlacement) return;
        HandleBlockManipulation();
        GridManager.Instance.ValidatePlacement(_parentBlock);
        if (_isDragging) return; //Prevent unnecessary calculations
        PointerManager.Instance.SelectBlock(_parentBlock);
        GridManager.Instance.RemoveBlock(_parentBlock);
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
            _parentBlock.transform.Rotate(0, 0, -90);
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
    
    private void OnMouseUp()
    {
        if (!GameManager.Instance.GameStarted || GameManager.Instance.IsPaused) return;
        if (!_isDragging) return;
        PointerManager.Instance.DeselectBlock();
        if (GridManager.Instance.PlaceBlock(_parentBlock))
        {
            _parentBlock.IsPlaced = true;
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockPlaced, out _);
            RandomBlock.Instance.FreeSpawnPoint(_parentBlock.SpawnIndex);
            RandomBlock.Instance.SpawnRandomBlock();
            RandomBlock.Instance.GameOverCheck();
        }
        else
        {
            SoundManager.Instance.PlaySoundFX(SoundFXTypes.BlockCancel, out _);
            _parentBlock.ReturnToOriginal();
            _parentBlock.IsPlaced = false;
        }
        _isDragging = false;
    }
}
