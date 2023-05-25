using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RedButton.GamePlay
{
    public class RiverFlooding : MonoBehaviour
    {
        [Header("Pushers")]
        [SerializeField] private Rigidbody pusherRB;
        [SerializeField] private Transform pusherParent;
        [SerializeField] private Vector2 pusherMinMaxHeight;
        [SerializeField] private float raiseLowerTime;

        [Header("Scripting References")]
        [SerializeField] private GameArbiter gameArbiter;
        [SerializeField] private GimmickCore gimmickCore;

        [Header("Flood Charecteristics: \n")]
        private float Duration;
        [SerializeField]private Vector3 destination;
        [SerializeField] private Vector3 origin;
        
        [SerializeField] private bool LocalGim = false;
        private bool roundStarted = false;
        private bool IsGimmickOccuring => gimmickCore.IsGimmickOccuring;
        // Start is called before the first frame update
        void Start()
        {
            gameArbiter = FindObjectOfType<GameArbiter>();
            gimmickCore = gameArbiter.GetComponent<GimmickCore>();
            Duration = gimmickCore.Duration;
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

                Vector3 pusherPos = pusherParent.position;
                pusherPos.y = Mathf.Lerp(pusherMinMaxHeight.x,pusherMinMaxHeight.y, time);
                pusherRB.MovePosition(pusherPos);
                yield return null;
            }

            yield return new WaitForSeconds(Duration);

            for (float i = 0; i < raiseLowerTime; i += Time.deltaTime)
            {
                float time = Mathf.InverseLerp( 0, raiseLowerTime, i);
                Vector3 waterPos = transform.position;
                waterPos.y = Mathf.Lerp(destination.y, origin.y, time);
                transform.position = waterPos;

                Vector3 pusherPos = pusherParent.position;
                pusherPos.y = Mathf.Lerp(pusherMinMaxHeight.y, pusherMinMaxHeight.x, time);
                pusherRB.MovePosition(pusherPos);
                yield return null;
            }

            LocalGim = false;
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

                Vector3 pusherPos = pusherParent.position;
                pusherPos.y = Mathf.Lerp(pusherMinMaxHeight.y, pusherMinMaxHeight.x, time);
                pusherRB.MovePosition(pusherPos);
                yield return null;
            }

            LocalGim = false;
        }
    }
}
