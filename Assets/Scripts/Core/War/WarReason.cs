﻿using EuropeanWars.Core.Country;
using System.Reflection;

namespace EuropeanWars.Core.War {
    public abstract class WarReason {
        public abstract string Name { get; }
        public abstract bool ProvinceTakingEnabled { get; }

        public abstract bool CanInviteCountryToWar(CountryInfo invitator, CountryInfo friend);
    }
}