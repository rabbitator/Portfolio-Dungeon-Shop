using DungeonShop.GameLogic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Levels
{
    public class LevelElementRoom : LevelElement
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform limitLeftBottom;
        [SerializeField]
        private Transform limitRightTop;
        [SerializeField]
        private DungeonRoom dungeonRoom;
        [SerializeField]
        private ParticleSystem fogOfWar;
#pragma warning restore 0649

        public DungeonRoom DungeonRoom { get => dungeonRoom; }
        public Transform LimitLeftBottom { get => limitLeftBottom; }
        public Transform LimitRightTop { get => limitRightTop; }

        public void ActivateFogOfWar()
        {
            if (fogOfWar == null)
                return;

            fogOfWar.Play();
        }

        public void DeactivateFogOfWar()
        {
            if (fogOfWar == null)
                return;

            fogOfWar.Stop();
        }
    }
}
