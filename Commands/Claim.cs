using ChatCommands.Utils;
using ProjectM;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using Wetstone.API;
namespace ChatCommands.Commands
{
    /*
    When a user types ".claim" in the chat run the claim process. Return the proper information to the user.
    -Claim Successful/Unsuccessful
    -Items given
    -Current Claim amount (amount of times users have claimed items)
    -Claim cooldown
    -Time when .claim will be able to be used again

    Issue: 
    https://github.com/TristenSchuler/ChatCommands/issues/4
    */
    [Command("claim", Usage = "claim", Description = "User claims login reward")]
    public static class Claim
    {
        private const int COOLDOWN_IN_HOURS = 24;
        private static Dictionary<ulong, UserClaim> userclaims;
        private static Dictionary<int, Rewards> rewards;
        public static void Initialize(Context ctx)
        {

            loadClaims();
            bool rewardsLoaded = loadRewards();
            if (!rewardsLoaded)
            {
                ctx.Event.User.SendSystemMessage($"Claim Failed - Failed to properly read \"rewards.json\".");
                return;
            }

            try
            {
                UserClaim userclaim;
                Rewards reward;
                int claim_amount = 0;
                bool userExists = userclaims.TryGetValue(ctx.Event.User.PlatformId, out userclaim);

                if (userExists)
                {
                    claim_amount = userclaim.getClaimAmount();
                }

                bool claimExists = rewards.TryGetValue(claim_amount, out reward);

                if (!claimExists)
                {
                    ctx.Event.User.SendSystemMessage($"Claim Failed - No reward set for Claim_Amount: " + claim_amount);
                    return;
                }

                if (tryClaim(ctx, userclaim, reward))
                {
                    userclaim.incrementClaimAmount();
                    userclaim.setLastClaim(DateTime.UtcNow);
                    ctx.Event.User.SendSystemMessage($"Claim Successful!");
                    printGivenItems(ctx, reward);
                    ctx.Event.User.SendSystemMessage($"Current amount of Claims: " + userclaim.getClaimAmount());
                    ctx.Event.User.SendSystemMessage($"You can claim your next reward in: " + getCooldown(userclaim) + " hours");

                }
                else
                {
                    ctx.Event.User.SendSystemMessage($"Claim Failed.");
                    ctx.Event.User.SendSystemMessage($"Current amount of Claims: " + userclaim.getClaimAmount());
                    ctx.Event.User.SendSystemMessage($"You can claim your next reward in: " + getCooldown(userclaim) + " hours");
                }

            }
            catch
            {
                ctx.Event.User.SendSystemMessage($"Claim Failed");
                return;
            }



        }

        private static bool tryClaim(Context ctx, UserClaim uclaim, Rewards r)
        {
            try
            {
                if (getHourDifference(uclaim) >= COOLDOWN_IN_HOURS)
                {
                    CommandHelper.AddItemToInventory(ctx, r.getItemGuid(), r.getItemAmount());
                    return true;
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }

        private static void printGivenItems(Context ctx, Rewards r)
        {
            ctx.Event.User.SendSystemMessage($"You got <color=#ffff00ff>{r.getItemAmount()} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(r.getItemName())}</color>");
        }


        private static int getHourDifference(UserClaim uclaim)
        {
            DateTime claim_dt = DateTime.UtcNow;
            DateTime last_claim_dt = uclaim.getLastClaim();

            TimeSpan span = last_claim_dt.Subtract(claim_dt);
            
            int totalhours = span.Hours + span.Days * 24;

            return totalhours;
        }

        /*
            Loads all UserClaim objects into userclaims(Dictionary<ulong,UserClaim>)
            from BepInEx/config/ChatCommands/userclaims.json
        */
        private static void loadClaims()
        {
            if (!File.Exists("BepInEx/config/ChatCommands/userclaims.json")) File.Create("BepInEx/config/ChatCommands/userclaims.json");
            string json = File.ReadAllText("BepInEx/config/ChatCommands/userclaims.json");
            try
            {
                userclaims = JsonSerializer.Deserialize<Dictionary<ulong, UserClaim>>(json);
            }
            catch
            {
                userclaims = new Dictionary<ulong, UserClaim>();
            }
        }

        private static bool loadRewards()
        {
            if (!File.Exists("BepInEx/config/ChatCommands/rewards.json")) File.Create("BepInEx/config/ChatCommands/rewards.json");
            string json = File.ReadAllText("BepInEx/config/ChatCommands/rewards.json");
            try
            {
                rewards = JsonSerializer.Deserialize<Dictionary<int, Rewards>>(json);
                return true;
            }
            catch
            {
                rewards = new Dictionary<int, Rewards>();
                return false;
            }
        }

    }

}
