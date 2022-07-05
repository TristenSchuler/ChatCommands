using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/*
Object holding information for user claims.
user_id, claim_amount ,last_claim _time, last_claim_date

Issue:
https://github.com/TristenSchuler/ChatCommands/issues/7
*/
namespace ChatCommands.Utils
{   
    [Serializable()]
    public class UserClaim
    {
        public int claim_amount { get; set; }
        public DateTime last_claim { get; set; }

        public UserClaim(int claim_amount, DateTime last_claim)
        {
            this.claim_amount = claim_amount;
            this.last_claim = last_claim;
        }

        public DateTime getLastClaim()
        {
            return last_claim;
        }

        public void setLastClaim(DateTime claimtime)
        {
            this.last_claim = claimtime;
        }

        public int getClaimAmount()
        {
            return claim_amount;
        }

        public void incrementClaimAmount()
        {
            this.claim_amount+=1;
        }

    }
}