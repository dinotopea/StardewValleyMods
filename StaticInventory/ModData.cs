namespace StaticInventory
{
    public sealed class ModData
    {
        public int ToolbarOffset { get; set; } = 0;
        public bool IsShifted { get; set; } = false;

        public ModData() {}

        public ModData(int toolbarOffset, bool isShifted)
        {
            ToolbarOffset = toolbarOffset;
            IsShifted = isShifted;
        }
    }
}
