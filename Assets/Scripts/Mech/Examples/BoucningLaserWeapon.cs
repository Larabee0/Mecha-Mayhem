using NUnit;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.Mathematics;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class BoucningLaserWeapon : ExampleBasicRaycasterWeapon
    {
        [SerializeField] private int bounces = 5;
        [SerializeField] private Vector3[] points;
        [SerializeField] private Vector3[] meshVertices;

        [SerializeField] private float laserLength = 5f;
        private BezierPath bezierPath;

        private float curLaserDst = 0f;
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
            points = new Vector3[bounces * 2];
            meshVertices = new Vector3[3];
            int[] indicies = new int[] { 0, 1, 1, 2 };

            projectileMesh.SetVertices(meshVertices);
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
            curLaserDst = 0f;
            for (int i = 0, v = 0; i < bounces; i++, v+=2)
            {
                if (Physics.SphereCast(initialRay, laserDiameter, out RaycastHit hitInfo, raycastRange, ~IgnoreCollisionsLayers))
                {
                    HandleHit(hitInfo);
                    float dst = Vector3.Distance(initialRay.origin, hitInfo.point);
                    points[v] = transform.InverseTransformPoint( initialRay.origin);
                    aimDirection = Vector3.ProjectOnPlane(Vector3.Reflect(initialRay.direction, hitInfo.normal), Vector3.up);
                    initialRay = new(hitInfo.point, aimDirection);
                    points[v+1] = transform.InverseTransformPoint(hitInfo.point);
                    curLaserDst += dst;
                }
            }
            bezierPath = new(points, false, PathSpace.xz);
            Show(points[0], points[^1]);

        }

        protected override  void CorrectVertexTransform()
        {
            Vector3[] transformCorrectedPoints = new Vector3[meshVertices.Length];
            for (int i = 0; i < meshVertices.Length; i++)
            {
                transformCorrectedPoints[i] = transform.InverseTransformPoint(meshVertices[i]);
            }
            projectileMesh.SetVertices(meshVertices);
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < points.Length-1; i++)
            {
                Gizmos.DrawLine(transform.TransformPoint(points[i]),transform.TransformPoint( points[i + 1]));
            }
        }

        protected override IEnumerator Hide()
        {
            VertexPath vertexPath = new(bezierPath, transform, 0.1f);
            
            float halfLength = laserLength / 2;
            float startTime = showTime;
            while(showTime > 0)
            {

                float showTimeCentrePointDst = Mathf.Lerp(0, vertexPath.length, Mathf.InverseLerp(startTime, 0, showTime));
                
                Vector3 start = vertexPath.GetPointAtDistance(showTimeCentrePointDst - halfLength, EndOfPathInstruction.Stop);
                Vector3 centre = vertexPath.GetPointAtDistance(showTimeCentrePointDst, EndOfPathInstruction.Stop);
                Vector3 end = vertexPath.GetPointAtDistance(showTimeCentrePointDst+halfLength, EndOfPathInstruction.Stop);
                meshVertices[0]= start; meshVertices[1]= centre; meshVertices[2]= end;

                CorrectVertexTransform();
                showTime -= Time.deltaTime;

                yield return null;
            }
            projectileMeshRenderer.enabled = false;
        }


    }
}