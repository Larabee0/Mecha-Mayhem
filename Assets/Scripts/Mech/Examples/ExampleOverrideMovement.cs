using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// This scripts shows how to override an existing movement script,
    /// in this case we are overriding ExampleBasicMovement.
    /// 
    /// Here we change how the mech is moved from add force to velocity setting.
    /// 
    /// Because OnMove is implemented in ExampleBasicMovement, it does not need to be implemented here.
    /// </summary>
    public class ExampleOverrideMovement : ExampleBasicMovement
    {
        protected override void FixedUpdate()
        {
            if (rb.velocity.y < float.Epsilon)
            {
                if (moveInput > float.Epsilon) { rb.velocity = moveInput * (moveSpeed * 0.375f) * transform.forward; }
            }
        }
    }
}