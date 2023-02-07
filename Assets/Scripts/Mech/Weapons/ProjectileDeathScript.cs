using RedButton.Mech.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileDeathScript : ExampleBasicProjectile
{
    [SerializeField]
    private GameObject projectileExplosion;

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        Instantiate(projectileExplosion, transform.position, Quaternion.identity);
    }
}
