using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MechResults : IComparable<MechResults>
{
    public string player;
    public int roundsWon;
    public int powerUpsConsumed;
    public int hitsMade;
    public int damageToOtherMechs;
    public int hitsTaken;
    public int damageRecieved;

    public int Score => (powerUpsConsumed + hitsMade + damageToOtherMechs - hitsTaken - damageRecieved) * 1 + roundsWon;

    public string this[int i] => i switch
    {
        0 => player,
        1 => roundsWon.ToString(),
        2 => hitsMade.ToString("D3"),
        3 => damageToOtherMechs.ToString("D3"),
        4 => powerUpsConsumed.ToString("D3"),
        5 => hitsTaken.ToString("D3"),
        6 => damageRecieved.ToString("D3"),
        7 => Score.ToString("D5"),
        _ => int.MaxValue.ToString()
    };

    public int CompareTo(MechResults other)
    {
        return roundsWon.CompareTo(other.roundsWon);
    }
}
