namespace ItemSystem
{
    public struct InventoryItem
    {
        public AbstractItemDataSo ItemData {get; private set;}
        public int StackSize { get; private set; }
        public int SlotNumber {get; set;}
        
        public bool IsFullStack => StackSize >= ItemData.maxStack;

        public InventoryItem(AbstractItemDataSo itemData, int slotNumber, int stackSize = 1)
        {
            ItemData = itemData;
            SlotNumber = slotNumber;
            StackSize = stackSize;
        }

        public int AddStack(int count)
        {
            int remainCount = 0;
            StackSize += count;

            if (StackSize >= ItemData.maxStack)
            {
                remainCount = StackSize - ItemData.maxStack;
                StackSize = ItemData.maxStack;
            }

            return remainCount;
        }
        
        public void RemoveStack(int count) => StackSize -= count;
    }
}