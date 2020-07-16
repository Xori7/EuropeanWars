﻿using EuropeanWars.Core.Province;
using UnityEngine;

namespace EuropeanWars.Core.War {
    public class ProvincePeaceDealElement : PeaceDealElement {
        public override string Name => $"<color=#004c8f>{province.NationalCountry.name}</color> oddaje prowincję <color=#e3c800>{province.name}</color> dla <color=#004c8f>{province.Country.name}</color>"; //TODO: Make this translating when translate system is done.
        public override int WarScoreCost => Mathf.FloorToInt(province.taxation * costModifier);
        public override Color Color => Color.yellow;

        public readonly ProvinceInfo province;
        public readonly float costModifier;

        public ProvincePeaceDealElement(ProvinceInfo province, PeaceDeal peaceDeal) : base(peaceDeal, province.NationalCountry, province.Country) {
            this.province = province;
            costModifier = province.claimators.Contains(province.Country) ? 0.5f : 1.0f;
        }

        public override bool CanBeUsed(PeaceDeal peaceDeal) {
            return peaceDeal.war.warReason.ProvinceTakingEnabled;
        }

        public override bool IsSame(PeaceDealElement element) {
            ProvincePeaceDealElement e = (ProvincePeaceDealElement)element;
            return base.IsSame(element) && province == e.province && costModifier == e.costModifier;
        }

        public override void Execute() {
            if (province != null) {
                province.SetCountry(to, true);
            }
        }
    }
}
