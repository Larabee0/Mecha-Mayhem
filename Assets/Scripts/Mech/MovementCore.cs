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
        [SerializeField] private float aimSpeed = 5;
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
        protected virtual void OnDestroy()
        {
            CMC.MechInputController.OnLeftStick -= OnMove;
            CMC.MechInputController.OnRightStick -= OnAim;
        }

        protected abstract void OnMove(Vector2 axis);

        /// <summary>
        /// OnAim is implemented in the base class. But can be overriden in inheriting classes.
        /// </summary>
        /// <param name="axis"></param>
        protected virtual void OnAim(Vector2 axis, bool position)
        {
            Vector3 dir;
            if (position)
            {
                float height = targetPoint.position.y;
                Ray ray = Camera.main.ScreenPointToRay(axis);
                Vector3 aimPoint = ExtraMaths.GetPointAtHeight(ray, height);
                aimPoint.y = targetPointParent.position.y;

                dir = (aimPoint - targetPointParent.position).normalized;
                targetPoint.position = aimPoint;
            }
            else
            {
                float aimInput = axis.sqrMagnitude;
                targetDirection = aimInput > 0 ? new Vector3(axis.x, 0f, axis.y) : targetPointParent.forward;
                dir = Vector3.RotateTowards(targetPointParent.forward, targetDirection, aimInput * aimSpeed * CMC.MechInputController.ControllerSense * Time.deltaTime, 0.0f);
            }
            targetPointParent.forward = targetPointParentForward = dir;
        }
    }
}