using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    /// <summary>
    /// 
    /// </summary>
    public class VolcanoGimmick : MonoBehaviour
    {
        [Header("Ejecta Characteristics")]
        [SerializeField] private int EruptionInterval = 60;
        [SerializeField] private int Duration = 10;
        [SerializeField] private int spawnrate = 1;
        [SerializeField] public int damage = 50;

        [Header("Flight characteristics")]
        [SerializeField] public float Velocity = 5f;
        [SerializeField] private float height = 20f;

        [Header("Objects")]
        [SerializeField] private GameObject Ejecta;
        [SerializeField] private GameObject TargetPlane;
        [SerializeField] private GameObject HazardZone;

        private float time = 0f;
        private GameObject SelfCollider;
        public Vector3 TargetPoint;

        private void Awake()
        {
            SelfCollider = GetComponentInParent<GameObject>();
        }
        void Start()
        {
            spawnrate = spawnrate * 100;
            time = Time.time;
        }


        private void Update()
        {
            if (Time.time >= time + EruptionInterval)
            {
                time = Time.time;
                Erupt();
            }


        }
        public void Erupt()
        {
            while (Time.time < time + Duration)
            {
                if (Random.Range(0f,1f) < spawnrate)
                {
                    NewTarget();
                    SpawnEjecta();
                }
            }
            time = Time.time;
        }

        public void SpawnEjecta()
        {
            // safety measure.
            if  (TargetPlane == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin or TargetObject for weapon {0} was not set, aborting weapon firing", gameObject.name);
                return;
            }

            Vector3 origin = new Vector3(TargetPoint.x, height, TargetPoint.z);
            Vector3 bulletVector = (TargetPoint - origin).normalized;
            Quaternion rotation = Quaternion.LookRotation(bulletVector);

            // spawn the projectile in the correct orientaiton and position
            GameObject ejecta = Instantiate(Ejecta, origin, rotation);  
        }

        public void NewTarget()
        {
            List<Vector3> VerticeList = new List<Vector3>(TargetPlane.GetComponent<MeshFilter>().sharedMesh.vertices);
            Vector3 leftTop = TargetPlane.transform.TransformPoint(VerticeList[0]);
            Vector3 rightTop = TargetPlane.transform.TransformPoint(VerticeList[10]);
            Vector3 leftBottom = TargetPlane.transform.TransformPoint(VerticeList[110]);
            Vector3 rightBottom = TargetPlane.transform.TransformPoint(VerticeList[120]);
            Vector3 XAxis = rightTop - leftTop;
            Vector3 ZAxis = leftBottom - leftTop;
            Vector3 TargetPoint = leftTop + XAxis * Random.value + ZAxis * Random.value;

            // spawn a warning sign in the random area
            GameObject Haz = GameObject.Instantiate(HazardZone);
            Haz.transform.position = TargetPoint + TargetPlane.transform.up * 0.5f;
        }
        
    }
}
