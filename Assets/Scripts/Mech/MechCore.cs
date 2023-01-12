using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech
{
    /// <summary>
    /// Core mech control script
    /// Acts as an interface for movement and weapons
    /// </summary>
    public class MechCore : MonoBehaviour
    {
        [SerializeField] private MechMovementBase movementControl;
        [SerializeField] private WeaponBase[] weapons;

        private void Start()
        {

        }

        private void Update()
        {

        }
    }
}