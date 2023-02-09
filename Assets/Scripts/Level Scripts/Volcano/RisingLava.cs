using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RedButton.GamePlay
{
    public class RisingLava : MonoBehaviour
    {
        [Header("Lava Charecteristics: \n")]
        [SerializeField] private int EruptionInterval = 60;
        [SerializeField] private int Duration = 10;
        [SerializeField] private float height = 20f;
        [SerializeField] public int damage = 50;

        private float timer = 0f;
        private Vector3 destination;
        private Vector3 origin;
        // Start is called before the first frame update
        void Start()
        {
            timer = Time.time;
            Vector3 origin = new Vector3 (gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
            Vector3 destination = new Vector3 (gameObject.transform.position.x, height, gameObject.transform.position.z);
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= timer + EruptionInterval)
            {
                timer = Time.time;
                Erupt();
            }
        }

        public void Erupt()
        {
            RaiseLava();
            if (Time.time > timer + Duration)
            {
                LowerLava();
                timer = Time.time;
            }
        }

        public void RaiseLava()
        {
            gameObject.transform.Translate(destination);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.name == "mech")
            {
                Mech.CentralMechComponent mech = collision.gameObject.GetComponentInParent<Mech.CentralMechComponent>();
                mech.UpdateHealth(damage);
            }
        }

        public void LowerLava()
        {
            gameObject.transform.Translate(origin);
        }
    }
}
