using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RedButton.ProGen.VersionTwo
{
    public class TileCombCounter : MonoBehaviour
    {
        public Text textField;
        public int PossibleTiles { set => textField.text = value.ToString(); }
    }
}