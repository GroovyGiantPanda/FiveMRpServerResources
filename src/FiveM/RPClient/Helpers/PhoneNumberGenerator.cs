using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyRP.Roleplay.Client.Helpers
{
    // TODO: Make shared? Maybe server should generate these (so clients can't claim they randomed phone # "1" or "11111" or whatever)
    class PhoneNumberGenerator
    {
        // Chose this so all phone numbers are equal length
        // Maybe we want them even more realistic?
        // TODO
        const int minNumber = 10000;
        const int maxNumber = 99999;

        public static int Generate()
        {
            Random r = new Random();
            int rInt;
            while(IsPhoneNumberInUse(rInt = r.Next(minNumber, maxNumber))) { }
            return rInt;
        }

        public static bool IsPhoneNumberInUse(int number)
        {
            return false;
            // TODO: Implement check
        }
    }
}
