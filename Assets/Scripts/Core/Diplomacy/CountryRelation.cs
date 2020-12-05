﻿using EuropeanWars.Core.Country;
using EuropeanWars.Core.Language;
using EuropeanWars.Core.Time;
using EuropeanWars.Network;
using EuropeanWars.UI.Windows;
using Lidgren.Network;
using System;
using UnityEngine;

namespace EuropeanWars.Core.Diplomacy {
    public enum DiplomaticRelation {
        Alliance = 0,
        RoyalMariage = 1,
        MilitaryAccess = 2,
        TradeAgreament = 3,
    }

    public enum DiplomaticAction {
        Insult = 0,
        Vassalization = 1
    }

    public class CountryRelation {
        public int Points { get; private set; }
        public bool[] relations;
        public int truceInMonths;
        public int monthsToNextAction;

        public bool withPlayerCountry;

        public CountryRelation(int points) {
            Points = Mathf.Clamp(points, -100, 100);
            this.relations = new bool[Enum.GetValues(typeof(DiplomaticRelation)).Length];
            truceInMonths = 0;
            TimeManager.onMonthElapsed += OnMonthElapsed;
        }

        public void OnMonthElapsed() {
            if (truceInMonths > 0) {
                truceInMonths--;
            }
            if (monthsToNextAction > 0) {
                monthsToNextAction--;
            }
        }

        public bool CanChangeRelationStateTo(DiplomaticRelation relation, bool targetState) => relations[(int)relation] != targetState && monthsToNextAction == 0;

        public void ChangeRelationState(DiplomaticRelation relation, CountryInfo sender, CountryInfo receiver) {
            ChangeRelationState((int)relation, sender, receiver);
        }
        public void ChangeRelationState(int relation, CountryInfo sender, CountryInfo receiver) {
            //TODO: Add switch and additional actions in this place
            relations[relation] = !relations[relation];
            if (DiplomacyWindow.Singleton.window.activeInHierarchy) {
                DiplomacyWindow.Singleton.UpdateWindow();
            }

            if (withPlayerCountry) {
                foreach (var item in GameInfo.provinces) {
                    item.Value.RefreshFogOfWar();
                }

                DipRequestWindow window = DiplomacyWindow.Singleton.SpawnRequest(sender, receiver, true);
                window.acceptText.text = "Ok";
                window.deliceText.transform.parent.gameObject.SetActive(false);
                window.title.text = LanguageDictionary.language[Enum.GetName(typeof(DiplomaticRelation), relation)];
                window.description.text = string.Format(
                    LanguageDictionary.language["DiplomaticRelationChanged"],
                    window.title.text, LanguageDictionary.language[relations[relation] ? "HasBeenCreated" : "HasBeenDeleted"],
                    receiver.name);
            }
        }

        public void ChangePoints(int change) {
            Points = Mathf.Clamp(Points + change, -100, 100);
        }

        public void TryChangeRelationState(DiplomaticRelation relation, CountryInfo sender, CountryInfo receiver) {
            if (monthsToNextAction > 0) {
                return;
            }

            if (relations[(int)relation]) {
                if (!sender.isPlayer) {
                    ChangeRelationState(relation, sender, receiver);
                }
                else {
                    SendMessage(relation, sender, receiver, 1039);
                }

                monthsToNextAction += 12;
            }
            else if (!sender.IsInWarAgainstCountry(receiver)) {
                if (!sender.isPlayer && !receiver.isPlayer) {
                    if (GameInfo.countryAIs[receiver].IsDiplomaticRelationChangeAccepted(relation, sender)) {
                        ChangeRelationState(relation, sender, receiver);
                    }
                }
                else if (sender.isPlayer && !receiver.isPlayer) {
                    if (GameInfo.countryAIs[receiver].IsDiplomaticRelationChangeAccepted(relation, sender)) {
                        SendMessage(relation, sender, receiver, 1039);
                    }
                    else {
                        DipRequestWindow window = DiplomacyWindow.Singleton.SpawnRequest(sender, receiver, true);
                        window.acceptText.text = "Ok";
                        window.deliceText.transform.parent.gameObject.SetActive(false);
                        window.title.text = LanguageDictionary.language[Enum.GetName(typeof(DiplomaticRelation), relation)];
                        window.description.text = string.Format(
                            LanguageDictionary.language["DiplomaticRelationDeliced"], receiver.name, window.title.text);
                    }
                }
                else if (!sender.isPlayer && receiver.isPlayer) {
                    if (GameInfo.PlayerCountry == receiver) {
                        ShowRequest(relation, sender, receiver);
                    }
                }
                else if (GameInfo.PlayerCountry == sender) {
                    SendMessage(relation, sender, receiver, 1040);
                }

                monthsToNextAction += 12;
            }
        }
        public void ProcessRequest(DiplomaticRelation relation, CountryInfo sender, CountryInfo receiver) {
            if (GameInfo.PlayerCountry == receiver) {
                ShowRequest(relation, sender, receiver);
            }
        }
        private void ShowRequest(DiplomaticRelation relation, CountryInfo sender, CountryInfo receiver) {
            DipRequestWindow window = DiplomacyWindow.Singleton.SpawnRequest(sender, receiver, true);
            window.acceptText.text = LanguageDictionary.language["Accept"];
            window.deliceText.text = LanguageDictionary.language["Delice"];
            window.title.text = LanguageDictionary.language[relation.ToString()];
            window.description.text = string.Format(
                LanguageDictionary.language["DiplomaticRequestDescripton"], sender.name, window.title.text);

            NetOutgoingMessage acceptMessage = Client.Singleton.c.CreateMessage();
            acceptMessage.Write((ushort)1039);
            acceptMessage.Write(sender.id);
            acceptMessage.Write(receiver.id);
            acceptMessage.Write((int)relation);
            window.acceptMessage = acceptMessage;

            NetOutgoingMessage deliceMessage = Client.Singleton.c.CreateMessage();
            deliceMessage.Write((ushort)1041);
            deliceMessage.Write(sender.id);
            deliceMessage.Write(receiver.id);
            deliceMessage.Write((int)relation);
            window.deliceMessage = deliceMessage;
        }
        private void SendMessage(DiplomaticRelation relation, CountryInfo sender, CountryInfo receiver, ushort id) {
            NetOutgoingMessage msg = Client.Singleton.c.CreateMessage();
            msg.Write(id);
            msg.Write(sender.id);
            msg.Write(receiver.id);
            msg.Write((int)relation);
            Client.Singleton.c.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
        }
    }
}
