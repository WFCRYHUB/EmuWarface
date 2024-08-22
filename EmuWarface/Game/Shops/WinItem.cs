namespace EmuWarface.Game.Shops
{
    public struct WinItem
    {
        //sr31_shop,5400,36000;sr31_gold01_shop,0,0;
        //mg23_shop,5400,36000;mg23_gold01_shop,5400,36000;

        public readonly string Name;
        public readonly int RepairCost;
        public readonly int DurabilityPoints;

        public WinItem(string name, int repairCost, int durabilityPoints)
        {
            Name = name;
            RepairCost = repairCost;
            DurabilityPoints = durabilityPoints;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WinItem))
                return false;

            var item = (WinItem)obj;
            return Name == item.Name &&
                RepairCost == item.RepairCost &&
                DurabilityPoints == item.DurabilityPoints;
        }

        public static bool operator ==(WinItem item1, WinItem item2)
        {
            return item1.Equals(item2);
        }

        public static bool operator !=(WinItem item1, WinItem item2)
        {
            return !(item1 == item2);
        }
    }
}
