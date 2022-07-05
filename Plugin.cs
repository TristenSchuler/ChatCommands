using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using ChatCommands.Utils;
using HarmonyLib;
using System.Text.Json;
using ProjectM;
using System;
using System.IO;
using System.Reflection;
using Wetstone.API;
using Wetstone.Hooks;
using System.Collections.Generic;
using System.Linq;
using ChatCommands.Commands;


namespace ChatCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin
    {
        const string INITIALWIPEDATE = "07-01-2022";
        private bool isWipeFileFound = false;
        private Harmony harmony;

        private CommandHandler cmd;
        private ConfigEntry<string> Prefix;
        private ConfigEntry<string> DisabledCommands;
        private ConfigEntry<int> WaypointLimit;



        private void InitConfig()
        {

            Prefix = Config.Bind("Config", "Prefix", ".", "The prefix used for chat commands.");
            DisabledCommands = Config.Bind("Config", "Disabled Commands", "", "Enter command names to disable them. Seperated by commas. Ex.: health,speed");
            WaypointLimit = Config.Bind("Config", "Waypoint Limit", 3, "Sets a waypoint limit per user.");


            if (!File.Exists("BepInEx/config/ChatCommands/kits.json"))
            {
                if (!Directory.Exists("BepInEx/config/ChatCommands")) Directory.CreateDirectory("BepInEx/config/ChatCommands");
                File.Create("BepInEx/config/ChatCommands/kits.json");
            }

            if (!File.Exists("BepInEx/config/ChatCommands/permissions.json"))
            {
                if (!Directory.Exists("BepInEx/config/ChatCommands")) Directory.CreateDirectory("BepInEx/config/ChatCommands");
                File.Create("BepInEx/config/ChatCommands/permissions.json");
            }
            if (!File.Exists("BepInEx/config/ChatCommands/userclaims.json"))
            {
                if (!Directory.Exists("BepInEx/config/ChatCommands")) Directory.CreateDirectory("BepInEx/config/ChatCommands");
                File.Create("BepInEx/config/ChatCommands/userclaims.json").Dispose();
            }
            if (!File.Exists("BepInEx/config/ChatCommands/rewards.json"))
            {
                if (!Directory.Exists("BepInEx/config/ChatCommands")) Directory.CreateDirectory("BepInEx/config/ChatCommands");
                File.Create("BepInEx/config/ChatCommands/rewards.json").Dispose();
            }
            if (!File.Exists("BepInEx/config/ChatCommands/WipeData.txt"))
            {
                if (!Directory.Exists("BepInEx/config/ChatCommands")) Directory.CreateDirectory("BepInEx/config/ChatCommands");
                File.Create("BepInEx/config/ChatCommands/WipeData.txt").Dispose();
            }
            else
            {
                isWipeFileFound = true;
            }

        }

        public override void Load()
        {
            InitConfig();
            cmd = new CommandHandler(Prefix.Value, DisabledCommands.Value);
            Chat.OnChatMessage += HandleChatMessage;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            string filestring = "";
            string newstring = DateTime.UtcNow.ToString("MM-dd-yyyy");

            if (isWipeFileFound)
                filestring = System.IO.File.ReadAllText(@"BepInEx/config/ChatCommands/WipeData.txt");
            else
                filestring = INITIALWIPEDATE;



            TimeSpan span = DateTime.UtcNow.Subtract(DateHelper.getWipeDateTime());

            // Empties userclaims.json if its the start of a new wipe. 
            // && DateTime.UtcNow.Hour >= DateHelper.WIPEHOUR
            if (span.Days >= DateHelper.DAYS_BETWEEN_WIPES)
            {
                Claim.resetClaims();
            }
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine("--------------------------DEBUG DATA----------------------");
            Console.Write("span.Days,utcNow.Hour = " + span.Days + "," + DateTime.UtcNow.Hour);
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            
            DateHelper.writeDate(DateHelper.getWipeDate(DateHelper.getDif(DateHelper.Parse(filestring), DateHelper.Parse(newstring))));
            Rewards.writeRewards();


        }

        public override bool Unload()
        {
            Config.Clear();
            Chat.OnChatMessage -= HandleChatMessage;
            return true;
        }

        private void HandleChatMessage(VChatEvent ev)
        {
            cmd.HandleCommands(ev, Log, Config);
        }

    }
}
