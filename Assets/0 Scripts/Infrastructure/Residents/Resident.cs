﻿using Infrastructure.Grid.Entities.Buildings;
using Infrastructure.Tick;
using Settings;

namespace Infrastructure.Residents
{
    public class Resident : ITickable
    {
        #region Static Members
        public static System.Random rand;

        static Resident() {
            rand = new System.Random(System.DateTime.Now.Second);
        }

        public static string[] FirstNames = {
            "Jon", "Robert", "Damian", "Jabir", "Jacob", "Gordon", "Alyx", "Barney", "Eli", "Kliener"
        };

        public static string[] SecondNames =
        {
            "Smith", "Charles", "Jones", "Freeman", "Vance", "Breen"
        };

        private static string GetRandomFirstName()
        {
            return FirstNames[rand.Next(FirstNames.Length)];
        }

        private static string GetRandomSecondName()
        {
            return SecondNames[rand.Next(SecondNames.Length)];
        }
        #endregion

        public string FirstName { get; protected set; }
        public string SecondName { get; protected set; }
        public Residential Home;
        public Happiness Happiness { get; protected set; }
        public bool ShouldBeRemoved { get; set; }
        public bool Homeless = true;
        public Job Job { get; private set; }

        private Session sess;
        private float _timeWithLowHappiness = 0;
        private float _timeOffset;

        public Resident()
        {
            FirstName = GetRandomFirstName();
            SecondName = GetRandomSecondName();
            Happiness = new Happiness(this);
            sess = Game.CurrentSession;
            _timeOffset = (float)(new System.Random().NextDouble() * 10 - 5);
        }

        public Resident(ResidentData resData)
        {
            FirstName = resData.FirstName;
            SecondName = resData.SecondName;

            Happiness = new Happiness(this);
            sess = Game.CurrentSession;
            _timeOffset = (float)(new System.Random().NextDouble() * 10 - 5);
        }

        public void Tick(float time)
        {
            if(Job != null)
            {
                //Add funds to the player as we have a job.
                sess.AddFunds((uint)System.Math.Ceiling(Job.Salary.GetValue() / 0.2 * time));
            }
            if (Happiness.Level <= 0.2f)
            {
                _timeWithLowHappiness += time;
                if (_timeWithLowHappiness > GameSettings.ResidentTimeWithLowHappiness + _timeOffset)
                {
                    //The resident will move out.
                    MoveOut();
                }
            }
            else _timeWithLowHappiness = 0;
        }

        protected void MoveOut()
        {
            Home.RemoveResident(this);
        }

        public void SetJob(Job job)
        {
            if(job != null)
            {
                Job = job;
                Job.Taken = true;
                Job.Holder = this;
            }
        }

        public void RemoveJob()
        {
            Job = null;
        }
    }
}
