using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beyond
{
    public enum Season : int { Winter, Spring, Summer, Autumn }

    public class Gametime
    {
        public static List<int> AllowedSpeeds = new List<int> { 0, 1, 2, 3, 4, 5 };
        private float timer = 0f;
        private int speed = 2;
        private int year = 0;
        private int month = 1;
        private int day = 1;
        private int hour = 0;
        private int minute = 0;
        private int dayOfTheWeek = 0;
        private static readonly Dictionary<int, float> SpeedFactor = new Dictionary<int, float>
        {
            {0,0f},
            {1,1/60f}, //1 real time
            {2,1f}, //1 second is 1 minute
            {3,10f}, //1 second is 10 minute
            {4,60f}, //1 second is 1 hour 
            {5,1440f}, //1 second is 1 day
        };

        private static readonly List<int> monthWith30Days = new List<int>() { 2, 4, 6, 9, 11 };
        private static readonly Dictionary<int, string> Weekdays = new Dictionary<int, string>
        {
            {0,"Monday"},
            {1,"Tuesday" },
            {2,"Wednesday" },
            {3,"Thursday" },
            {4,"Friday" },
            {5,"Saturday" },
            {6,"Sunday"}
        };
        private static readonly Dictionary<int, string> WeekdaysShort = new Dictionary<int, string>
        {
            {0,"Mon"},
            {1,"Tue" },
            {2,"Wed" },
            {3,"Thu" },
            {4,"Fri" },
            {5,"Sat" },
            {6,"Sun"}
        };
        private static readonly Dictionary<int, string> Months = new Dictionary<int, string>
        {
            {1,"January" },
            {2,"February" },
            {3,"March" },
            {4,"April" },
            {5,"May" },
            {6,"Jun"},
            {7,"July"},
            {8,"August"},
            {9,"September"},
            {10,"October"},
            {11,"November"},
            {12,"December"}
        };
        private static readonly Dictionary<int, string> MonthsShort = new Dictionary<int, string>
        {
            {1,"Jan" },
            {2,"Feb" },
            {3,"Mar" },
            {4,"Apr" },
            {5,"May" },
            {6,"Jun"},
            {7,"Jul"},
            {8,"Aug"},
            {9,"Sep"},
            {10,"Oct"},
            {11,"Nov"},
            {12,"Dec"}
        };

        public static int DaysInMonth(int month)
        { // Triple binary operator for the win
            return ((month < 1 | month > 12) ? 0 : ((month == 2) ? 28 : ((monthWith30Days.Contains(month)) ? 30 : 31)));
        }

        public static string MonthStr(int month)
        {
            switch (month)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                default: return "December";
            }
        }

        public static string MonthShortStr(int month)
        {
            switch (month)
            {
                case 1: return "Jan";
                case 2: return "Feb";
                case 3: return "Mar";
                case 4: return "Apr";
                case 5: return "May";
                case 6: return "Jun";
                case 7: return "Jul";
                case 8: return "Aug";
                case 9: return "Sep";
                case 10: return "Oct";
                case 11: return "Nov";
                default: return "Dec";
            }
        }
        public Gametime()
        {
            year = 0;
            month = 1;
            day = 1;
            hour = 0;
            minute = 0;
            dayOfTheWeek = 0; // Do we really want to always start on a Monday ?
        }

        public bool setSpeed(int i)
        {
            if (AllowedSpeeds.Contains(i))
            {
                speed = i;
                return true;
            }
            else
            {
                Debug.Log(String.Format("Invalid speed {0}.", i));
            }
            return false;
        }


        /// <summary>
        /// Adds time  based on the current speed. Will never add more than 1 day
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (speed == 0)
            {
                return;
            }
            timer += deltaTime;
            if (timer > 1 / SpeedFactor[speed])
            {
                //Debug.Log("Adding "+ Mathf.RoundToInt(SpeedFactor[speed] * timer) + " minutes");
                minute += Mathf.RoundToInt(SpeedFactor[speed] * timer);
                timer = 0;
                if (minute >= 60)
                {
                    int hoursToAdd = minute / 60;
                    minute = minute % 60;
                    hour += hoursToAdd;
                    if (hour >= 24)
                    {
                        hour = hour % 24;
                        if (++dayOfTheWeek == 7)
                        {
                            dayOfTheWeek = 0;
                        }
                        if (day++ == DaysInMonth(month))
                        {
                            day = 1;
                            if (++month == 13)
                            {
                                month = 1;
                                year++;
                            }
                        }
                    }
                }
            }
        }

        public int DayInYear()
        {
            int days = 0;
            for (int i = 0; i < month; i++)
            {
                days += DaysInMonth(i);
            }
            return days + day;
        }

        public string TimeStr()
        {
            return String.Format("{0:00}:{1:00}", hour, minute);
        }

        public string DateStr()
        {
            return "Year " + year + ", " + Weekdays[dayOfTheWeek] + " " + Months[month] + " " + day + " " + TimeStr();
        }

        public string DateShortStr()
        {
            return "Year " + year + ", " + WeekdaysShort[dayOfTheWeek] + " " + MonthsShort[month] + " " + day + " " + TimeStr();
        }

        public Season GetSeason(Hemisphere h)
        {
            int diy = DayInYear();
            if (diy >= 80 && diy < 169)
            {
                return (h == Hemisphere.North ? Season.Spring : Season.Autumn);
            }
            if (diy >= 169 && diy < 262)
            {
                return (h == Hemisphere.North ? Season.Summer : Season.Winter);
            }
            if (diy >= 262 && diy < 351)
            {
                return (h == Hemisphere.North ? Season.Autumn : Season.Spring);
            }
            return (h == Hemisphere.North ? Season.Winter : Season.Summer);
        }

    }
}
