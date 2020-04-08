using Ensage.Common.Menu;
using Ensage.SDK.Menu;

namespace SmartPowerTreads.Configuration
{
    public class MenuConfiguration
    {
        private readonly MenuFactory _factory;

        public MenuItem<Slider> SwitchingDelay { get; }
        public MenuItem<Slider> SwitchingSpeed { get; }
        public MenuItem<bool> AutoRegeneration { get; }
        public MenuItem<bool> StrengthOnLowHP { get; }

        public MenuConfiguration()
        {
            _factory = MenuFactory.CreateWithTexture("Smart Power Treads", "item_power_treads");
            AutoRegeneration = _factory.Item("Auto Regeneration", true);
            StrengthOnLowHP = _factory.Item("Switch to strength on low HP", true);
            SwitchingDelay = _factory.Item("Switching delay to regen", new Slider(0, 0, 10));
            SwitchingSpeed = _factory.Item("Switching speed", new Slider(180, 100, 5000));
            //Tooltips
            AutoRegeneration.Item.Tooltip = "When toggled boots will switch to int or strength depending on if you don't have max mana or health.";
            SwitchingDelay.Item.Tooltip = "Switching delay in seconds before boots switches to regen mode. This is in seconds.";
            SwitchingSpeed.Item.Tooltip = "The switching speed interval in milliseconds when boots are switching to targeted attribute. This is also randomized by 5-50.";
            StrengthOnLowHP.Item.Tooltip = "Switches to strength if the following condition happens: (hero max health / 3).";
        }

        public void DisposeFactory()
        {
            _factory.Dispose();
        }
    }
}