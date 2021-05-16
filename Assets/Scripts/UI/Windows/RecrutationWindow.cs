﻿using EuropeanWars.Core;
using EuropeanWars.Core.Army;
using EuropeanWars.Core.Language;
using EuropeanWars.GameMap;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EuropeanWars.UI.Windows {
    public class RecrutationWindow : MonoBehaviour {
        public List<UnitButton> units = new List<UnitButton>();
        public Transform unitsContent;
        public UnitButton unitButtonPrefab;

        [Header("UnitInfo")]
        public UnitInfo selectedUnit;
        public Text unitName;
        public Image unitImage;
        public Text attack;
        public Text health;
        public Text speed;
        public Text type;
        public Text maintenance;
        public Text foodPerMonth;

        public Slider recruitSizeSlider;
        public Text recruitSizeText;
        public Text recruitCostText;

        [Header("RecruitingUnits")]
        public Transform recruitingUnitsListContent;
        public RecruitingUnitProgressWindow recruitingUnitProgressWindowPrefab;

        public void OnDisable() {
            if (MapPainter.mapMode == MapMode.Recrutation) {
                MapPainter.PaintMap(MapMode.Countries);
            }
        }

        public void Start() {
            foreach (var item in GameInfo.PlayerCountry.units) {
                UnitButton b = Instantiate(unitButtonPrefab, unitsContent);
                b.SetUnit(item);
                units.Add(b);
            }
            units.FirstOrDefault().OnClick();
        }

        public void Update() {
            if (selectedUnit != null) {
                int recruitSize = selectedUnit.type == UnitType.Navy ? 1 : selectedUnit.recruitSize;
                recruitSizeText.text = (recruitSizeSlider.value * recruitSize).ToString();
                recruitCostText.text = (recruitSizeSlider.value * selectedUnit.recruitCost).ToString();
                recruitSizeSlider.minValue = 0;
                recruitSizeSlider.maxValue = Mathf.Clamp(Mathf.Min(GameInfo.PlayerCountry.gold / selectedUnit.recruitCost,
                    GameInfo.PlayerCountry.manpower / recruitSize), 0, int.MaxValue);
            }
        }

        public void SelectUnit(UnitInfo unit) {
            if (unit != null) {
                selectedUnit = unit;
                unitName.text = unit.name;
                unitImage.sprite = unit.image;
                type.text = LanguageDictionary.language[unit.type.ToString()];
                attack.text = unit.attack.ToString();
                health.text = unit.health.ToString();
                speed.text = unit.speed.ToString();
                maintenance.text = unit.maintenance.ToString();
                foodPerMonth.text = unit.foodPerMonth.ToString();
            }
        }

        public void ShowRecrutationMap() {
            if (selectedUnit != null) {
                MapPainter.PaintMap(MapMode.Recrutation);
            }
        }

        public void AddRecruitingUnit(UnitToRecruit unit) {
            RecruitingUnitProgressWindow window = Instantiate(recruitingUnitProgressWindowPrefab, recruitingUnitsListContent);
            window.SetUnit(unit);
        }
    }
}

