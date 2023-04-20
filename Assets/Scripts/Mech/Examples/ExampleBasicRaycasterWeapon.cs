using System.Collections;
using UnityEngine;

namespace RedButton.Mech.Examples
{
    /// <summary>
    /// A raycasting weapon example, this does not spawn a projectile prefab.
    /// 
    /// Instead the weapon object has a mesh renderer & mesh filter which it uses to draw a line
    /// between the muzzle origin and whatever the raycast hit, or a maximim range infront of the muzzle origin if it hit nothing
    /// 
    /// This line glows making it look like a laser.
    /// Damage application is instant.
    /// </summary>
    public class ExampleBasicRaycasterWeapon : WeaponCore
    {
        protected Mesh projectileMesh;

        private float fireInterval = 0f;
        protected float showTime = 0f;

        private Vector3 laserStart;
        private Vector3 laserEnd;

        [Header("Raycast Weapon settings")]
        [SerializeField] protected LayerMask IgnoreCollisionsLayers;
        [SerializeField] private float laserBrightness=5;
        [SerializeField] private bool takeMechAccentColour = false;
        [SerializeField, Range(0f, 1f)] float fireIntervalMin = 0.01f;
        [SerializeField, Range(0f, 1f)] float fireIntervalMax = 0.05f;
        [SerializeField, Range(5f, 200f)] protected float raycastRange = 10f;
        [SerializeField, Range(0.001f, 1f)] protected float laserDiameter = 0.005f;
        [Tooltip("Multiplies the showTime by this value to get decayStartTime.\n The effect starts after the showTime becomes less than or equal to the decayStartTime.\nLowering this causes the laser to stay at full length for longer, but also makes the apparent speed when it decays faster.")]
        [SerializeField, Range(0f, 1f)] float laserEffectDecayDelayFraction = 1;
        [SerializeField] private MeshFilter projectileMeshFilter;
        [SerializeField] protected MeshRenderer projectileMeshRenderer;
        protected Coroutine hideCoroutine;
        /// <summary>
        /// overriding start to create the mesh to modify at runtime.
        /// </summary>
        protected override void Start()
        { 
            // start requires base.Start() to be called so the target object is still set.
            // Alternatively you could copy all the code from WeaponCore's start method and not have to do this.
            base.Start();
            projectileMeshFilter.mesh = projectileMesh = new Mesh() { subMeshCount = 1 };

            projectileMesh.SetVertices(new Vector3[] {Vector3.zero, Vector3.forward });
            projectileMesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
            projectileMeshRenderer.enabled = false;
            if (takeMechAccentColour)
            {
                projectileMeshRenderer.material.SetColor("_BaseColor", CMC.MechAccentColour);
                projectileMeshRenderer.material.SetColor("_EmissiveColor", CMC.MechAccentColour * laserBrightness);
            }
        }

        protected override void OnEnable()
        {
            fireInterval = 0;
            base.OnEnable();
            projectileMeshRenderer.enabled = false;
        }
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
            {
                if(CMC != null)
                {
                    projectileMeshRenderer.material.SetColor("_BaseColor", CMC.MechAccentColour);
                    projectileMeshRenderer.material.SetColor("_EmissiveColor", CMC.MechAccentColour * laserBrightness);
                }
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            hideCoroutine = null;
        }
        /// <summary>
        /// Fire is one of the methods that requires implmentation.
        /// Here whenever Fire() is called, it simply counts down a timer called FireInterval.
        /// When this values is less than zero it fires the weapon by calling BasicRayCaster().
        /// It also then resets the fireInteral to a random time between a min and max value which you can set in the editor.
        /// </summary>
        public override void Fire()
        {
            if (!CMC.ShieldActive)
            {
                switch (fireInterval)
                {
                    case <= 0f:
                        StartCoroutine(LaserCoolDown());
                        showTime = fireInterval / 2f; // showTime is half the time of the new fireInterval to ensure the laser visual is hidden before it fires again.
                        BasicRayCaster();
                        break;
                }
            }
        }

