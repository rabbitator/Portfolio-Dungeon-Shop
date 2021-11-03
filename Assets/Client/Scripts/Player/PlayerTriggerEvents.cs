using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Player
{
    public class PlayerTriggerEvents : MonoBehaviour
    {
        public UnityEvent PlayerEnteredTrigger;
        public UnityEvent PlayerStayInTrigger;
        public UnityEvent PlayerExitTrigger;

        private void OnTriggerEnter(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
                return;

            PlayerEnteredTrigger.Invoke();
        }

        private void OnTriggerStay(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
                return;

            PlayerStayInTrigger.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
                return;

            PlayerExitTrigger.Invoke();
        }
    }
}