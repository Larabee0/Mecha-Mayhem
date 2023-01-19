using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace RedButton.Mech
{
    /// <summary>
    /// Base class all mech movement variants are derived from
    /// </summary>
    public abstract class MovementCore : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] protected CentralMechComponent CMC;
        [SerializeField, Range(1f, 20f)] private float aimSpeed = 5;
        [SerializeField] protected Transform targetPointParent;
        [SerializeField] protected Transform targetPoint;
        [SerializeField] private float targetPointDistance = 5f;

        protected Vector3 targetDirection;
        protected Vector3 targetPointParentForward;
        public virtual Transform TargetPoint => targetPoint;

        protected virtual void Awake()
        {
            CMC = GetComponentInParent<CentralMechComponent>();

            if (CMC == null)
            {
                Debug.LogErrorFormat("MovementCore attached to {0}, was unable to find the CMC! of its mech", gameObject.name);
                enabled = false;
                return;
            }
            CMC.MechInputController.OnLeftStick += OnMove;
            CMC.MechInputController.OnRightStick += OnAim;
        }

        protected virtual void Start()
        {
            targetPoint.localPosition += targetPoint.forward * targetPointDistance;
        }

        protected virtual void Update() { }

        protected virtual void FixedUpdate() { }

        protected abstract void OnMove(Vector2 axis);

        /// <summary>
        /// OnAim is implemented in the base class. But can be overriden in inheriting classes.
        /// </summary>
        /// <param name="axis"></param>
        protected virtual void OnAim(Vector2 axis)
        {
            float aimInput = axis.magnitude;
            targetDirection = aimInput > 0 ? new Vector3(axis.x, 0f, axis.y) : targetPointParent.forward;
            targetPointParent.forward = targetPointParentForward =
                Vector3.RotateTowards(targetPointParent.forward, targetDirection, aimInput * aimSpeed * Time.deltaTime, 0.0f);
        }
    }
}