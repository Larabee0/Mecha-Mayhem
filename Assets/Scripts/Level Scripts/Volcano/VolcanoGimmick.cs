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
        private float Interval;
        private float Duration;
        private int amount;
        private float spawnrate;

        [Header("Flight characteristics")]
        [SerializeField] public float Velocity = 5f;
        [SerializeField] private float height = 20f;

        [Header("Objects")]
        [SerializeField] private GameObject Ejecta;
        [SerializeField] private GameObject TargetPlane;
        [SerializeField] private GameObject HazardZone;

        [Header("Debugging")]
        public float TimeFlag = 0f;
        public Vector3 TargetPoint;
        private bool IsGimmickOccuring;
        private bool LocalGim = false;

        void Start()
        {
            TimeFlag = Time.time;
        }

        void Update()
        {
            GameObject GameArb = GameObject.Find("GameArbiter");
            IsGimmickOccuring = GameArb.GetComponent<GimmickCore>().IsGimmickOccuring;
            if (IsGimmickOccuring == true)
            {
                if (LocalGim == false)
                { 
                    Interval = GameArb.GetComponent<GimmickCore>().Interval;
                    Duration = GameArb.GetComponent<GimmickCore>().Duration;
                    amount = GameArb.GetComponent<GimmickCore>().VolcanoAmount - 1;
                    spawnrate = Duration / amount;
                    //Debug.Log("volcano: " + Duration + "," + spawnrate + "," + amount);
                    TimeFlag = Time.time;
                    LocalGim = true;
                    StartCoroutine(Erupt());
                }
            }
        }
        IEnumerator Erupt()
        {
            while (Time.time < TimeFlag + Duration) {
                yield return new WaitForSeconds(spawnrate);
                NewTarget();
                SpawnEjecta();
            }
            yield return LocalGim = false;
            StopCoroutine(Erupt());          
        }

        public void SpawnEjecta()
        {
            // safety measure.
            if  (TargetPlane == null)
            {
                Debug.LogWarningFormat("Target Plane for VolcanoGimmick was not set ", gameObject.name);
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
