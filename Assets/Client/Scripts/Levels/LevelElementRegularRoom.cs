using DungeonShop.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Levels
{
    public class LevelElementRegularRoom : LevelElementRoom
    {
#pragma warning disable 0649
        [SerializeField]
        private Door entrance;
        [SerializeField]
        private Door exit;
#pragma warning restore 0649

        public Door Exit { get => exit; }
        public Door Entrance { get => entrance; }
    }
}