using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// Basic example of implementing movement core.
    /// </summary>
    public class ExampleBasicMovement : MovementCore
    {
        [Header("Movement settings")]
        [SerializeField] protected Rigidbody rb;
        [SerializeField] protected float moveSpeed = 20;
        [SerializeField] protected float rotationSpeed = 5;
        protected float moveInput;
        protected Vector3 moveDir;

        /// <summary>
        /// On move is the only required member to implement all others are optional.
        /// </summary>
        /// <param name="axis"></param>
        protected override void OnMove(Vector2 axis)
        {
            moveInput = axis.magnitude;
            moveDir = moveInput > 0 ? new Vector3(axis.x, 0f, axis.y) : transform.forward;
            targetPointParentForward = targetPointParent.forward;
            Vector3 pos = targetPoint.transform.position;
            pos.y = targetPointParent.transform.position.y;
            targetPoint.position = pos;
            transform.forward = Vector3.RotateTowards(transform.forward, moveDir, rotationSpeed * Time.deltaTime, 0.0f);
            targetPointParent.forward = targetPointParentForward;
        }

        /// <summary>
        /// This basic movement script moves the mech with add force.
        /// </summary>
        protected override void FixedUpdate()
        {
            if (rb.velocity.y < float.Epsilon)
            {
                rb.AddForce(moveInput * moveSpeed * moveDir, ForceMode.Force);
            }
        }
    }
}