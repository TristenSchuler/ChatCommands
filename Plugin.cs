using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using ChatCommands.Utils;
using HarmonyLib;
using Newtonsoft.Json;
using ProjectM;
using System;
using System.IO;
using System.Reflection;
using Wetstone.API;
using Wetstone.Hooks;

namespace ChatCommands
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("xyz.molenzwiebel.wetstone")]
    [Reloadable]
    public class Plugin : BasePlugin
    {
        private Harmony harmony;

        private CommandHandler cmd;
        private ConfigEntry<string> Prefix;
        private ConfigEntry<string> DisabledCommands;
        private ConfigEntry<int> WaypointLimit;

        private void InitConfig()
        {
            Prefix = Config.Bind("Config", "Prefix", "?", "The prefix used for chat commands.");
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
        }

        // For correctly updating most recent wipe date. Only works if the difference between the months < 2. Ex: June,July works and June,August doesnt work
        // You can always manually update WipeDate.txt to start fresh, and this function will take it from there as long as its ran once a month. 
        public void OnGameInitialized()
        {
            string filestring = System.IO.File.ReadAllText(@"WipeDate.txt");
            string newstring = DateTime.UtcNow.ToString("MM-dd-yyyy");

            // [month,day,year]
            int [] filedate = DateHelper.Parse(filestring);
            int [] currentdate = DateHelper.Parse(newstring);
            int [] wipedate = DateHelper.getWipeDate(DateHelper.getDif(filedate,currentdate));
            DateHelper.writeDate(wipedate);

        }

        public override void Load()
        {
            InitConfig();
            cmd = new CommandHandler(Prefix.Value, DisabledCommands.Value);
            Chat.OnChatMessage += HandleChatMessage;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
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
