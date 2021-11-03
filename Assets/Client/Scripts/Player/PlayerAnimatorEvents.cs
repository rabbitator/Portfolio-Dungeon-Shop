using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonShop.Player;

namespace DungeonShop.Player
{
    public class PlayerAnimatorEvents : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private PlayerController.PlayerType playerType;
        [SerializeField]
        private PlayerController playerController;
#pragma warning restore 0649

        private void HitResource()
        {
            playerController.HitResource(playerType);
        }
    }
}