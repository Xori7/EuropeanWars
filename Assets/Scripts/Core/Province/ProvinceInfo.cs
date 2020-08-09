﻿using EuropeanWars.Core.Army;
using EuropeanWars.Core.Building;
using EuropeanWars.Core.Country;
using EuropeanWars.Core.Culture;
using EuropeanWars.Core.Data;
using EuropeanWars.Core.Language;
using EuropeanWars.Core.Religion;
using EuropeanWars.Core.Time;
using EuropeanWars.GameMap;
using EuropeanWars.Province;
using EuropeanWars.UI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EuropeanWars.Core.Province {
    public class ProvinceInfo {
        private readonly ProvinceData data;

        public readonly int id;
        public readonly string color;
        public bool isLand;

        public float x, y;
        public MapProvince mapProvince;

        public string name;

        public int taxation;
        public int buildingsIncome;
        public int tradeIncome;
        public bool isInteractive;
        public bool isActive;

        public bool fogOfWar;

        public List<ProvinceInfo> neighbours = new List<ProvinceInfo>();
        public CountryInfo Country { get; private set; }
        public CountryInfo NationalCountry { get; private set; }
        public List<CountryInfo> claimators = new List<CountryInfo>();

        public ReligionInfo religion;
        public int[] religionFollowers;
        public CultureInfo culture;
        public int defense;
        public Dictionary<UnitInfo, int> garnison = new Dictionary<UnitInfo, int>();
        public bool isTradeCity;
        public bool isTradeRoute;
        public BuildingInfo[] buildings = new BuildingInfo[10];

        public List<ArmyInfo> armies = new List<ArmyInfo>();
        public ProvinceOccupationCounter OccupationCounter { get; private set; }

        public ProvinceInfo(ProvinceData d) {
            this.data = d;
            id = d.id;
            color = d.color;
            x = d.x;
            y = d.y;
            isLand = d.isLand;
            taxation = d.taxation;
            isInteractive = d.isInteractive;
            isActive = d.isActive;
            religionFollowers = d.religionFollowers;
            defense = d.defense;
            isTradeCity = d.isTradeCity;
            isTradeRoute = d.isTradeRoute;

            GameInfo.provincesByColor.Add(color, this);
        }

        public void Initialize() {
            SetCountry(GameInfo.countries[data.country], true);

            //TODO: Uncomment this.
            //if (data.neighbours != null) {
            //    neighbours = new List<ProvinceInfo>();
            //    for (int i = 0; i < data.neighbours.Length; i++) {
            //        neighbours.Add(GameInfo.provinces[data.neighbours[i]]);
            //    }
            //}
            garnison.Add(GameInfo.units[0], taxation * 100); //TODO: Change it to garnison = data.garnison; or something

            for (int i = 0; i < 10; i++) {
                buildings[i] = GameInfo.buildings[data.buildings[i]];
            }
            religion = GameInfo.religions[data.religion];
            culture = GameInfo.cultures[data.culture];
            OccupationCounter = new ProvinceOccupationCounter(this);
            UpdateLanguage();

            TimeManager.onDayElapsed += OnDayElapsed;
            TimeManager.onMonthElapsed += OnMonthElapsed;
        }

        public void UpdateLanguage() {
            if (LanguageDictionary.language.ContainsKey("ProvinceName-" + color)) {
                name = LanguageDictionary.language["ProvinceName-" + color];
            }
        }

        public void OnDayElapsed() {
            OccupationCounter.UpdateProgress();
        }

        public void OnMonthElapsed() {
            UpdateGarnisonSize();
        }

        public void SetCountry(CountryInfo country, bool nationalCountry = false) {
            if (Country != null) {
                Country.provinces.Remove(this);
                if (nationalCountry) {
                    Country.nationalProvinces.Remove(this);
                }

                foreach (var item in new List<UnitToRecruit>(Country.toRecruit)) {
                    if (item.province == this) {
                        Country.gold += item.unitInfo.recruitCost * item.count;
                        Country.manpower += item.unitInfo.recruitSize * item.count;
                        Country.toRecruit.Remove(item);
                    }
                }
            }
            Country = country;
            Country.provinces.Add(this);
            if (nationalCountry) {
                NationalCountry = country;
                Country.nationalProvinces.Add(this);
                FabricateClaim(Country);
            }
            if (mapProvince != null && isLand) {
                mapProvince.material.color = country.color;
                mapProvince.UpdateBorders();
                MapPainter.PaintProvince(this);
                if (GameInfo.gameStarted) {
                    RefreshFogOfWar();
                }
            }
        }

        public void UpdateGarnisonSize() {
            if (garnison[GameInfo.units[0]] < taxation * 100 && OccupationCounter.Army == null) {
                garnison[GameInfo.units[0]] = Mathf.Clamp(garnison[GameInfo.units[0]] + taxation * 10, 0, taxation * 100);
                if (ProvinceWindow.Singleton.province == this) {
                    ProvinceWindow.Singleton.UpdateWindow(this);
                }
            }
        }

        public void FabricateClaim(CountryInfo country) {
            if (isInteractive && !claimators.Contains(country)) {
                claimators.Add(country);
                country.claimedProvinces.Add(this);
            }
        }

        public void BuildBuilding(BuildingInfo building, int slot) {
            if (buildings.Contains(building) && building.id != 0) {
                return;
            }

            taxation -= buildings[slot].incomeModifier;
            defense -= buildings[slot].defenceModifier;

            Country.gold -= building.cost;
            buildings[slot] = building;

            buildingsIncome += buildings[slot].incomeModifier;
            defense += buildings[slot].defenceModifier;


            if (ProvinceWindow.Singleton.province == this) {
                ProvinceWindow.Singleton.UpdateWindow(this);
            }
        }

        public void UpgradeProvince() {
            if (Country.gold > 50) {
                Country.gold -= 50;
                taxation += 1;
                if (ProvinceWindow.Singleton.province == this) {
                    ProvinceWindow.Singleton.UpdateWindow(this);
                }
            }
        }

        public void DevastateProvince() {
            if (taxation > 0) {
                Country.gold += 30;
                taxation -= 1;
                if (ProvinceWindow.Singleton.province == this) {
                    ProvinceWindow.Singleton.UpdateWindow(this);
                }
            }
        }

        #region Fow
        public void RefreshFogOfWarInRegion() {
            RefreshFogOfWar();
            foreach (var item in neighbours) {
                item.RefreshFogOfWar();
            }
        }

        public void RefreshFogOfWar() {
            foreach (var item in neighbours) {
                if (item.Country == GameInfo.PlayerCountry) {
                    SetFogOfWar(false);
                    return;
                }
            }
            SetFogOfWar(IsFow());
        }

        public bool IsFow() {
            return !(Country == GameInfo.PlayerCountry
                || GameInfo.PlayerCountry.alliances.ContainsKey(Country)
                || armies.Where(t => t.Country == GameInfo.PlayerCountry 
                || GameInfo.PlayerCountry.alliances.ContainsKey(t.Country)).Any());
        }

        public void SetFogOfWarInRegion(bool b) {
            SetFogOfWar(b);
            foreach (var item in neighbours) {
                item.SetFogOfWar(b);
            }
        }

        public void SetFogOfWar(bool b) {
            fogOfWar = false; //b;
            if (mapProvince && MapPainter.mapMode == MapMode.Countries) {
                mapProvince.material.SetFloat("_FogOfWar", b ? 1 : 0);
            }
        }
        #endregion

        public void MergeArmiesRequest(ArmyInfo[] armies) {
            if (armies.Length > 1) {
                for (int i = 1; i < armies.Length; i++) {
                    foreach (var unit in armies[i].units) {
                        armies[i].MoveUnitToOtherArmyRequest(unit.Key, armies[0], armies[i].maxUnits[unit.Key]);
                    }
                }
            }
        }

        public void MergeArmies(ArmyInfo[] armies) {
            if (armies.Length > 1) {
                for (int i = 1; i < armies.Length; i++) {
                    foreach (var unit in new Dictionary<UnitInfo, int>(armies[i].units)) {
                        armies[i].MoveUnitToOtherArmy(unit.Key, armies[0], armies[i].maxUnits[unit.Key]);
                    }
                }
            }
        }
    }
}
