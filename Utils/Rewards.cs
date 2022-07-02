using System;
using System.Collections.Generic;
using System.Text;
using ProjectM;

namespace ChatCommands.Utils
{

    /*
    Object holding information for Rewards.
    ItemName, ItemGUID, claimreq

    */
    public class Rewards
    {
        string ItemName { get; set; }
        PrefabGUID ItemGUID { get; set; }
        int ItemAmount { get; set; }
        int claimreq { get; set; }


        public Rewards(string iname, PrefabGUID iguid, int creq)
        {
            this.ItemName = iname;
            this.ItemGUID = iguid;
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
    }
}
