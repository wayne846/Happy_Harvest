namespace HappyHarvest
{
    public class FishingSpot : InteractiveObject
    {
        public override void InteractedWith()
        {
            UIHandler.OpenFishingSpot();
        }
    }
}
