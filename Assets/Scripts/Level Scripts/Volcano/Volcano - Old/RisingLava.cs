using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class RisingLava : MonoBehaviour
    {
        private float Duration;
        private int Damage;

        public Vector3 LavaSpeed;
        public float timer = 0f;
        public Vector3 destination;
        public Vector3 origin;
        private bool IsGimmickOccuring => gimmickCore.IsGimmickOccuring;
        private bool LocalGim = false;
        private bool roundStarted = false;

        [Header("Scripting References")]
        [SerializeField] private GameArbiter gameArbiter;
        [SerializeField] private GimmickCore gimmickCore;

        private float raiseLowerTime = 0f;

        // Start is called before the first frame update
        void Start()
        {
            raiseLowerTime = Mathf.Abs(origin.y - destination.y) / LavaSpeed.y;

            timer = Time.time;
            gameArbiter = FindObjectOfType<GameArbiter>();
            gimmickCore = gameArbiter.GetComponent<GimmickCore>();
            Damage = gimmickCore.VolcanoDamage;
            gameArbiter.OnRoundStarted += OnRoundStart;
            gameArbiter.OnRoundEnded += OnRoundEnd;
        }

        // Update is called once per frame
        void Update()
        {
            if (!roundStarted)
            {
                return;
            }
            if (IsGimmickOccuring == true)
            {
                if (LocalGim == false)
                {
                    LocalGim = true;
                    Duration = gimmickCore.Duration;
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
            if (LocalGim)
            {
                StopAllCoroutines();
                StartCoroutine(EndRound());
            }
        }

        IEnumerator Erupt()
        {


            for (float i = 0; i < raiseLowerTime; i += Time.deltaTime)
            {
                float time = Mathf.InverseLerp(0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(origin.y, destination.y, time);
                transform.position = waterPos;
                yield return null;
            }


            // while (gameObject.transform.position.y <= destination.y)
            // {
            //     yield return null;
            //     RaiseLava();
            // }
            yield return new WaitForSeconds(Duration);


            for (float i = 0; i < raiseLowerTime; i += Time.deltaTime)
            {
                float time = Mathf.InverseLerp(0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(destination.y, origin.y, time);
                transform.position = waterPos;

                yield return null;
            }
            // while (gameObject.transform.position.y > origin.y)
            // {
            //     yield return null;
            //     LowerLava();
            // }
            LocalGim = false;
            StopCoroutine(Erupt());           
        }

        private IEnumerator EndRound()
        {
            float time = Mathf.InverseLerp(destination.y, origin.y, transform.position.y);

            float startTime = Mathf.Lerp(0, raiseLowerTime, time);

            for (float i = startTime; i < raiseLowerTime; i += Time.deltaTime)
            {
                time = Mathf.InverseLerp(0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(destination.y, origin.y, time);
                transform.position = waterPos;
                yield return null;
            }

            LocalGim = false;
        }

        public void RaiseLava()
        {
            gameObject.transform.Translate(LavaSpeed * Time.deltaTime);
        }

             
        public void LowerLava()
        {
            gameObject.transform.Translate(-LavaSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
                mech.UpdateHealth(Damage);
            }
        }
    }
}
