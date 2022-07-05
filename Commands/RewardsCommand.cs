using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Wetstone.API;
using System.Linq;
using ChatCommands.Utils;
using ProjectM;

namespace ChatCommands.Commands
{
    /*
    When a user enters ".rewards" into chat. Print the available rewards connected to the amount of claims required for those rewards.

    Ex:
    Below are the login rewards for this wipe. You can claim once every 24 hours. Enter ".claim" in chat ASAP to start stacking your rewards!
    Login Rewards:
    1st Claim - 300 bones
    2nd Claim - 500 stone brick
    3rd Claim - 50 iron bars
    4th Claim - Sapphire Pendant
    5th Claim - Bloodmoon Chestguard
    6th Claim - Sanguine Spear

    Issue: #5 https://github.com/TristenSchuler/ChatCommands/issues/5
    */
    [Command("rewards", Usage = "rewards", Description = "Shows user all login rewards and their requirements.")]
    public static class RewardsClaim
    {
        private const int COOLDOWN_IN_HOURS = 24;

        public static void Initialize(Context ctx)
        {
            
            ctx.Event.User.SendSystemMessage
            ($"" + Environment.NewLine
            + "Below are the login rewards for this wipe. You can claim once every 24 hours. Enter \".claim\" in chat ASAP to start stacking your rewards!" + Environment.NewLine 
            + "Login Rewards:" + Environment.NewLine + "1st Claim - 300 Bones" + Environment.NewLine + "2nd Claim - 500 Stone Bricks" + Environment.NewLine + Environment.NewLine
            + "3rd Claim - 50 Iron Bars" + Environment.NewLine + "4th Claim - Sapphire Pendant" + Environment.NewLine + "5th Claim - Bloodmoon Chestguard" + Environment.NewLine 
            + "6th Claim - Sanguine Spear" + Environment.NewLine);
        }

    }

}
