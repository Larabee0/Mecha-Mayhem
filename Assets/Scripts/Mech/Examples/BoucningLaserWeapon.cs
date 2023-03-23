using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class BoucningLaserWeapon : ExampleBasicRaycasterWeapon
    {
        [SerializeField] private int bounces = 5;
        [SerializeField] private Vector3[] vertices;
        protected override void Start()
        {
            base.Start();
            ResetLaser();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if(Application.isPlaying )
            {
                ResetLaser();
            }
            
        }

        private void ResetLaser()
        {
            vertices = new Vector3[bounces * 2];
            int[] indicies = new int[bounces * 2];

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Vector3.forward * i;
                indicies[i] = i;
            }

            projectileMesh.SetVertices(vertices);
            projectileMesh.SetIndices(indicies, MeshTopology.Lines, 0);
        }

        protected override void BasicRayCaster()
        {
            // safety measure.
            if (muzzleOriginPoint == null || targetObject == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin or TargetObject for weapon {0} was not set, aborting weapon firing", gameObject.name);
                return;
            }

            Vector3 aimDirection = TargetPos - muzzleOriginPoint.position;
            Ray initialRay = new(muzzleOriginPoint.position, aimDirection);
            for (int i = 0, v = 0; i < bounces; i++, v+=2)
            {
                if (Physics.SphereCast(initialRay, laserDiameter, out RaycastHit hitInfo))
                {
                    HandleHit(hitInfo);
                    vertices[v] = initialRay.origin;
                    aimDirection = Vector3.ProjectOnPlane(Vector3.Reflect(initialRay.direction, hitInfo.normal), Vector3.up);
                    initialRay = new(hitInfo.point, aimDirection);
                    vertices[v+1] = hitInfo.point;
                }
            }

            Show(vertices[0], vertices[^1]);

        }

        protected override  void CorrectVertexTransform()
        {
            Vector3[] transformCorrectedPoints = new Vector3[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                transformCorrectedPoints[i] = transform.InverseTransformPoint(vertices[i]);
            }
            projectileMesh.SetVertices(transformCorrectedPoints);
        }

        protected override IEnumerator Hide()
        {
            while(showTime > 0)
            {
                showTime -= Time.deltaTime;
                CorrectVertexTransform();
                yield return null;
            }
            projectileMeshRenderer.enabled = false;
        }

    }
}