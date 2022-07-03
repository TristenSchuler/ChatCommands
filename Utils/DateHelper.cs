// Tools for managing/updating WipeDate.txt
using System;
using System.IO;



public class DateHelper
{
    // Change this value to match your servers wipe schedule. 
    public const int DAYS_BETWEEN_WIPES = 7;
    
    //Takes "mm-dd-yyyy" returns int arr [month,day,year]
    public static int[] Parse(string date)
    {
        int[] arr = new int[3];
        bool flag;

        flag = int.TryParse(date.Substring(0, 2), out arr[0]);
        flag = int.TryParse(date.Substring(3, 2), out arr[1]);
        flag = int.TryParse(date.Substring(6), out arr[2]);

        return arr;

    }

    // Takes two int arrays [month,day,year]
    // Returns int array [difference,DaysToRecentMonth,OldMonthsMaximunAmountOfDays,OldDay,RecentMonth, RecentYear]
    public static int[] getDif(int[] old, int[] recent)
    {
        int tempdif = recent[1] - old[1];

        int[] amountOfDays = new int[12];

        if (recent[2] % 4 == 0)
        {
            amountOfDays = new int[] { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        }
        else
        {
            amountOfDays = new int[] { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        }

        int oldmaxday = amountOfDays[old[0] - 1];
        int torecentmonth = oldmaxday - old[1];

        if (recent[0] > old[0])
        {
            return new int[] { torecentmonth + recent[1], torecentmonth, oldmaxday, old[1], recent[0], recent[2] };
        }
        else return new int[] { tempdif, -1, -1, old[1], recent[0], recent[2] };

    }

    // Meant to be used with getDif. Example: int wipeday = getWipeDay(getDif(old,recent))
    // Takes int array [difference,DaysToRecentMonth,OldMonthsMaximunAmountOfDays,OldDay,RecentMonth, RecentYear]
    // Returns int array[month,day,year]
    public static int[] getWipeDate(int[] getDifResult)
    {
        int tempdif = getDifResult[0];
        int torecentmonth = getDifResult[1];
        int oldday = getDifResult[3];

        //diff month
        if (torecentmonth >= 0)
        {
            if (tempdif < DAYS_BETWEEN_WIPES)

                return new int[] { getDifResult[4], oldday, getDifResult[5] };

            else if (tempdif % DAYS_BETWEEN_WIPES == 0)
                return new int[] { getDifResult[4], tempdif - torecentmonth, getDifResult[5] };
            else
            {
                for (int i = 0; i < DAYS_BETWEEN_WIPES; i++)
                {
                    if (tempdif % DAYS_BETWEEN_WIPES == 0)
                        return new int[] { getDifResult[4], tempdif - torecentmonth, getDifResult[5] };
                    else
                        tempdif--;
                }
                return new int[] { getDifResult[4], tempdif - torecentmonth, getDifResult[5] };
            }

        }
        // same month
        else
        {
            if (tempdif < DAYS_BETWEEN_WIPES)
                return new int[] { getDifResult[4], oldday, getDifResult[5] };

            for (int i = 0; i < DAYS_BETWEEN_WIPES; i++)
            {
                if (tempdif % DAYS_BETWEEN_WIPES == 0)
                    return new int[] { getDifResult[4], getDifResult[0] + oldday, getDifResult[5] };
                else
                    tempdif--;
            }
            return new int[] { getDifResult[4], tempdif + oldday, getDifResult[5] };
        }



        //same month
    }

    // For writing to WipeDate.txt 
    // Writes mm-dd-yyyy
    // Taken from https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file
    public static void writeDate(int[] date)
    {
        string month, day;
        month = day = "";
        int monthnum = date[0];
        int daynum = date[1];

        if (monthnum < 10)
            month = "0" + monthnum.ToString();
        else
            month = monthnum.ToString();

        if (daynum < 10)
            day = "0" + daynum.ToString();
        else
            day = daynum.ToString();

        string text = month + "-" + day + "-" + date[2].ToString();

        File.WriteAllText(@"BepInEx/config/ChatCommands/WipeData.txt", text);

    }
}



