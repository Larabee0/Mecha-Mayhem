using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RedButton.Mech;
using RedButton.Mech.Examples;

namespace RedButton.Wiimote
{
    public class WiimoteMovementOverride : ExampleAdvancedMovement
    {
        [SerializeField] private WiimoteTesting wiimoteInput;

        protected override void Awake()
        {
            wiimoteInput = FindObjectOfType<WiimoteTesting>();
            CMC = GetComponentInParent<CentralMechComponent>();

            if (CMC == null)
            {
                Debug.LogErrorFormat("MovementCore attached to {0}, was unable to find the CMC! of its mech", gameObject.name);
                enabled = false;
                return;
            }
        }

        // protected override void Start()
        // {
        //     base.Start();
        //     if(wiimoteTesting == null)
        //     {
        //         enabled = false;
        //     }
        // }

        protected override void Update()
        {
            if(wiimoteInput != null)
            {
                OnMove(wiimoteInput.NunChuckStick);
                OnAim(wiimoteInput.PointPosition);
            }
            else
            {
                Vector2 keyboardAxis = new()
                {
                    x = Input.GetAxis("Horizontal"),
                    y = Input.GetAxis("Vertical")
                };
                OnMove(keyboardAxis);
                OnAim(Input.mousePosition);
            }
        }

        protected override void OnAim(Vector2 position)
        {
            float height = targetPoint.position.y;
            Ray ray = Camera.main.ScreenPointToRay(position);
            Vector3 aimPoint = ExtraMaths.GetPointAtHeight(ray, height);
            Vector3 dir = (aimPoint- targetPointParent.position).normalized;
            targetPointParent.forward = targetPointParentForward = dir;
        }
    }
}