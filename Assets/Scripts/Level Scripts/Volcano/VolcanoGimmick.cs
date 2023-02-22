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
        [SerializeField] private float spawnrate = 1;
        [SerializeField] public int damage = 50;

        [Header("Flight characteristics")]
        [SerializeField] public float Velocity = 5f;
        [SerializeField] private float height = 20f;

        [Header("Objects")]
        [SerializeField] private GameObject Ejecta;
        [SerializeField] private GameObject TargetPlane;
        [SerializeField] private GameObject HazardZone;

        public float time = 0f;
        public Vector3 TargetPoint;

        void Start()
        {
            spawnrate = spawnrate / 1000;
            time = Time.time;
        }


        private void Update()
        {
            if (Time.time >= time + EruptionInterval)
            {
                Erupt();
            }

        }
        public void Erupt()
        {
            if (Time.time < time + Duration + EruptionInterval)
            {
                if (Random.Range(0f, 1f) < spawnrate)
                {
                    NewTarget();
                    SpawnEjecta();
                }
            }
            else
            {
                time = Time.time;
            }
            
        }

        public void SpawnEjecta()
        {
            // safety measure.
            if  (TargetPlane == null)
            {
                Debug.LogWarningFormat("MuzzleOrigin or TargetObject for weapon {0} was not set, aborting weapon firing", gameObject.name);
                return;
            }

            Vector3 SpawnPoint = new Vector3(TargetPoint.x, height, TargetPoint.z);
            Vector3 bulletVector = (TargetPoint - SpawnPoint).normalized;
            Quaternion rotation = Quaternion.LookRotation(bulletVector);

            // spawn the projectile in the correct orientaiton and position
            GameObject ejecta = Instantiate(Ejecta, SpawnPoint, rotation);  
        }

        public void NewTarget()
        {
            List<Vector3> VerticeList = new List<Vector3>(TargetPlane.GetComponent<MeshFilter>().sharedMesh.vertices);
            Vector3 leftTop = TargetPlane.transform.TransformPoint(VerticeList[0]);
            Vector3 rightTop = TargetPlane.transform.TransformPoint(VerticeList[10]);
            Vector3 leftBottom = TargetPlane.transform.TransformPoint(VerticeList[110]);
            Vector3 XAxis = rightTop - leftTop;
            Vector3 ZAxis = leftBottom - leftTop;
            TargetPoint = leftTop + XAxis * Random.value + ZAxis * Random.value;

            /*TargetPoint.x = Random.Range(leftTop.x, rightTop.x);
            TargetPoint.y = 0f;
            TargetPoint.z = Random.Range(leftTop.z, leftBottom.z);*/
        }
        
    }
}