        /// <summary>
        /// GroupFire is a pass through to call BasicRayCast() directly as timing is handled by the WeaponGroup.
        /// </summary>
        public override void GroupFire()
        {
            if (!CMC.ShieldActive)
            {
                // we still set showTime so the laser is shown for a reasonable time. 
                // this is less important than in Fire() as the same weapon will not fire twice in a row.
                showTime = Random.Range(fireIntervalMin, fireIntervalMax) / 2f;
                BasicRayCaster();
            }
        }

        protected IEnumerator LaserCoolDown()
        {
            fireInterval = Random.Range(fireIntervalMin, fireIntervalMax);
            yield return new WaitForSeconds(fireInterval);
            fireInterval = 0f;
        }

        /// <summary>
        /// Here is where the weapon is fired.
        /// This uses a sphere cast to fire a ray from the muzzleOriginPoint in the direction of a TargetPoint.
        /// If the cast hits a mech it will find the Mech's CMC and update its health.
        /// </summary>
        protected virtual void BasicRayCaster()
        {
            // safety measure.
            if(muzzleOriginPoint == null || targetObject == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin or TargetObject for weapon {0} was not set, aborting weapon firing", gameObject.name);
                return;
            }

            Vector3 aimDirection = TargetPos - muzzleOriginPoint.position;
            Ray ray = new(muzzleOriginPoint.position,aimDirection);

            Vector3 endPoint; // end point must be calulated weather the spherecast hits something or not.
            switch (Physics.SphereCast(ray, laserDiameter, out RaycastHit hit, raycastRange,~IgnoreCollisionsLayers))
            {
                case true:
                    endPoint = hit.point; // if the sphere cast hits something we can set the end point to the hit point.
                    HandleHit(hit);
                    break;
                case false:
                    // if nothing was hit we can calculate the end point as a point infront of the 
                    endPoint = ray.GetPoint(raycastRange);
                    break;
            }

            // display the laser visual.
            Show(ray.origin, endPoint);
        }

        protected void HandleHit(RaycastHit hit)
        {
            ShieldTagger hitShield = hit.collider.gameObject.GetComponent<ShieldTagger>();
            if (hitShield != null && hitShield.shield.ShieldOwner)
            {
                hitShield.shield.DamageShield();
                return;
            }
            if (hitShield != null && hitShield != CMC.shield)
            {
            }
            CentralMechComponent otherMech = hit.collider.gameObject.GetComponentInParent<CentralMechComponent>();
            if (otherMech != null && otherMech != CMC) // saftey in case we hit ourselves.
            {
                otherMech.UpdateHealth(damage);
            }
        }

        /// <summary>
        /// given a start and end point (world coordinates)
        /// we can draw a line between these two points in a mesh.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        protected void Show(Vector3 start, Vector3 end)
        {
            laserStart = start; laserEnd = end; // needed to keep the mesh updated in the coroutine.
            CorrectVertexTransform();
            projectileMeshRenderer.enabled = true;
            if(hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
                hideCoroutine = null;
            }
            if(!enabled || !gameObject.activeInHierarchy)
            {
                return;
            }
            hideCoroutine = StartCoroutine(Hide()); // start the hide corountine to hide the visual after showTime has elapsed.
        }
        protected virtual void CorrectVertexTransform()
        {
            projectileMesh.SetVertices(new Vector3[] { transform.InverseTransformPoint(laserStart), transform.InverseTransformPoint(laserEnd) });
        }

        /// <summary>
        /// Hides the laser after showTime duration, also keeps the laser visuals up to date for this duration
        /// This prevents the laser getting swung around by the weapon as it turns or the mech moves.
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator Hide()
        {
            float decayStartTime = showTime * laserEffectDecayDelayFraction;
            while(showTime > 0)
            {
                showTime -= Time.deltaTime;
                Vector3 currentStart = Vector3.Lerp(laserStart, laserEnd, Mathf.InverseLerp(decayStartTime, 0, showTime));
                projectileMesh.SetVertices(new Vector3[] { transform.InverseTransformPoint(currentStart), transform.InverseTransformPoint(laserEnd) });
                yield return null;
            }
            projectileMeshRenderer.enabled = false;
            hideCoroutine = null;
        }
    }
}