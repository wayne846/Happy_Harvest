namespace HappyHarvest
{
    public class UpgradeMarketStall : InteractiveObject
    {
        public override void InteractedWith()
        {
            UIHandler.OpenUpgradeMarket();
        }
    }
}
