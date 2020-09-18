﻿using EuropeanWars.Core.Province;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EuropeanWars.Core.Army {
    public class ArmyObject : MonoBehaviour {
        public ArmyInfo army;
        public LineRenderer lineRenderer;

        public GameObject gui;
        public Image selectionOutline;
        public Image crest;
        public Text size;
        public Text artillerySize;
        public Image sizeBackground;
        public GameObject blackStatusImage;
        public GameObject occupationProgress;
        public Image occupationProgressBar;
        public Text occupationProgressText;

        public float scale;

        public bool isMovementCoroutineExecuting;

        public void Initialize(ArmyInfo army) {
            this.army = army;
            crest.sprite = army.Country.crest;
            transform.position = new Vector3(army.Province.x, army.Province.y, 0);
        }

        public void Update() {
            if (GameInfo.gameStarted) {
                size.text = $"{Math.Round(army.Size * 0.001f, 1)}k";
                artillerySize.text = army.Artilleries.ToString();
                UpdateScale();
                UpdateColor();
                blackStatusImage.SetActive(army.BlackStatus);

                if (army.Province.OccupationCounter?.Army == army) {
                    occupationProgress.SetActive(true);
                    occupationProgressBar.fillAmount = army.Province.OccupationCounter.Progress / 100;
                    occupationProgressText.text = Mathf.RoundToInt(army.Province.OccupationCounter.Progress) + "%";
                }
                else {
                    occupationProgress.SetActive(false);
                }
            }
        }

        public void OnClick() {
            if (army.IsSelected) {
                army.UnselectArmy();
            }
            else {
                army.SelectArmy(!Input.GetKey(KeyCode.LeftShift));
            }
        }

        public void UpdateScale() {
            float curDistance = Controller.Singleton.playerCam.orthographicSize;

            if (curDistance > Controller.Singleton.armiesDistance || army.Province.fogOfWar) {
                gui.SetActive(false);
                lineRenderer.enabled = false;
            }
            else {
                gui.SetActive(true);
                lineRenderer.enabled = true;
            }
            transform.localScale = new Vector3(1, 1, 1) * scale * curDistance / Controller.Singleton.minScope;
        }

        public void UpdateColor() {
            Color color = Color.gray;

            if (army.Country == GameInfo.PlayerCountry) {
                color = Color.green;
            }
            else if (GameInfo.PlayerCountry.IsInWarAgainstCountry(army.Country)) {
                color = Color.red;
            }
            else if (GameInfo.PlayerCountry.IsInWarWithCountry(army.Country)) {
                color = Color.blue;
            }
            else if (GameInfo.PlayerCountry.alliances.ContainsKey(army.Country)) {
                color = Color.cyan;
            }

            sizeBackground.color = color;
        }

        public void DrawRoute(ProvinceInfo[] route) {
            try { 
                lineRenderer.positionCount = route.Length;
                lineRenderer.SetPosition(0, transform.position);

                for (int i = 0; i < route.Length; i++) {
                    ProvinceInfo item = route[i];
                    lineRenderer.SetPosition(i, new Vector2(item.x, item.y));
                }
            }
            catch {

            }
        }
    }
}
