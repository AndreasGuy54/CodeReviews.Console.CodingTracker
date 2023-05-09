﻿using System.Configuration;

namespace CodeTracker.csm_stough
{
    internal class GoalManager
    {
        public static List<CodingGoal> currentGoals
        {
            get
            {
                return Database.GetAllGoals(where: "date('now') <= End AND date('now') >= Start");
            }
        }
        public static List<CodingGoal> upcomingGoals
        {
            get
            {
                return Database.GetAllGoals(where: "date('now') < Start");
            }
        }
        public static List<CodingGoal> pastGoals
        {
            get
            {
                return Database.GetAllGoals(where: "date('now') > End");
            }
        }
        private static string dateFormat = ConfigurationManager.AppSettings.Get("dateFormat");

        public static void DisplayCurrentGoals()
        {
            Console.WriteLine("Current Goals ~~~~~~~~~~~~~~~~~~~~~~");
            if(currentGoals.Count == 0)
            {
                Console.WriteLine("No Current Goals...");
            }
            currentGoals.ForEach(goal => { DisplayGoal(goal); });
        }

        public static void UpdateGoals()
        {
            currentGoals.ForEach((goal) => {
                List<CodingSession> records = Database.GetAll(where: $"Start BETWEEN '{goal.Start.ToString(dateFormat)}' AND '{goal.End.ToString(dateFormat)}'");
                goal.CurrentHours = TimeSpan.FromSeconds(records.Sum(record => record.duration.TotalSeconds));
                Database.UpdateGoal(goal);
            });
        }

        public static void DisplayGoal(CodingGoal goal)
        {
            Console.WriteLine($"\nStart Date: {goal.Start}, End Date: {goal.End}, Goal: {goal.TargetHours.ToString("hh\\:mm")} hours, Progress: {GetProgressString(goal)}");
            if (DateTime.Now >= goal.Start && DateTime.Now <= goal.End)
            {
                if((goal.End - DateTime.Now).TotalHours < (goal.TargetHours - goal.CurrentHours).TotalHours)
                {
                    Console.WriteLine("Not enough time left to complete");
                }
                else
                {
                    Console.WriteLine($"Hours per day needed to complete: {((goal.TargetHours - goal.CurrentHours).TotalHours / MathF.Max((int)(goal.End - DateTime.Now).TotalDays, 1))} hours/day");
                }
            }
        }

        private static string GetProgressString(CodingGoal goal)
        {
            int width = 20;
            string progress = "";
            float percentage = (float)(goal.CurrentHours.TotalSeconds / (goal.TargetHours.TotalSeconds + float.Epsilon));
            for(int i = 0; i < width; i++)
            {
                if(i < percentage * width)
                {
                    progress += "#";
                }
                else
                {
                    progress += "-";
                }
            }
            return $"[{progress}] : {(percentage * 100f).ToString("0.00")}%";
        }
    }
}
