using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;
    
    public void SetEntry(int rank, string playerName, int score)
    {
        rankText.text = rank.ToString();
        nameText.text = playerName;
        scoreText.text = score.ToString("N0");
    }
    
    public void SetTextColor(Color color)
    {
        rankText.color = color;
        nameText.color = color;
        scoreText.color = color;
    }
}
