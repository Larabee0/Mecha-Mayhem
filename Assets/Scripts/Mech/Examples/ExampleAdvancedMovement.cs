using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// This is another version of implementing MovementCore it is exactly
    /// the same as Example Basic Movement other than Fixed Update.
    /// 
    /// I just want to show that we can have wholely different scripts implement the same abstract class.
    /// </summary>
    public class ExampleAdvancedMovement : MovementCore
    {
        [Header("Movement settings")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected float moveSpeed = 20;
        [SerializeField] protected float forceMultiplier = 1;
        [SerializeField] protected float rotationSpeed = 5 ;
        protected float moveInput;
        protected Vector3 moveDir;

        protected override void OnMove(Vector2 axis)
        {
            moveInput = axis.magnitude;
            moveDir = moveInput > 0 ? new Vector3(axis.x, 0f, axis.y).normalized : transform.forward;
            targetPointParentForward = targetPointParent.forward;
            Vector3 pos = targetPoint.transform.position;
            pos.y = targetPointParent.transform.position.y;
            targetPoint.position = pos;
            transform.forward = Vector3.RotateTowards(transform.forward, moveDir, rotationSpeed * Time.deltaTime, 0.0f);
            targetPointParent.forward = targetPointParentForward;
        }

        /// <summary>
        /// Here we add a force to slow the mech down in its previous direction of travel
        /// aka its current direction of momentum, before we push it in the desired direction.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (rb.velocity.y < float.Epsilon)
            {
                Vector3 currentDir = rb.velocity.normalized;
                Vector3 delta = currentDir - moveDir;
                rb.AddForce(moveInput * ((-moveSpeed * forceMultiplier) * 2) * delta, ForceMode.Force);
                rb.AddForce(moveInput * moveSpeed * forceMultiplier * moveDir, ForceMode.Force);
            }
        }
    }
}