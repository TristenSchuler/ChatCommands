using System;
using System.Collections.Generic;
using ProjectM;
using System.IO;
using System.Linq;
using System.Text.Json;

/*
Object holding information for Rewards.
ItemName, ItemGUID, claimreq

*/
namespace ChatCommands.Utils
{
    public class Rewards
    {
        string ItemName { get; set; }
        PrefabGUID ItemGUID { get; set; }
        int ItemAmount { get; set; }
        int claimreq { get; set; }


        public Rewards(string iname, PrefabGUID iguid, int i_amount, int creq)
        {
            this.ItemName = iname;
            this.ItemGUID = iguid;
            this.ItemAmount = i_amount;
            this.claimreq = creq;
        }

        public PrefabGUID getItemGuid()
        {
            return this.ItemGUID;
        }

        public int getItemAmount()
        {
            return this.ItemAmount;
        }

        public string getItemName()
        {
            return this.ItemName;
        }

        public int getClaimReq()
        {
            return this.claimreq;
        }

        public override string ToString()
        {
            return this.ItemName + "," + this.ItemGUID + "," + this.ItemAmount + "," + this.claimreq;
        }

        //"0": {},
        public static string dictToString(Dictionary<int, Rewards> d)
        {
            string result = "{" + Environment.NewLine;
            foreach (KeyValuePair<int, Rewards> entry in d)
            {
                result += "\"" + (int)entry.Key + "\"" + ": {" + entry.Value.ToString() + "},"+ Environment.NewLine;
            }

            result += "}";

            return result;   
        }


        /*
            Login Rewards:
            1st Claim - 300 bones
            2nd Claim - 500 stone brick
            3rd Claim - 50 iron bars
            4th Claim - a mid tier neck
            5th Claim - a sanguine armor piece
            6th Claim - a sanguine weapon
        */
        public static void writeRewards()
        {
            Dictionary<int, Rewards> dict = new Dictionary<int, Rewards>();

            string name1 = "Bones";
            string name2 = "Stone Bricks";
            string name3 = "Iron Ingots";
            string name4 = "Sapphire Pendant";
            string name5 = "Bloodmoon Chestguard";
            string name6 = "Sanguine Spear";

            PrefabGUID g1 = new PrefabGUID(1821405450);
            PrefabGUID g2 = new PrefabGUID(1788016417);
            PrefabGUID g3 = new PrefabGUID(-1750550553);
            PrefabGUID g4 = new PrefabGUID(-651554566);
            PrefabGUID g5 = new PrefabGUID(488592933);
            PrefabGUID g6 = new PrefabGUID(-850142339);

            Rewards c1 = new Rewards(name1, g1, 300, 0);
            Rewards c2 = new Rewards(name2, g2, 500, 1);
            Rewards c3 = new Rewards(name3, g3, 50, 2);
            Rewards c4 = new Rewards(name4, g4, 1, 3);
            Rewards c5 = new Rewards(name5, g5, 1, 4);
            Rewards c6 = new Rewards(name6, g6, 1, 5);

            List<Rewards> rlist = new List<Rewards>();
            rlist.Add(c1);
            rlist.Add(c2);
            rlist.Add(c3);
            rlist.Add(c4);
            rlist.Add(c5);
            rlist.Add(c6);
            Rewards r;
            for (int i = 0; i < 6; i++)
            {
                r = rlist.ElementAt(i);
                dict.Add(i, r);
            }
            

            try
            {
                File.WriteAllText("BepInEx/config/ChatCommands/rewards.json", dictToString(dict));
                Console.WriteLine("Updated rewards.json");
            }
            catch
            {
                Console.WriteLine("Failed to write to rewards.json");
            }

        }
        
    }
}
