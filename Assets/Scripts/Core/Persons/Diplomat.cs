﻿using EuropeanWars.Core.Country;
using EuropeanWars.Core.Diplomacy;
using EuropeanWars.Core.Language;

namespace EuropeanWars.Core.Persons {
    public class Diplomat : Person {
        /// <summary>
        /// Tells how much has to be added to monthlyPointsIncreaseChance when diplomat is improving relations
        /// </summary>
        public float ImproveRelationsModifier { get; private set; }
        public CountryRelation CurrentlyImprovingRelation { get; private set; }
        public override string Speciality => LanguageDictionary.language["Diplomat"];
        public override string MoreInfo => LanguageDictionary.language["ImproveRelationsModifier"] + ": " + ImproveRelationsModifier;

        public Diplomat(string name, int birthYear, int deathYear, CountryInfo country, float improveRelationsModifier) : base(name, birthYear, deathYear, country) {
            ImproveRelationsModifier = improveRelationsModifier;
        }

        public void ImproveRelation(CountryRelation relation) {
            if (CurrentlyImprovingRelation != relation) {
                if (CurrentlyImprovingRelation != null) {
                    CurrentlyImprovingRelation.monthlyPointsIncreaseChance -= ImproveRelationsModifier;
                }

                Diplomat dip = country.GetDiplomatInRelation(relation);
                if (dip != null) {
                    dip.ImproveRelation(null);
                }

                CurrentlyImprovingRelation = relation;
                if (relation != null) {
                    CurrentlyImprovingRelation.monthlyPointsIncreaseChance += ImproveRelationsModifier;
                }
            }
        }
    }
}