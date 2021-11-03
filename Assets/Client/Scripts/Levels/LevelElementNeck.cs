using DungeonShop.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Levels
{
    public class LevelElementNeck : LevelElement
    {
#pragma warning disable 0649
        [SerializeField]
        private PlayerTriggerEvents doorOpeningTrigger;
#pragma warning restore 0649

        public PlayerTriggerEvents DoorOpeningTrigger { get => doorOpeningTrigger; set => doorOpeningTrigger = value; }
    }
}
