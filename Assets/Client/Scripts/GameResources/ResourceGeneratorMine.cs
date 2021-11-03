using DungeonShop.Player;

namespace DungeonShop.GameResources
{
    public class ResourceGeneratorMine : ResourceGenerator
    {
        public ResourceGeneratorMine()
        {
            levelPropertyName = "Level_Mine";
            lastCheckTimePropertyName = "Last_Check_Time_Mine";
            debugHoursPropertName = "Debug_Hours_Mine";
        }

        protected override int GetPlayerFreeSpace()
        {
            PlayerController player = FindObjectOfType<PlayerController>();

            if (player != null)
            {
                return player.GetCrystalFreeSpace();
            }

            return 0;
        }
    }
}
