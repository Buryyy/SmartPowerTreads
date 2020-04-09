using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Items;
using Ensage.SDK.Handlers;
using Ensage.SDK.Helpers;
using Ensage.SDK.Service;
using Ensage.SDK.Service.Metadata;
using SmartPowerTreads.Configuration;
using Attribute = Ensage.Attribute;

namespace SmartPowerTreads
{
    [ExportPlugin("Smart Power Treads", author: "nakama")]
    public class SmartPowerTreads : Plugin
    {
        private readonly Hero _hero;
        private readonly MenuConfiguration _configs;

        private readonly IServiceContext _serviceContext;
        private readonly IUpdateHandler _updateHandler;
        private readonly Random _random;

        private bool _isSwitching;

        [ImportingConstructor]
        public SmartPowerTreads(IServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
            _hero = serviceContext.Owner as Hero;
            _updateHandler = UpdateManager.Subscribe(OnGameUpdate, 200);
            _random = new Random();
            _configs = new MenuConfiguration();
        }

        private PowerTreads Treads
        {
            get
            {
                var treads = _hero.FindItem("item_power_treads");

                return treads == null || !treads.IsEnabled ? null : treads as PowerTreads;
            }
        }

        protected override void OnActivate()
        {
            Entity.OnAnimationChanged += OnAnimationChanged;
            _updateHandler.TryActivate();
        }

        protected override void OnDeactivate()
        {
            Entity.OnAnimationChanged -= OnAnimationChanged;
            _updateHandler.TryDeactivate();
            _configs.DisposeFactory();
        }

        private async void OnGameUpdate()
        {
            if (!_hero.IsAlive || Game.IsPaused)
            {
                return;
            }
            var boots = Treads;

            if (boots == null) return;

            if (_configs.AutoRegeneration.Value && !_hero.IsAttacking())
            {
                await OnAutoRegeneration(boots);
            }
        }

        private async void OnAnimationChanged(Entity sender, EventArgs args)
        {
            if (sender.Owner != _hero.Owner) return;

            var boots = Treads;

            if (boots == null || IsLowHealth) return;
            
            if (sender.Animation.Name.Contains("attack") && boots.ActiveAttribute != _hero.PrimaryAttribute && _hero.IsAttacking() && !_isSwitching)
            {
                await SetBootsAttributeAsync(boots, _hero.PrimaryAttribute);
            }
        }

        private async Task OnAutoRegeneration(PowerTreads treads)
        {
            var unitsWithinRange = ObjectManager.GetEntities<Unit>().Any(
                u => u.IsValid && u.IsAlive && u.Team != _hero.Team
                && _hero.Distance2D(u) < _hero.GetAttackRange() + (_hero.IsMelee ? 300 : 100));

            if (IsLowHealth)
            {
                await SetBootsAttributeAsync(treads, Attribute.Strength);
                return;
            }

            if (unitsWithinRange
                || _hero.Animation.Name.Contains("attack")
                || _hero.IsAttacking()
                || _hero.Target != null) return;


            if (_configs.SwitchingDelay.Value > 0)
            {
                await Task.Delay(_configs.SwitchingDelay.Value * 1000);
            }
            if (IsFullManaAndHealth)
            {
                await SetBootsAttributeAsync(treads, _hero.PrimaryAttribute);
                return;
            }

        
            if (!_hero.Mana.Equals(_hero.MaximumMana))
            {
                await SetBootsAttributeAsync(treads, Attribute.Intelligence);
            }
            else
            {
                await SetBootsAttributeAsync(treads, Attribute.Strength);
            }
        }
        /*
         *
         *     if (_hero.Health < _hero.MaximumHealth / 3 && _configs.StrengthOnLowHP.Value) //Sit on strength if hp is low.
            {
                await SetBootsAttributeAsync(treads, Attribute.Strength);
                return;
            }
         */
        

        public bool IsLowHealth => _hero.Health < _hero.MaximumHealth / 3 && _configs.StrengthOnLowHP.Value;

        public bool IsFullManaAndHealth => _hero.Mana.Equals(_hero.MaximumMana) && _hero.Health.Equals(_hero.MaximumHealth);
        

        private async Task SetBootsAttributeAsync(PowerTreads boots, Attribute targetAttribute)
        {
            if (boots.ActiveAttribute == targetAttribute || _isSwitching) return;

            while (boots.ActiveAttribute != targetAttribute)
            {
                _isSwitching = true;
                boots.UseAbility(); 
                await Task.Delay(_configs.SwitchingSpeed.Value + (_configs.RandomizeSwitches ? _random.Next(5, 50) : 0)); //Randomize this a bit..
            }
            _isSwitching = false;
        }
    }
}