﻿namespace Infrastructure.Residents
{
    public class Happiness
    {
        #region Static Members
        public static float BaseHappiness { get { return 0.19f; } }
        public static float BonusHappiness { get; private set; }
        private static float InitialBonusValue = 0.4f;
        private static int BonusProviders = 0;

        static Happiness()
        {
            BonusHappiness = 0;
        }

        /// <summary>
        /// Should electricity be evaluated for happiness?
        /// </summary>
        public static bool EvaluateElectricity {
            get {
                return evaluateElecticity;
            }

            set {
                _evalIsDirty = true;
                evaluateElecticity = value;
            }
        }
        /// <summary>
        /// Should water be evaluated for happiness?
        /// </summary>
        public static bool EvaluateWater {
            get {
                return evaluateWater;
            }

            set {
                _evalIsDirty = true;
                evaluateWater = value;
            }
        }
        /// <summary>
        /// Should base happiness be added for happiness?
        /// </summary>
        public static bool EnableBaseHappiness {
            get {
                return enableBaseHappiness;
            }

            set {
                _evalIsDirty = true;
                enableBaseHappiness = value;
            }
        }

        public static bool EnableJob {
            get {
                return enableJob;
            }

            set {
                _evalIsDirty = true;
                enableJob = value;
            }
        }

        /// <summary>
        /// How many evaluation variables exist. Cached.
        /// </summary>
        private static int EvaluationQuantity {
            get {
                //This value gets updated only when its dirty to avoid needless repeat processing.
                if (_evalIsDirty)
                {
                    _evalIsDirty = false;
                    int value = 0;
                    if (EvaluateElectricity) value++;
                    if (EvaluateWater) value++;
                    if (EnableBaseHappiness) value++;
                    if (EnableJob) value++;
                    _evalQuantity = value;
                }
                return _evalQuantity;
            }
        }
        private static bool _evalIsDirty = true;
        private static int _evalQuantity = 0;

        private static bool evaluateElecticity = false;
        private static bool evaluateWater = false;
        private static bool enableBaseHappiness = true;
        private static bool enableJob = false;

        public static void IncreaseBonusHappiness()
        {
            BonusProviders++;
            BonusHappiness += InitialBonusValue * BonusProviders;
        }

        public static void ReduceBonusHappiness()
        {
            BonusHappiness -= InitialBonusValue * BonusProviders;
            BonusProviders--;
            if (BonusHappiness < 0) BonusHappiness = 0;
        }
        #endregion

        public float Level {
            get { return GetLevel(); }
        }

        private Resident resident;


        public Happiness(Resident res)
        {
            resident = res;
        }

        private float GetLevel()
        {
            float value = 0;
            if (EvaluateElectricity)
            {
                if (resident.Home.HasPower) value += (float)1 / EvaluationQuantity;
            }
            if (EvaluateWater)
            {
                if (resident.Home.HasWaterSupply) value += (float)1 / EvaluationQuantity;
            }
            if (EnableBaseHappiness)
            {
                value += BaseHappiness;
            }
            return value + BonusHappiness;
        }
    }
}
