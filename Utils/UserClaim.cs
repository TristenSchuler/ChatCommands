using System;
using System.Collections.Generic;
using System.Text;

namespace ChatCommands.Utils
{

    /*
    Object holding information for user claims.
    user_id, claim_amount ,last_claim _time, last_claim_date

    Issue:
    https://github.com/TristenSchuler/ChatCommands/issues/7
    */
    public class UserClaim
    {
        int claim_amount { get; set; }
        DateTime last_claim { get; set; }

        public UserClaim(int c_amount, DateTime l_claim)
        {
            this.claim_amount = c_amount;
            this.last_claim = l_claim;
        }

        public DateTime getLastClaim()
        {
            return last_claim;
        }

        public int getClaimAmount()
        {
            return claim_amount;
        }

        public void incrementClaimAmount()
        {
            this.claim_amount++;
        }

    }
}
