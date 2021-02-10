using Random = UnityEngine.Random;

namespace Game.Scripts.GameManagement
{
    public static class PlayerWinChanceCalculator
    {
        private static PlayerData CurrentPlayerData => PlayerSaveData.CurrentData;

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;

        public static void Test()
        {
            var r = new System.Random();
            System.Console.WriteLine(r.Next(0, 0).ToString());
            System.Console.WriteLine(Random.Range(0, 0).ToString());
        }

        public static int GetLeaderboardRatingGroup(int rating)
        {
            int g;
            // leaderboard group 9 means player not in any top 8 groups and is unranked on leaderboard
            if (CurrentPlayerData.gameLeaderboardRating > 5000) g = 9;
            // g=8, the eighth leaderboard group, top 5000 players
            else if (CurrentPlayerData.gameLeaderboardRating > 3000) g = 8;
            // g=7, the seventh leaderboard group, top 3000 players
            else if (CurrentPlayerData.gameLeaderboardRating > 1000) g = 7;
            // g=6, the sixth leaderboard group, top 1000 players
            else if (CurrentPlayerData.gameLeaderboardRating > 750) g = 6;
            // g=5, the fifth leaderboard group, top 750 players
            else if (CurrentPlayerData.gameLeaderboardRating > 500) g = 5;
            // g=4, the fourth leaderboard group, top 500 players
            else if (CurrentPlayerData.gameLeaderboardRating > 100) g = 4;
            // g=3, the third leaderboard group, top 100 players
            else if (CurrentPlayerData.gameLeaderboardRating > 50) g = 3;
            // g=2, the second leaderboard group, top 50 players
            else if (CurrentPlayerData.gameLeaderboardRating > 10) g = 2;
            // g=1, the first leaderboard group, top 10 players
            else g = 1;
            return g;
        }

