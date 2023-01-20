using System.Collections;
using System.Net;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    public class ExampleBasicRaycasterWeapon : WeaponCore
    {
        [Header("Raycast Weapon settings")]
        [SerializeField] private MeshFilter projectileMeshFilter;
        [SerializeField] private MeshRenderer projectileMeshRenderer;

        private Mesh projectileMesh;

        [SerializeField][Range(0f, 1f)] float fireIntervalMin = 0.01f;
        [SerializeField][Range(0f, 1f)] float fireIntervalMax = 0.05f;
        [SerializeField][Range(5f, 200f)] float raycastRange = 10f;
        private float fireInterval = 0f;
        private float showTime = 0f;

        private Vector3 laserStart;
        private Vector3 laserEnd;

        protected override void Start()
        {
            base.Start();
            projectileMeshFilter.mesh = projectileMesh = new Mesh() { subMeshCount = 1 };

            projectileMesh.SetVertices(new Vector3[] {Vector3.zero, Vector3.forward });
            projectileMesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
            projectileMeshRenderer.enabled = false;
        }

        public override void Fire()
        {
            fireInterval -= Time.deltaTime;
            switch (fireInterval)
            {
                case <= 0f:
                    fireInterval = Random.Range(fireIntervalMin, fireIntervalMax);
                    showTime = fireInterval / 2f;
                    BasicRayCaster();
                    break;
            }
        }

        public override void GroupFire()
        {
            BasicRayCaster();
        }

        private void BasicRayCaster()
        {
            if(projectileSpawnPoint == null || targetObject == null) return;

            Vector3 aimDirection = TargetPos - projectileSpawnPoint.position;
            Ray ray = new(projectileSpawnPoint.position,aimDirection);

            Vector3 endPoint;
            switch (Physics.SphereCast(ray, 0.005f, out RaycastHit hit, raycastRange))
            {
                case true:
                    endPoint = hit.point;
                    CentralMechComponent otherMech = hit.collider.gameObject.GetComponentInParent<CentralMechComponent>();
                    if(otherMech != null && otherMech != CMC)
                    {
                        otherMech.UpdateHealth(damage);
                    }
                    
                    break;
                case false:
                    endPoint = TargetPos + (aimDirection * raycastRange);
                    break;
            }
            Show(ray.origin, endPoint);
        }

        private void Show(Vector3 start, Vector3 end)
        {
            laserStart = start; laserEnd = end;

            projectileMesh.SetVertices(new Vector3[] { transform.InverseTransformPoint(laserStart), transform.InverseTransformPoint(laserEnd) });
            projectileMeshRenderer.enabled = true;
            StartCoroutine(Hide());
        }

        private IEnumerator Hide()
        {
            while(showTime > 0)
            {
                showTime -= Time.deltaTime;
                projectileMesh.SetVertices(new Vector3[] { transform.InverseTransformPoint(laserStart), transform.InverseTransformPoint(laserEnd) });
                yield return null;
            }
            projectileMeshRenderer.enabled = false;
        }
    }
}