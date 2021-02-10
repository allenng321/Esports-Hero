using System;
using UnityEngine;

namespace Game.Scripts.GameManagement
{
    [Serializable]
    public class PlayerExpLevelUpData
    {
        public int expLevel;
        public int xpProgressRequired;
    }

    [Serializable]
    public class GameRankLevelUpData
    {
        public int gameRank;

        public int rankProgressRequired;

        // could be like soldier, captain, etc. or whatever rank naming scheme we chose
        public string rankName;
    }

    [CreateAssetMenu(fileName = "PlayerGameData", menuName = "Scriptable Objects/New Player Game Data Object",
        order = 0)]
    public class PlayerGameDataObject : ScriptableObject
    {
        public PlayerExpLevelUpData[] expLevelUp;
        public GameRankLevelUpData[] rankLevelUp;

        public int GetRequiredExpProgressForReachingNextLevel(int nextLevel)
        {
            foreach (var d in expLevelUp)
                if (d.expLevel == nextLevel)
                    return d.xpProgressRequired;

            // -1 if next level is not added which means max player Exp level reached reached and we can not give more exp levels,
            // unless a new update
            return -1;
        }

        public int GetRequiredRankProgressForGoingToNextRank(int nextRank)
        {
            foreach (var d in rankLevelUp)
                if (d.gameRank == nextRank)
                    return d.rankProgressRequired;

            // -1 if next rank is not added which means max game rank reached and we can not get higher rank.
            return -1;
        }

        public string GetRankName(int rank)
        {
            foreach (var d in rankLevelUp)
                if (d.gameRank == rank)
                    return d.rankName;

            // Return the name of first rank if rank invalid
            return rankLevelUp[0].rankName;
        }
    }
}