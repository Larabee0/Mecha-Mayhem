using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MechResults
{
    public int roundsWon;
    public int powerUpsConsumed;
    public int hitsMade;
    public int damageToOtherMechs;
    public int hitsTaken;
    public int damageRecieved;

    public int Score => roundsWon * (powerUpsConsumed + hitsMade + damageToOtherMechs - hitsTaken - damageRecieved);
}
