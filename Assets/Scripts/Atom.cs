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

    private void OnMouseDrag()
    {
        HandleBlockManipulation();
        GridManager.Instance.ValidatePlacement(_parentBlock);
        if (_isDragging) return; //Prevent unnecessary calculations
        PointerManager.Instance.SelectBlock(_parentBlock);
        GridManager.Instance.RemoveBlock(_parentBlock);
        _isDragging = true;
    }

    /// <summary>
    /// Handle rotation and flip of the block
    /// </summary>
    private void HandleBlockManipulation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _parentBlock.transform.Rotate(0, 0, 90);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _parentBlock.transform.Rotate(0, 0, -90);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            var blockTransform = _parentBlock.transform;
            var localScale = blockTransform.localScale;
            localScale = new Vector3(localScale.x * -1,
                localScale.y, localScale.z);
            blockTransform.localScale = localScale;
        }
    }
    
    private void OnMouseUp()
    {
        PointerManager.Instance.DeselectBlock();
        if (!GridManager.Instance.PlaceBlock(_parentBlock))
        {
            _parentBlock.ReturnToOriginal();
        }
        _isDragging = false;
    }
}