        // Calculate the player's win chance in percent (0 to 100 [inclusive]), and number of kills and deaths subsequently
        // based on factors including player game rank, current leaderboard rating, total wins, consecutive wins
        public static int CalculateWinChance(out int kills, out int deaths)
        {
            const int maxIntValue = int.MaxValue;

            // chance <11 = definite loss
            var chance = 11;
            kills = deaths = 0;

            var lbGroup = GetLeaderboardRatingGroup(CurrentPlayerData.gameLeaderboardRating);

            // Let the player always win in their first 5 matches
            if (CurrentPlayerData.totalGameWins <= 5)
            {
                chance = 100;

                // deaths and kills here could be random and independent of each other
                // the player can also win the starting few matches by support of teammates and other players helping and teaching
                deaths = Random.Range(0, 9);
                int[] killsNumberChoiceChanceArray = {5, 7, 5, 3, 4, 5, 6, 7, 8, 4, 7, 8, 6, 9, 10, 10, 10};
                kills =
                    // Just select a random from the kills choices defined above more occurence in array = more chance
                    killsNumberChoiceChanceArray[Random.Range(0, killsNumberChoiceChanceArray.Length)] +
                    Random.Range(0, 5);
            }

            // If player is below than the top 8 leaderboard rating groups he should have a high chance to win against new noob players if the player is experienced self
            else switch (lbGroup)
            {
                // the 8th leaderboard group, 5000 to 3000
                case 9:
                {
                    // base 25% win chance increase for playing with low rating players
                    chance += 25;
                    // 5% win chance increase for each new rank player has achieved
                    chance += (CurrentPlayerData.gameRank - 1) * 5 +
                              // a random chance increase based on the previous consecutive wins,
                              // here we clamp the value since consecutive wins can be negative internally to represent consecutive losses
                              Random.Range(0, Clamp(CurrentPlayerData.consecutiveGameWins * 5, 0, maxIntValue));

                    // We deduct 25% chance if the player is getting to many continuous wins,
                    // this can be thought of like when the player is winning too much and then the game matches the player with other opponents
                    // who are also high ranked and going above the leaderboard
                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 25;
                    // A random chance deduct if the consecutive wins is even above 5 if not 7
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(0, 7);

                    // When we have very high win chance it is safe to assume this situation as high ranked/experienced player playing with other low rank/exp opponents
                    // so basically many kills and quite few deaths
                    if (chance > 80)
                    {
                        // limited deaths in this case and kills should be at least 3 higher than deaths
                        deaths = Random.Range(0, 4);
                        int[] killsNumberChoiceChanceArray = {5, 7, 5, 4, 7, 8, 6, 9, 10, 10, 10, 15, 17, 20, 20};
                        kills = Clamp(
                            // Just select a random from the kills choices defined above more occurence in array = more chance
                            killsNumberChoiceChanceArray[Random.Range(0, killsNumberChoiceChanceArray.Length)] +
                            Random.Range(0, 5), deaths + 3, maxIntValue);
                    }
                    else if (chance > 50)
                    {
                        // more deaths allowed with least fixed amount in this case and kills should be at least 1 higher than deaths
                        deaths = Clamp(Random.Range(0, 10), Random.Range(3, 6), 10);
                        int[] killsNumberChoiceChanceArray =
                            {5, 7, 5, 4, 7, 8, 6, 9, 10, 10, 10, 15, 17, 15, 7, 5, 8, 9, 7, 5, 9};
                        kills = Clamp(
                            // Just select a random from the kills choices defined above more occurence in array = more chance
                            killsNumberChoiceChanceArray[Random.Range(0, killsNumberChoiceChanceArray.Length)] +
                            Random.Range(0, 5), deaths + 1, maxIntValue);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(4, 7), 12);
                        kills = Random.Range(Random.Range(6, 8), 20);
                    }

                    break;
                }
                // the 7th leaderboard group, 3000 to 1000
                case 8:
                {
                    // base 20% win chance increase
                    chance += 20;
                    chance +=
                        // 5% win chance increase for each new rank after rank 3 player has achieved
                        Clamp(CurrentPlayerData.gameRank - 3, 0, maxIntValue) * 5 +
                        // a random chance increase based on the previous consecutive wins,
                        // here we clamp the value since consecutive wins can be negative internally to represent consecutive losses
                        Random.Range(0, Clamp(CurrentPlayerData.consecutiveGameWins * 5, 0, maxIntValue));

                    // We deduct 20% chance if the player is getting to many continuous wins,
                    // this can be thought of like when the player is winning too much and then the game matches the player with other opponents
                    // who are also high ranked and going above the leaderboard
                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 20;
                    // A random chance deduct if the consecutive wins is even above 5 if not 7
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(1, 7);

                    if (chance > 80)
                    {
                        // limited deaths in this case and kills should be at least higher than deaths
                        deaths = Random.Range(2, 6);
                        int[] killsNumberChoiceChanceArray = {5, 7, 5, 4, 7, 8, 6, 9, 10, 10, 10, 15, 17, 20, 20};
                        kills = Clamp(
                            // Just select a random from the kills choices defined above more occurence in array = more chance
                            killsNumberChoiceChanceArray[Random.Range(0, killsNumberChoiceChanceArray.Length)] +
                            Random.Range(0, 5), deaths + 1, maxIntValue);
                    }
                    else if (chance > 50)
                    {
                        // more deaths allowed with least fixed amount in this case and kills should be at near to deaths
                        deaths = Random.Range(Random.Range(3, 6), 10);
                        int[] killsNumberChoiceChanceArray =
                            {5, 7, 5, 4, 7, 8, 6, 9, 10, 10, 10, 15, 17, 15, 7, 5, 8, 9, 7, 5, 9};
                        kills = Clamp(
                            // Just select a random from the kills choices defined above more occurence in array = more chance
                            killsNumberChoiceChanceArray[Random.Range(0, killsNumberChoiceChanceArray.Length)] +
                            Random.Range(0, 3), deaths - 1, maxIntValue);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(5, 7), 12);
                        kills = Random.Range(Random.Range(4, 7), 20);
                    }

                    break;
                }
                // the 6th leaderboard group, 1000 to 750
                case 7:
                {
                    chance += 17;
                    chance += Clamp(CurrentPlayerData.gameRank - 6, 0, maxIntValue) * 5 +
                              Random.Range(0, Clamp(CurrentPlayerData.consecutiveGameWins * 5, 0, maxIntValue));

                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 17;
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(1, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(4, 6), 9);
                        kills = Random.Range(Random.Range(4, 10), 22);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(5, 10), 13);
                        kills = Random.Range(Random.Range(4, 9), 20);
                    }

