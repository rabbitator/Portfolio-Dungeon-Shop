using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Enemies
{
    public class EnemyTriggerEvents : MonoBehaviour
    {
        public EnemyEvent EnemyEnteredTrigger;
        public EnemyEvent EnemyStayInTrigger;
        public EnemyEvent EnemyExitTrigger;

        [Serializable]
        public class EnemyEvent : UnityEvent<Enemy> { }

        private void OnTriggerEnter(Collider other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
                return;

            EnemyEnteredTrigger.Invoke(enemy);
        }

        private void OnTriggerStay(Collider other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
                return;

            EnemyStayInTrigger.Invoke(enemy);
        }

        private void OnTriggerExit(Collider other)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
                return;

            EnemyExitTrigger.Invoke(enemy);
        }
    }
}