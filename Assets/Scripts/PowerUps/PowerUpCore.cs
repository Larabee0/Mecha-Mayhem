using RedButton.Mech;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace RedButton.GamePlay
{
    public class PowerUpCore : MonoBehaviour
    {
        protected CentralMechComponent CMC;
        public int limit=3;
        public Color powerUpColour;
        public virtual void Copy(PowerUpCore source)
        {
            
        }
        public virtual void AddTo(CentralMechComponent target)
        {

        }
    }
}
