using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerManager : MonoBehaviour
{
    private static PointerManager _instance;
    public static PointerManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Pointer Manager is null");
            }
            return _instance;
        }
    }
    private Block _selectedBlock;
    private Camera _camera;
    private Vector3 _mousePositionDifference;
    public Vector3 MousePosition
    {
        get
        {
            Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            return mousePosition;
        }
    }
    
    private void Awake()
    {
        _instance = this;
        _camera = Camera.main;
    }
    
    void Update()
    {
        HandleMovement();
    }
    
    /// <summary>
    /// Select a block
    /// </summary>
    /// <param name="block">Block to select</param>
    public void SelectBlock(Block block)
    {
        if (_selectedBlock && _selectedBlock == block) return;
        _selectedBlock = block;
        var position = _selectedBlock.transform.position;
        _mousePositionDifference = new Vector3(MousePosition.x - position.x,
            MousePosition.y - position.y, 0);
        _selectedBlock.SetRendererSortingOrder(2);
    }
    
    /// <summary>
    /// Deselect current block
    /// </summary>
    public void DeselectBlock()
    {
        if (!_selectedBlock) return;
        _mousePositionDifference = Vector3.zero;
        _selectedBlock = null;
    }
    
    /// <summary>
    /// Handle the movement of the selected block
    /// </summary>
    private void HandleMovement()
    {
        if (!_selectedBlock) return;
        _selectedBlock.transform.position = MousePosition - _mousePositionDifference;
    }
}
