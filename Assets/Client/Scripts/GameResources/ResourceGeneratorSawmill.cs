using DungeonShop.Player;

namespace DungeonShop.GameResources
{
    public class ResourceGeneratorSawmill : ResourceGenerator
    {
        public ResourceGeneratorSawmill()
        {
            levelPropertyName = "Level_Sawmill";
            lastCheckTimePropertyName = "Last_Check_Time_Sawmill";
            debugHoursPropertName = "Debug_Hours_Sawmill";
        }

        protected override int GetPlayerFreeSpace()
        {
            PlayerController player = FindObjectOfType<PlayerController>();

            if (player != null)
            {
                return player.GetWoodFreeSpace();
            }

            return 0;
        }
    }
}
