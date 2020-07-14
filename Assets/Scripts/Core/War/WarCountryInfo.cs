﻿using EuropeanWars.Core.Country;
using EuropeanWars.Core.Province;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EuropeanWars.Core.War {
    public class WarCountryInfo {
        public readonly CountryInfo country;
        public readonly WarParty party;

        public readonly List<ProvinceInfo> enemyOccupatedProvinces = new List<ProvinceInfo>();
        public readonly List<ProvinceInfo> localOccupatedProvinces = new List<ProvinceInfo>();
        //TODO: Add battles

        public int killedEnemies;
        public int killedLocal;

        public bool IsMajor => party.major == this;

        public int CountryScoreCost => country.nationalProvinces.Sum(t => t.taxation);
        public int WarScore { get; private set; }
        public float PercentWarScore => (float)WarScore / (WarScore < 0 ? CountryScoreCost : party.Enemies.PartyScoreCost);
        public Color PercentWarScoreColor => PercentWarScore == 0 ? Color.yellow : (PercentWarScore > 0 ? Color.green : Color.red);

        public WarCountryInfo(CountryInfo country, WarParty party) {
            this.country = country;
            this.party = party;
        }

        /// <summary>
        /// Automatically invokes AddLocalOccupatedProvince in occupated Country.
        /// </summary>
        /// <param name="province"></param>
        public void AddEnemyOccupatedProvince(ProvinceInfo province) {
            if (!enemyOccupatedProvinces.Contains(province)
                && province.Country == country && party.Enemies.ContainsCountry(province.NationalCountry)) {
                enemyOccupatedProvinces.Add(province);
                WarScore += province.taxation;
                party.Enemies.countries[province.NationalCountry].AddLocalOccupatedProvince(province);
            }
        }

        /// <summary>
        /// Automatically invokes RemoveLocalOccupatedProvince in occupated Country.
        /// </summary>
        /// <param name="province"></param>
        public void RemoveEnemyOccupatedProvince(ProvinceInfo province) {
            if (enemyOccupatedProvinces.Contains(province) && province.Country == province.NationalCountry) {
                enemyOccupatedProvinces.Remove(province);
                WarScore -= province.taxation;
                party.Enemies.countries[province.NationalCountry].RemoveLocalOccupatedProvince(province);
            }
        }

        /// <summary>
        /// Doesn't invoke AddEnemyOccupatedProvince in occupant.
        /// </summary>
        /// <param name="province"></param>
        public void AddLocalOccupatedProvince(ProvinceInfo province) {
            if (!localOccupatedProvinces.Contains(province) 
                && province.NationalCountry == country && party.Enemies.ContainsCountry(province.Country)) {
                localOccupatedProvinces.Add(province);
                WarScore -= province.taxation;
            }
        }

        /// <summary>
        /// Doesn't invoke RemoveEnemyOccupatedProvince in occupant.
        /// </summary>
        /// <param name="province"></param>
        public void RemoveLocalOccupatedProvince(ProvinceInfo province) {
            if (localOccupatedProvinces.Contains(province) && province.Country == province.NationalCountry && province.Country == country) {
                localOccupatedProvinces.Remove(province);
                WarScore += province.taxation;
            }
        }
    }
}
