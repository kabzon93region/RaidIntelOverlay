using System;

using System.Collections.Generic;



namespace RaidIntelOverlay

{

    internal static class WildSpawnTypeLabels

    {

        private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)

        {

            ["marksman"] = "Снайпер",

            ["assault"] = "Дикий",

            ["cursedAssault"] = "Проклятый",

            ["assaultGroup"] = "Группа диких",

            ["pmcBot"] = "PMC бот",

            ["pmcUSEC"] = "USEC",

            ["pmcBEAR"] = "BEAR",

            ["exUsec"] = "Отступник",

            ["arenaFighter"] = "Arena",

            ["bossKnight"] = "Knight",

            ["followerBigPipe"] = "Big Pipe",

            ["followerBirdEye"] = "Bird Eye",

            ["bossKilla"] = "Killa",

            ["bossTagilla"] = "Tagilla",

            ["bossGluhar"] = "Глухарь",

            ["bossSanitar"] = "Sanitar",

            // В игре WildSpawnType.bossKojaniy — это Штурман (Woods), не «Кожаный»

            ["bossKojaniy"] = "Штурман",

            ["followerKojaniy"] = "Свита Штурмана",

            ["bossBully"] = "Решала",

            ["followerBully"] = "Свита Решалы",

            ["bossBoar"] = "Кабан",

            ["followerBoar"] = "Свита Кабана",

            ["followerBoarClose1"] = "Свита Кабана",

            ["followerBoarClose2"] = "Свита Кабана",

            ["bossBoarSniper"] = "Снайпер Кабана",

            ["bossKolontay"] = "Колонтай",

            ["followerKolontayAssault"] = "Свита Колонтая",

            ["followerKolontaySecurity"] = "Свита Колонтая",

            ["bossPartisan"] = "Партизан",

            ["bossZryachiy"] = "Зрячий",

            ["followerZryachiy"] = "Свита Зрячего",

            ["followerGluharAssault"] = "Свита Глухаря",

            ["followerGluharSecurity"] = "Свита Глухаря",

            ["followerGluharScout"] = "Свита Глухаря",

            ["followerGluharSnipe"] = "Снайпер Глухаря",

            ["followerSanitar"] = "Свита Sanitar",

            ["followerTagilla"] = "Свита Tagilla",

            ["shooterBTR"] = "BTR",

            ["infectedAssault"] = "Infected",

            ["infectedPmc"] = "Infected PMC",

            ["infectedTagilla"] = "Infected Tagilla",

            ["bossTagillaAgro"] = "Tagilla (агро)",

            ["bossKillaAgro"] = "Killa (агро)"

        };



        public static string GetDisplayName(string roleName)

        {

            if (string.IsNullOrEmpty(roleName))

            {

                return "?";

            }



            if (Labels.TryGetValue(roleName, out var label))

            {

                return label;

            }



            if (roleName.StartsWith("boss", StringComparison.OrdinalIgnoreCase))

            {

                return "Босс " + roleName.Substring(4);

            }



            if (roleName.StartsWith("follower", StringComparison.OrdinalIgnoreCase))

            {

                return "Свита " + roleName.Substring(8);

            }



            return roleName;

        }

    }

}


