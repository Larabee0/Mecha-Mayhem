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
        [SerializeField] private Transform laserTransform;
        [SerializeField] private int bounces = 5;
        [SerializeField] private Vector3[] points;
        [SerializeField] private Vector3[] hitNormals;
        [SerializeField] private Vector3[] meshVertices;

        [SerializeField] private float laserLength = 5f;
        private BezierPath bezierPath;


        protected override void Start()
        {
            base.Start();
            ResetLaser();
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying)
            {
                ResetLaser();
            }

        }

        private void ResetLaser()
        {
            if (projectileMesh != null)
            {
                laserTransform.SetParent(transform);
                laserTransform.localPosition = Vector3.zero;
                laserTransform.localRotation = Quaternion.identity;
                points = new Vector3[bounces * 2];
                meshVertices = new Vector3[3];
                int[] indicies = new int[] { 0, 1, 1, 2 };
                hitNormals = new Vector3[bounces];
                projectileMesh.SetVertices(meshVertices);
                projectileMesh.SetIndices(indicies, MeshTopology.Lines, 0);
            }
        }

        protected override void BasicRayCaster()
        {
            // safety measure.
            if (muzzleOriginPoint == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin for weapon {0} was not set, aborting weapon firing", gameObject);
                return;
            }

            Vector3 aimDirection = TargetForward;
            Ray initialRay = new(muzzleOriginPoint.position, aimDirection);
            float curLaserDst = 0f;
            for (int i = 0, v = 0; i < bounces; i++, v += 2)
            {
                if (Physics.SphereCast(initialRay, laserDiameter, out RaycastHit hitInfo, raycastRange, ~IgnoreCollisionsLayers))
                {
                    HandleHit(hitInfo);
                    float dst = Vector3.Distance(initialRay.origin, hitInfo.point);
                    points[v] = transform.InverseTransformPoint(initialRay.origin);
                    //points[v] = initialRay.origin;
                    aimDirection = Vector3.ProjectOnPlane(Vector3.Reflect(initialRay.direction, hitInfo.normal), Vector3.up);
                    initialRay = new(hitInfo.point, aimDirection);
                    points[v + 1] = transform.InverseTransformPoint(hitInfo.point);
                    //points[v + 1] = hitInfo.point;
                    hitNormals[i] = hitInfo.normal;
                    curLaserDst += dst;
                }
            }
            bezierPath = new(points, false, PathSpace.xz);
            Show(points[0], points[^1]);

        }

        protected override void Show(Vector3 start, Vector3 end)
        {
            base.Show(start, end);
            laserTransform.SetParent(null);
            SpawnLaserExplosion(transform.TransformPoint(points[1]), hitNormals[1]);
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < points.Length - 1; i++)
            {
                Gizmos.DrawLine(transform.TransformPoint(points[i]), transform.TransformPoint(points[i + 1]));
            }
        }

        protected override IEnumerator Hide()
        {
            VertexPath vertexPath = new(bezierPath, laserTransform, 0.1f);

            float halfLength = laserLength / 2;
            float startTime = showTime;
            bool exploded = false;
            while (showTime > 0)
            {
                float timePeroid = Mathf.InverseLerp(startTime, 0, showTime);
                float showTimeCentrePointDst = Mathf.Lerp(0, vertexPath.length, timePeroid);
                if(!exploded && timePeroid >= 0.5f)
                {
                    SpawnLaserExplosion(transform.TransformPoint(points[3]), hitNormals[2]);
                    exploded = true;
                }
                Vector3 start = vertexPath.GetPointAtDistance(showTimeCentrePointDst - halfLength, EndOfPathInstruction.Stop);
                Vector3 centre = vertexPath.GetPointAtDistance(showTimeCentrePointDst, EndOfPathInstruction.Stop);
                Vector3 end = vertexPath.GetPointAtDistance(showTimeCentrePointDst + halfLength, EndOfPathInstruction.Stop);
                meshVertices[0] = start; meshVertices[1] = centre; meshVertices[2] = end;

                projectileMesh.SetVertices(meshVertices);
                showTime -= Time.deltaTime;

                yield return null;
            }

            SpawnLaserExplosion(transform.TransformPoint(points[^1]), hitNormals[^1]);
            projectileMeshRenderer.enabled = false;
            laserTransform.SetParent(transform);
            laserTransform.localPosition = Vector3.zero;
            laserTransform.localRotation = Quaternion.identity;
        }

    }
}