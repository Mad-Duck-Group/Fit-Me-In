using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] private Atom[] atoms;
    
    private Vector3 _originalPosition;
    private Vector3 _originalRotation;
    private Vector3 _originalScale;

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
}
