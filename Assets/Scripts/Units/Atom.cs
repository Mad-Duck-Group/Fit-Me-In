using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Atom : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private Block _parentBlock;
    public Block ParentBlock {get => _parentBlock; set => _parentBlock = value;}
    public SpriteRenderer SpriteRenderer => _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
