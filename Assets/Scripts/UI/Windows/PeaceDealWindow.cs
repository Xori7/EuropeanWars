﻿using EuropeanWars.Core.Country;
using EuropeanWars.Core.War;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EuropeanWars.UI.Windows {
    public class PeaceDealWindow : MonoBehaviour {
        public static PeaceDealWindow Singleton { get; private set; }

        public DynamicPeaceDeal peaceDeal;

        public GameObject windowObject;

        public Image receiverCrest;
        public Text warScore;
        public Text usedWarScore;

        public Text gold;

        public PeaceDealElementButton peaceDealElementButtonPrefab;

        public Transform senderElementsContent;
        public Transform receiverElementsContent;

        public Dictionary<PeaceDealElement, PeaceDealElementButton> senderElements = new Dictionary<PeaceDealElement, PeaceDealElementButton>();
        public Dictionary<PeaceDealElement, PeaceDealElementButton> receiverElements = new Dictionary<PeaceDealElement, PeaceDealElementButton>();

        public void Awake() {
            Singleton = this;
        }

        public void CreatePeaceDeal(WarInfo war, WarCountryInfo sender, WarCountryInfo receiver) {
            ClearElements();
            peaceDeal = new DynamicPeaceDeal(war, sender, receiver);
            windowObject.SetActive(true);
            receiverCrest.sprite = receiver.country.crest;
            InitElements();
        }

        public void Update() {
            if (peaceDeal != null) {
                warScore.text = peaceDeal.SenderWarScore.ToString();
                warScore.color = peaceDeal.sender.PercentWarScoreColor;
                usedWarScore.text = peaceDeal.UsedWarScore.ToString();
                usedWarScore.color = peaceDeal.UsedWarScore <= peaceDeal.sender.PercentWarScore ? Color.yellow : Color.red;
                gold.text = peaceDeal.GainedGold.ToString();
            }
        }

        public void AddSenderElement(PeaceDealElement element) {
            PeaceDealElementButton b = Instantiate(peaceDealElementButtonPrefab, senderElementsContent);
            b.SetElement(element, true);
            senderElements.Add(element, b);
        }

        public void RemoveSenderElement(PeaceDealElement element) {
            Destroy(senderElements[element].gameObject);
            senderElements.Remove(element);
        }

        public void AddReceiverElement(PeaceDealElement element) {
            PeaceDealElementButton b = Instantiate(peaceDealElementButtonPrefab, senderElementsContent);
            b.SetElement(element, false);
            senderElements.Add(element, b);
        }

        public void RemoveReceiverElement(PeaceDealElement element) {
            Destroy(receiverElements[element].gameObject);
            receiverElements.Remove(element);
        }
        
        private void ClearElements() {
            foreach (var item in senderElements) {
                Destroy(item.Value);
            }
            foreach (var item in receiverElements) {
                Destroy(item.Value);
            }

            senderElements.Clear();
            receiverElements.Clear();
        }

        private void InitElements() {
            foreach (var item in peaceDeal.senderElements) {
                AddSenderElement(item.Value);
            }
            foreach (var item in peaceDeal.receiverElements) {
                AddReceiverElement(item.Value);
            }
        }
    }
}
