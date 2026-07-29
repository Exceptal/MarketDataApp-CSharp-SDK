namespace MarketData.Options;

/// <summary>The side (call or put) of an option contract.</summary>
public enum OptionSide
{
    /// <summary>Call option.</summary>
    Call,

    /// <summary>Put option.</summary>
    Put,
}

internal static class OptionSideExtensions
{
    internal static string ToWireValue(this OptionSide side) => side switch
    {
        OptionSide.Call => "call",
        OptionSide.Put => "put",
        _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
    };
}