                    break;
                }
                // the 5th leaderboard group, 750 to 500
                case 6:
                {
                    chance += 15;
                    chance += Clamp(CurrentPlayerData.gameRank - 7, 0, maxIntValue) * 7 +
                        CurrentPlayerData.consecutiveGameWins > 1
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 1) * 3, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 15;
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(1, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(4, 6), 9);
                        kills = Random.Range(Random.Range(4, 10), 22);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(5, 10), 14);
                        kills = Random.Range(Random.Range(4, 9), 21);
                    }

                    break;
                }
                // the 4th leaderboard group, 500 to 100
                case 5:
                {
                    chance += 10;
                    chance += Clamp(CurrentPlayerData.gameRank - 9, 0, maxIntValue) * 7 +
                        CurrentPlayerData.consecutiveGameWins > 2
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 1) * 3, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 10;
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(1, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(4, 6), 9);
                        kills = Random.Range(Random.Range(4, 10), 22);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(5, 10), 14);
                        kills = Random.Range(Random.Range(4, 9), 21);
                    }

                    break;
                }
                // the 3rd leaderboard group, 100 to 50
                case 4:
                {
                    chance += 5;
                    chance += Clamp(CurrentPlayerData.gameRank - 12, 0, maxIntValue) * 7 +
                        CurrentPlayerData.consecutiveGameWins > 2
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 1) * 3, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 7) chance -= 5;
                    else if (CurrentPlayerData.consecutiveGameWins > 5) chance -= Random.Range(2, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(4, 6), 11);
                        kills = Random.Range(Random.Range(4, 10), 25);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(5, 10), 17);
                        kills = Random.Range(Random.Range(4, 9), 23);
                    }

                    break;
                }
                // the 2nd leaderboard group, 50 to 10
                case 3:
                {
                    // no base chance increase from here on
                    chance += Clamp(CurrentPlayerData.gameRank - 15, 0, maxIntValue) * 8 +
                        CurrentPlayerData.consecutiveGameWins > 3
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 1) * 3, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 4) chance -= Random.Range(2, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(5, 9), 13);
                        kills = Random.Range(Random.Range(7, 10), 25);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(6, 10), 20);
                        kills = Random.Range(Random.Range(7, 9), 25);
                    }

                    break;
                }
                // the top leaderboard group, top 10
                case 2:
                {
                    chance += Clamp(CurrentPlayerData.gameRank - 17, 0, maxIntValue) * 8 +
                        CurrentPlayerData.consecutiveGameWins > 3
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 2) * 3, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 4) chance -= Random.Range(2, 6);

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(5, 9), 15);
                        kills = Random.Range(Random.Range(7, 10), 27);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(6, 10), 22);
                        kills = Random.Range(Random.Range(7, 9), 27);
                    }

                    break;
                }
                case 1:
                {
                    chance += Clamp(CurrentPlayerData.gameRank - 20, 0, maxIntValue) * 8 +
                        CurrentPlayerData.consecutiveGameWins > 3
                            ? Random.Range(0, Clamp((CurrentPlayerData.consecutiveGameWins - 3) * 4, 0, maxIntValue))
                            : 0;

                    if (CurrentPlayerData.consecutiveGameWins > 4) chance -= Random.Range(2, 6);

                    if (CurrentPlayerData.gameLeaderboardRating == 1)
                    {
                        chance = 50;
                        if (CurrentPlayerData.gameRank > 25) chance += 11;
                    }

                    if (chance > 50)
                    {
                        deaths = Random.Range(Random.Range(5, 9), 25);
                        kills = Random.Range(Random.Range(7, 10), 30);
                    }
                    else
                    {
                        deaths = Random.Range(Random.Range(7, 10), 27);
                        kills = Random.Range(Random.Range(7, 9), 27);
                    }

                    break;
                }
            }

            // Finally a value clamp
            chance = Clamp(chance, 0, 100);
            return chance;
        }
    }
}