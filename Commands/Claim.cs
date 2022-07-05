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

    [Serializable()]
    public static class Claim
    {
        private const int COOLDOWN_IN_HOURS = 24;


        public static Dictionary<ulong, UserClaim> userclaims;
        public static Dictionary<int, Rewards> rewards;
        public static void Initialize(Context ctx)
        {

            bool claimsLoaded = loadClaims();
            bool rewardsLoaded = loadRewards();


            if (!rewardsLoaded)
            {
                ctx.Event.User.SendSystemMessage($"Claim Failed - Failed to properly read \"rewards.json\".");
                return;
            }

            if (!claimsLoaded)
            {
                ctx.Event.User.SendSystemMessage($"Claim Failed - Failed to properly read \"userclaims.json\". Ignore if first claim since wipe.");
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
                else
                {
                    userclaim = new UserClaim(claim_amount, DateTime.UtcNow.AddHours((COOLDOWN_IN_HOURS + 1) * -1));
                    userclaims.Add(ctx.Event.User.PlatformId, userclaim);
                }

                bool claimExists = rewards.TryGetValue(claim_amount, out reward);

                if (!claimExists)
                {
                    ctx.Event.User.SendSystemMessage($"You've claimed everything you can this wipe. Thank you for hanging out with us :" + ')');
                    DateTime wipedate = DateHelper.getWipeDateTime();
                    ctx.Event.User.SendSystemMessage($"Your rewards will reset next wipe which is " + wipedate.AddDays(DateHelper.DAYS_BETWEEN_WIPES));
                    return;
                }

                if (tryClaim(ctx, userclaim, reward))
                {
                    userclaim.incrementClaimAmount();
                    userclaim.setLastClaim(DateTime.UtcNow);
                    ctx.Event.User.SendSystemMessage($"Claim Successful!");
                    printGivenItems(ctx, reward);
                    ctx.Event.User.SendSystemMessage($"You have claimed " + userclaim.getClaimAmount() + " rewards this wipe.");
                    ctx.Event.User.SendSystemMessage($"You can claim your next reward in: " + (getHourDifference(userclaim)) + " hours");
                    userclaims.Remove(ctx.Event.User.PlatformId);
                    userclaims.Add(ctx.Event.User.PlatformId, userclaim);

                }
                else
                {
                    ctx.Event.User.SendSystemMessage($"Claim Failed.");
                    ctx.Event.User.SendSystemMessage($"You have claimed " + userclaim.getClaimAmount() + " rewards this wipe.");
                    ctx.Event.User.SendSystemMessage($"You can claim your next reward in: " + (getHourDifference(userclaim)) + " hours");
                }

            }
            catch
            {
                ctx.Event.User.SendSystemMessage($"Claim Failed.");
                return;
            }


            updateClaims();

        }


        public static void resetClaims()
        {
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine("----------------------IN RESETCLAIMS()-----------------------------------------------------");
            Console.Write(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            userclaims = new Dictionary<ulong, UserClaim>();
            updateClaims();

        }

        private static bool tryClaim(Context ctx, UserClaim uclaim, Rewards r)
        {
            try
            {
                ctx.Event.User.SendSystemMessage($"" + getHourDifference(uclaim));
                if (getHourDifference(uclaim) < 1)
                {

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("----------------------PASSED HOUR DIFF--------------------------------");
                    Console.WriteLine();
                    Console.WriteLine();
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

            TimeSpan span = claim_dt.Subtract(last_claim_dt);


            int totalhours = span.Hours + span.Days * 24;

            Console.WriteLine();
            Console.WriteLine("------------------------------HOUR DIFF DEBUG---------------------------------");
            Console.WriteLine();
            Console.WriteLine(span.Hours + ", " + span.Days + ", " + totalhours);
            Console.WriteLine();
            Console.WriteLine();


            return COOLDOWN_IN_HOURS - totalhours;
        }

        /*
            Loads all UserClaim objects into userclaims(Dictionary<ulong,UserClaim>)
            from BepInEx/config/ChatCommands/userclaims.json
        */

        private static bool loadClaims()
        {

            try
            {
                string json = File.ReadAllText("BepInEx/config/ChatCommands/userclaims.json");
                userclaims = JsonSerializer.Deserialize<Dictionary<ulong, UserClaim>>(json);
                Console.WriteLine("loaded increment: " + userclaims.First().Value.claim_amount);
                return true;
            }
            catch (Exception e)
            {
                userclaims = new Dictionary<ulong, UserClaim>();
                Console.WriteLine("Failed to load User Claims. Exception: " + e);
                return false;
            }
        }



        // Example: "0": {Bones,PrefabGuid(1821405450),300,0},
        //          012345678901                     
        private static string[] splitVars(string l)
        {
            string[] vars = new string[5];

            // key
            vars[0] = l.Substring(1, l.IndexOf(':') - 2);

            // ItemName
            int start = l.IndexOf('{') + 1;
            int end = l.IndexOf(',');
            vars[1] = l.Substring(start, end - start);

            // ItemGUID
            string nextchunk = l.Substring(l.IndexOf(',') + 1);
            // Ex: PrefabGuid(1821405450),300,0},
            //     012345678901234567890123
            end = nextchunk.IndexOf(',');
            vars[2] = nextchunk.Substring(0, end);

            // ItemAmount
            nextchunk = nextchunk.Substring(nextchunk.IndexOf(',') + 1);
            // Ex: 300,0},
            //     0123456
            end = nextchunk.IndexOf(',');
            vars[3] = nextchunk.Substring(0, end);

            // claimreq
            vars[4] = nextchunk.Substring(end + 1, nextchunk.IndexOf('}') - (end + 1));

            return vars;
        }


        /*
            Example:
            {
            "0": {Bones,PrefabGuid(1821405450),300,0},
            "1": {Stone Bricks,PrefabGuid(1788016417),500,1},
            "2": {Iron Ingots,PrefabGuid(-1750550553),50,2},
            "3": {Sapphire Pendant,PrefabGuid(-651554566),1,3},
            "4": {Bloodmoon Chestguard,PrefabGuid(488592933),1,4},
            "5": {Sanguine Spear,PrefabGuid(-850142339),1,5},
            }
        */
        private static Dictionary<int, Rewards> customDeserialize(string j)
        {

            using (var reader = new StringReader(j))
            {
                Dictionary<int, Rewards> dict = new Dictionary<int, Rewards>();

                //[key,itemname,guid,itemamount,creq]
                string[] varsplit = new string[5];

                string lineskipped = reader.ReadLine();

                Rewards r;
                string guid = "";
                int guidstart = 0;
                char open = '(';
                char close = ')';

                for (string line = reader.ReadLine(); line != "}"; line = reader.ReadLine())
                {
                    Console.WriteLine("----------------LOOP START-------------------");
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine(line);
                    varsplit = splitVars(line);
                    guid = varsplit[2];
                    guidstart = guid.IndexOf(open) + 1;
                    guid = guid.Substring(guidstart, guid.IndexOf(close) - guidstart);
                    Console.WriteLine(varsplit[0] + " " + varsplit[1] + " " + guid + " " + varsplit[3] + " " + varsplit[4]);
                    Console.WriteLine();
                    Console.WriteLine();
                    r = new Rewards(varsplit[1], new PrefabGUID(Int32.Parse(guid)), Int32.Parse(varsplit[3]), Int32.Parse(varsplit[4]));
                    dict.Add(Int32.Parse(varsplit[0]), r);
                    Console.WriteLine(Rewards.dictToString(dict));
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("---------------LOOP END-----------------------");
                }
                return dict;
            }
        }
        private static bool loadRewards()
        {
            string json = File.ReadAllText("BepInEx/config/ChatCommands/rewards.json");
            try
            {
                rewards = customDeserialize(json);
                return true;
            }
            catch
            {
                rewards = new Dictionary<int, Rewards>();
                return false;
            }
        }

        private static void updateClaims()
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                IncludeFields = true
            };
            try
            {
                File.WriteAllText("BepInEx/config/ChatCommands/userclaims.json", JsonSerializer.Serialize(userclaims, options));
            }
            catch
            {
                Console.WriteLine("Failed to update userclaims.json.");
            }
        }

    }

}
