using UnityEngine;

namespace HappyHarvest
{
    [CreateAssetMenu(menuName = "2D Farming/Items/Fish")]
    public class Fish : Item
    {
        public int SellPrice = 1;

        public override bool CanUse(Vector3Int target)
        {
            return true;
        }

        public override bool Use(Vector3Int target)
        {
            return true;
        }

        public override bool NeedTarget()
        {
            return false;
        }
    }
}
