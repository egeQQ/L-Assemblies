using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Activator
{
    public static class AutoSmite
    {
        private static SpellSlot SmiteSlot;
        private static Spell Special;
        private static Obj_AI_Hero Player = ObjectManager.Player;
        static AutoSmite()
        {
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            if (Player.BaseSkinName == "Nunu") { Special = new Spell(SpellSlot.Q, 125f); }
            if (Player.BaseSkinName == "Olaf") { Special = new Spell(SpellSlot.E, 125f); }
            if (Player.BaseSkinName == "ChoGath") { Special = new Spell(SpellSlot.R, 175f); }
            SmiteSlot = Player.GetSpellSlot("SummonerSmite");
        }

        public static void AddToMenu(Menu menu)
        {
            var smiteMenu = new Menu("Auto Smite", "AutoSmite");

            smiteMenu.AddItem(new MenuItem("AutoSmiteEnabled", "Enabled").SetValue(true));
            smiteMenu.AddItem(new MenuItem("EnableSmallCamps", "Smite small Camps").SetValue(true));
            smiteMenu.AddItem(new MenuItem("AutoSmiteDrawing", "Enable Drawing").SetValue(true));
            if (Player.BaseSkinName == "Olaf") { smiteMenu.AddItem(new MenuItem("OlafE", "Use Olaf E").SetValue(true)); }
            if (Player.BaseSkinName == "Nunu") { smiteMenu.AddItem(new MenuItem("NunuQ", "Use Nunu Q").SetValue(true)); }
            if (Player.BaseSkinName == "ChoGath") { smiteMenu.AddItem(new MenuItem("ChoR", "Use Cho'Gath R").SetValue(true)); }
            menu.AddSubMenu(smiteMenu);
        }

        //Get Monster
        private static readonly string[] MinionNames =
        {
            "Worm", "Dragon", "LizardElder", "AncientGolem", "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith"
        };

        private static readonly string[] SmallMinionNames =
        {
            //Andre add small camps
        };

        private static Obj_AI_Base GetMinion()
        {
            var minionList = MinionManager.GetMinions(Player.ServerPosition, 500, MinionTypes.All, MinionTeam.Neutral);
            var smallCamps = Config.Menu.Item("EnableSmallCamps").GetValue<bool>();
            return smallCamps
                ? minionList.FirstOrDefault(
                    minion => minion.IsValidTarget(500) && MinionNames.Any(name => minion.Name.StartsWith(name)) && SmallMinionNames.Any(smallname => minion.Name.StartsWith(smallname)))
                : minionList.FirstOrDefault(
                    minion => minion.IsValidTarget(500) && MinionNames.Any(name => minion.Name.StartsWith(name)));
        }

        //Kill monster
        private static void KillMinion(Obj_AI_Base minion)
        {
            bool check = isSpellEnabled(Player.BaseSkinName) ? (Special.IsReady() && Player.Distance(minion) <= Orbwalking.GetRealAutoAttackRange(null)+Special.Range) ? (Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) + Special.GetDamage(minion) >= minion.Health) : Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) >= minion.Health : Player.GetSummonerSpellDamage(minion, Damage.SummonerSpell.Smite) >= minion.Health;
            if (SmiteSlot != SpellSlot.Unknown && Player.SummonerSpellbook.CanUseSpell(SmiteSlot) == SpellState.Ready &&
                check)
            {
                if (isSpellEnabled(Player.BaseSkinName)) { Special.Cast(minion); }
                Player.SummonerSpellbook.CastSpell(SmiteSlot, minion);
            }
        }
        public static bool isSpellEnabled(String champName)
        {
            switch (champName)
            {
                case "Nunu":
                    return Config.Menu.Item("NunuQ").GetValue<bool>();
                case "Olaf":
                    return Config.Menu.Item("OlafE").GetValue<bool>();
                case "ChoGath":
                    return Config.Menu.Item("ChoR").GetValue<bool>();
                default:
                    return false;
            }
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (!Config.Menu.Item("AutoSmiteEnabled").GetValue<bool>())
                return;

            var minion = GetMinion();
            if (minion != null)
            {
                KillMinion(minion);
            }
        }

        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!Config.Menu.Item("AutoSmiteEnabled").GetValue<bool>() || !Config.Menu.Item("AutoSmiteDrawing").GetValue<bool>())
                return;

            Utility.DrawCircle(Player.Position, 700, Color.Coral);
        }
    }
}