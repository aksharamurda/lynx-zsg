namespace LynxStd
{
    public enum Tool
    {
        none = 0,
        radio,
        phone,
        flashlight
    }

    public struct ToolUseDescription
    {
        public bool HasAiming;

        public bool IsContinuous;

        public ToolUseDescription(bool hasAiming, bool isContinuous)
        {
            HasAiming = hasAiming;
            IsContinuous = isContinuous;
        }
    }

    public struct ToolDescription
    {
        public ToolUseDescription Main;

        public ToolUseDescription Alternate;

        public static ToolDescription[] Defaults = GetDefaults();

        public ToolDescription(ToolUseDescription main)
        {
            Main = main;
            Alternate = new ToolUseDescription(false, false);
        }

        public ToolDescription(ToolUseDescription main, ToolUseDescription alternate)
        {
            Main = main;
            Alternate = alternate;
        }

        public bool HasAiming(bool isAlternate)
        {
            return isAlternate ? Alternate.HasAiming : Main.HasAiming;
        }

        public bool IsContinuous(bool isAlternate)
        {
            return isAlternate ? Alternate.IsContinuous : Main.IsContinuous;
        }

        public static ToolDescription[] GetDefaults()
        {
            var descriptions = new ToolDescription[4];

            descriptions[1] = new ToolDescription(new ToolUseDescription(false, false));
            descriptions[2] = new ToolDescription(new ToolUseDescription(true, true), new ToolUseDescription(false, false));
            descriptions[3] = new ToolDescription(new ToolUseDescription(true, true));

            return descriptions;
        }
    }
}
