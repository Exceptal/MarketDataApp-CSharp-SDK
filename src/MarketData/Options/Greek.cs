namespace MarketData.Options;

/// <summary>Greek identifier used to look up a specific greek value on an <see cref="OptionQuote"/>.</summary>
public enum Greek
{
    /// <summary>Delta — rate of change of option price vs underlying price.</summary>
    Delta,

    /// <summary>Gamma — rate of change of delta vs underlying price.</summary>
    Gamma,

    /// <summary>Theta — time decay of option value per day.</summary>
    Theta,

    /// <summary>Vega — sensitivity of option price to implied volatility.</summary>
    Vega,

    /// <summary>Rho — sensitivity of option price to interest rates.</summary>
    Rho,
}
