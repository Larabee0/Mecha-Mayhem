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
        private float Duration;
        private int amount;
        private float spawnrate;

        [Header("Flight characteristics")]
        public float Velocity = 5f;
        [SerializeField] private float height = 20f;

        [Header("Objects")]
        [SerializeField] private GameObject Ejecta;
        [SerializeField] private GameObject TargetPlane;
        [SerializeField] private GameObject HazardZone;

        [SerializeField] private GameArbiter gameArbiter;
        [SerializeField] private GimmickCore gimmickCore;

        [Header("Debugging")]
        public float TimeFlag = 0f;
        private bool IsGimmickOccuring => gimmickCore.IsGimmickOccuring;
        private bool LocalGim = false;
        private bool roundStarted = false;

        void Start()
        {
            TimeFlag = Time.time;
            gameArbiter = FindObjectOfType<GameArbiter>();
            gimmickCore = gameArbiter.GetComponent<GimmickCore>();
            gameArbiter.OnRoundStarted += OnRoundStart;
            gameArbiter.OnRoundEnded += OnRoundEnd;
        }

        void Update()
        {
            if(!roundStarted)
            {
                return;
            }
            if (IsGimmickOccuring == true)
            {
                if (LocalGim == false)
                {
                    Duration = gimmickCore.Duration;
                    amount = gimmickCore.VolcanoAmount - 1;
                    spawnrate = Duration / amount;

                    TimeFlag = Time.time;
                    LocalGim = true;
                    StartCoroutine(Erupt());
                }
            }
        }
        
        private void OnDestroy()
        {
            gameArbiter.OnRoundStarted -= OnRoundStart;
            gameArbiter.OnRoundEnded -= OnRoundEnd;
        }

        private void OnRoundStart()
        {
            roundStarted = true;
        }

        private void OnRoundEnd()
        {
            roundStarted = false;
            StopAllCoroutines();
        }

        IEnumerator Erupt()
        {
            while (Time.time < TimeFlag + Duration)
            {
                yield return new WaitForSeconds(spawnrate);
                SpawnEjecta(NewTarget());
            }
            LocalGim = false;
        }

        public void SpawnEjecta(Vector3 targetPoint)
        {
            // safety measure.
            if (TargetPlane == null)
            {
                Debug.LogWarningFormat("Target Plane for VolcanoGimmick was not set ", gameObject.name);
                return;
            }

            Vector3 SpawnPoint = new(targetPoint.x, height, targetPoint.z);
            Vector3 bulletVector = (targetPoint - SpawnPoint).normalized;
            Quaternion rotation = Quaternion.LookRotation(bulletVector);

            // spawn the projectile in the correct orientaiton and position
            Instantiate(Ejecta, SpawnPoint, rotation).GetComponent<EjectaScript>().TargetPoint = targetPoint;
        }

        public Vector3 NewTarget()
        {
            List<Vector3> VerticeList = new(TargetPlane.GetComponent<MeshFilter>().sharedMesh.vertices);
            Vector3 leftTop = TargetPlane.transform.TransformPoint(VerticeList[0]);
            Vector3 rightTop = TargetPlane.transform.TransformPoint(VerticeList[10]);
            Vector3 leftBottom = TargetPlane.transform.TransformPoint(VerticeList[110]);
            Vector3 XAxis = rightTop - leftTop;
            Vector3 ZAxis = leftBottom - leftTop;
            return leftTop + XAxis * Random.value + ZAxis * Random.value;
        }
    }
}
