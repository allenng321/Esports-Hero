using System;
using Game.Scripts.Objects.Rooms;

namespace Game.Scripts.GameManagement
{
    [Serializable]
    public enum UpgradableName
    {
        BedroomLevel,
        BedroomBedLevel,
        BedroomPCLevel,
        BedroomTVLevel,
        BedroomFurnitureLevel,
        OfficeLevel,
        OfficeSofaLevel,
        OfficePCLevel,
        OfficeBookShelfLevel
    }

    [Serializable]
    public struct SerializableDateTime
    {
        public int second;
        public int minute;
        public int hour;

        public int day;
        public int month;
        public int year;

        public string kind;

        public static implicit operator DateTime(SerializableDateTime jdt) => new DateTime(year: jdt.year,
            month: jdt.month, day: jdt.day, hour: jdt.hour, minute: jdt.minute, second: jdt.second,
            kind: (DateTimeKind) Enum.Parse(typeof(DateTimeKind), jdt.kind));

        public static implicit operator SerializableDateTime(DateTime dt) => new SerializableDateTime
        {
            year = dt.Year,
            month = dt.Month,
            day = dt.Day,
            hour = dt.Hour,
            minute = dt.Minute,
            second = dt.Second,
            kind = dt.Kind.ToString()
        };

        public override string ToString()
        {
            var monthName = "Jan";

            switch (month)
            {
                case 1:
                    monthName = "Jan";
                    break;
                case 2:
                    monthName = "Feb";
                    break;
                case 3:
                    monthName = "Mar";
                    break;
                case 4:
                    monthName = "Apr";
                    break;
                case 5:
                    monthName = "May";
                    break;
                case 6:
                    monthName = "Jun";
                    break;
                case 7:
                    monthName = "Jul";
                    break;
                case 8:
                    monthName = "Aug";
                    break;
                case 9:
                    monthName = "Sep";
                    break;
                case 10:
                    monthName = "Oct";
                    break;
                case 11:
                    monthName = "Nov";
                    break;
                case 12:
                    monthName = "Dec";
                    break;
            }

            return $"{day} {monthName} {hour}:{minute}:{second}";
        }
    }

    [Serializable]
    public class UpgradeData
    {
        public RoomName room;
        public UpgradableName item;
        public int upgradedVersion;
        public SerializableDateTime startTime;
        public SerializableDateTime endTime;

        public override bool Equals(object obj)
        {
            // If the passed object is null
            if (obj == null)
            {
                return false;
            }

            if (!(obj is UpgradeData))
            {
                return false;
            }

            var o = (UpgradeData) obj;
            return room == o.room && item == o.item && upgradedVersion == o.upgradedVersion;
        }

        protected bool Equals(UpgradeData other)
        {
            return room == other.room && item == other.item && upgradedVersion == other.upgradedVersion;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) room;
                hashCode = (hashCode * 397) ^ (int) item;
                hashCode = (hashCode * 397) ^ upgradedVersion;
                return hashCode;
            }
        }
    }

    [Serializable]
    public struct RunningUpgrades
    {
        public UpgradeData[] upgrades;

        public RunningUpgrades(UpgradeData[] value)
        {
            upgrades = value;
        }
    }
}