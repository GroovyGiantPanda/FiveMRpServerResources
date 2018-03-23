using FamilyRP.Roleplay.SharedClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamilyRP.Roleplay.Enums.Character
{

    /// <summary>
    /// Investigative tool for last cause of death
    /// </summary>
    public enum CauseOfDeath
    {
        Unknown = 0,
        Beaten = 1,
        Stabbed = 2,
        RanOver = 3,
        Bombed = 4,
        Burned = 5,
        ShotWithPistol = 6,
        ShotWithShotgun = 7,
        ShotWithSMG = 8,
        ShotWithAssaultRifle = 9,
        ShotWithSniperRifle = 10
    }


    /// <summary>
    /// USAGE: new CauseOfDeathMapper()[3].ToString()
    /// You're welcome to do this a better way
    /// Death cause ints are 1-60 or so; want to be able to map them to more generic names
    /// TODO: Figure out what each exact number corresponds to
    /// Until then no exact enum will be made
    /// </summary>
    public class CauseOfDeathMapper
    {
        public static Dictionary<int[], CauseOfDeath> CauseOfDeathMapping;
        static CauseOfDeathMapper()
        { 

            // Input mappings here
            CauseOfDeathMapping = new Dictionary<int[], CauseOfDeath>
            {
                {new int[] {}, CauseOfDeath.Unknown }, // Comment on what the weapon is here
                {new int[] {0, 1, 2, 56}, CauseOfDeath.Beaten },
                {new int[] {3}, CauseOfDeath.Stabbed },
                {new int[] {49, 50}, CauseOfDeath.RanOver },
                {new int[] {4, 6, 18, 51}, CauseOfDeath.Bombed },
                {new int[] {5, 19}, CauseOfDeath.Burned },
                {new int[] {7, 9}, CauseOfDeath.ShotWithPistol },
                {new int[] {10, 11}, CauseOfDeath.ShotWithShotgun },
                {new int[] {12, 13, 52}, CauseOfDeath.ShotWithSMG },
                {new int[] {14, 15, 20}, CauseOfDeath.ShotWithAssaultRifle },
                {new int[] {16, 17}, CauseOfDeath.ShotWithSniperRifle }
            };
        }

        public CauseOfDeath this[int reason]
        {
            get
            {
                CauseOfDeath causeOfDeath;

                var cod = CauseOfDeathMapping.Where(mapping => mapping.Key.Contains(reason)).FirstOrDefault();
                if(cod.Equals(default(KeyValuePair<int[], CauseOfDeath>)))
                {
                    Log.Info($"Unknown cause of death index encountered: {reason}");
                    causeOfDeath = CauseOfDeath.Unknown;
                }
                else
                {
                    causeOfDeath = cod.Value;
                }
                return causeOfDeath;
            }
        }
    }

    /// <summary>
    /// Investigative tool for last cause of death
    /// </summary>
    public enum CauseOfDeathBodyPart
    {
        Torso = 3,
        Ass = 4,
        LeftArm = 5,
        RightArm = 6,
        LeftLeg = 7,
        RightLeg = 8,
        Head = 9
    }
}