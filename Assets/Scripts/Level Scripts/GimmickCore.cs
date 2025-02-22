using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class GimmickCore : MonoBehaviour
    {
        [Header("Timings")]
        public float MaxTimeInterval;
        public float MinTimeInterval;
        public float SpacebarCooldown;
        public float MaxDuration;
        public float MinDuration;

        [Header("Scripting References")]
        [SerializeField] private GameArbiter gameArbiter;

        [Space]
        [Header("Level Specific Parameters")]
        public int VolcanoAmount = 10;
        public int VolcanoDamage = 150;
        public int TestGroundLikelyhood;
        public int TestGroundAmount;

        [SerializeField] private AudioSource gimmickSound;

        [HideInInspector] public float Interval;
        [HideInInspector] public float Duration;
        [HideInInspector] public float TimeFlag;
        [HideInInspector] public bool IsGimmickOccuring = false;

        private bool SpacebarOn = false;
        private bool SpacebarReady = true;
        private bool roundStarted = false;

        // Start is called before the first frame update
        void Start()
        {
            TimeFlag = Time.time;
            Interval = Random.Range(MinTimeInterval, MaxTimeInterval);
            Duration = Random.Range(MinDuration, MaxDuration);
            gameArbiter = GetComponent<GameArbiter>();

            gameArbiter.OnRoundStarted += OnRoundStart;
            gameArbiter.OnRoundEnded += OnRoundEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if(!roundStarted)
            {
                return;
            }
            if (SpacebarReady && Input.GetKeyDown(KeyCode.Space))
            {
                SpacebarOn = true;
                if (gimmickSound != null)
                {
                    gimmickSound.Play();
                }
                SpacebarOn = true;
                StartCoroutine(SpaceDuration());
            }

            if (Time.time > TimeFlag + Interval && Time.time < TimeFlag + Interval + Duration || SpacebarOn)
            {
                IsGimmickOccuring = true;
            }
            else if (Time.time > TimeFlag + Interval + Duration)
            {
                IsGimmickOccuring = false;
                
                Interval = Random.Range(MinTimeInterval, MaxTimeInterval);
                Duration = Random.Range(MinDuration, MaxDuration);
                TimeFlag = Time.time;
                if (SpacebarOn)
                {
                    StartCoroutine(SpacebarCD());
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
        }

        IEnumerator SpaceDuration()
        {
            yield return new WaitForSeconds(Duration);
            SpacebarOn = false;
        }
        IEnumerator SpacebarCD()
        {
            yield return new WaitForSeconds(SpacebarCooldown);
            SpacebarReady = true;
        }
    }
}
