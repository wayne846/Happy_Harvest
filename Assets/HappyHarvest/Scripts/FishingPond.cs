namespace HappyHarvest
{
    public class FishingPond : InteractiveObject
    {
        public override void InteractedWith()
        {
            UIHandler.OpenFishingSpot();
        }
    }
}
