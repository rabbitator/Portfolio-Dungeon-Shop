using DungeonShop.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Levels
{
    public class LevelElementStartRoom : LevelElementRoom
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform playerSpawnPoint;
#pragma warning restore 0649

        public Transform PlayerSpawnPoint { get => playerSpawnPoint; }
    }
}