using Proto;

namespace Hashgraph;

/// <summary>
/// The definition of a Royalty computed from the amount of 
/// Fungible token exchanged, can be in the form as a deduction 
/// of the token echanged, or an exise amount taken from the 
/// sender of the fungible token.
/// </summary>
public sealed record TokenRoyalty : IRoyalty
{
    /// <summary>
    /// Identifies this royalty as a Token Royalty type.
    /// </summary>
    public RoyaltyType RoyaltyType => RoyaltyType.Token;
    /// <summary>
    /// Account receiving the royalty assessment.
    /// </summary>
    public AddressOrAlias Account { get; private init; }
    /// <summary>
    /// The minimum assessed value, in terms of the 
    /// smallest denomination of the associated token.
    /// </summary>
    public long Minimum { get; private init; }
    /// <summary>
    /// The numerator portion of the fraction of the 
    /// transferred units to assess.
    /// </summary>
    /// <remarks>
    /// This is not expressed as a floating point number
    /// in order to avoid rounding fees inheret in 
    /// computing platforms.
    /// </remarks>
    public long Numerator { get; private init; }
    /// <summary>
    /// The denominator portion of the fraction of the 
    /// transferred units to assess.
    /// </summary>
    /// <remarks>
    /// This is not expressed as a floating point number
    /// in order to avoid rounding fees inheret in 
    /// computing platforms.
    /// </remarks>
    public long Denominator { get; private init; }
    /// <summary>
    /// The maximum allowed fee value, in terms of
    /// the smallest denomination of the associated token.
    /// </summary>
    public long Maximum { get; private init; }
    /// <summary>
    /// Determines how the royalty assessment is applied, 
    /// if <code>true</code> the amount is added as an extra 
    /// surcharge paid by the sender of the associated token.
    /// If <code>false</code> (the default) the amount of token 
    /// received by the receiving account is reduced by the 
    /// assement computed from the total amount of the associated
    /// fungible token sent by the sender.
    /// </summary>
    public bool AssessAsSurcharge { get; private init; }
    /// <summary>
    /// Internal Constructor representing the "None" version of this royalty.
    /// </summary>
    private TokenRoyalty()
    {
        Account = Address.None;
        Numerator = 0;
        Denominator = 0;
        Minimum = 0;
        Maximum = 0;
        AssessAsSurcharge = false;
    }
    /// <summary>
    /// Public Constructor, an <code>TokenRoyalty</code> is immutable after creation.
    /// </summary>
    /// <param name="account">
    /// Account receiving the royalty assessment.
    /// </param>
    /// <param name="numerator">
    /// The numerator portion of the fraction of the 
    /// transferred units to assess.
    /// </param>
    /// <param name="denominator">
    /// The denominator portion of the fraction of the 
    /// transferred units to assess.
    /// </param>
    /// <param name="minimum">
    /// The minimum assessed value, in terms of the 
    /// smallest denomination of the associated token.
    /// </param>
    /// <param name="maximum">
    /// The maximum allowed fee value, in terms of
    /// the smallest denomination of the associated token.
    /// </param>
    /// <param name="assesAsSurcharge">
    /// Determines how the royalty assessment is applied, 
    /// if <code>true</code> the amount is added as an extra 
    /// surcharge paid by the sender of the associated token.
    /// If <code>false</code> (the default) the amount of token 
    /// received by the receiving account is reduced by the 
    /// assement computed from the total amount of the associated
    /// fungible token sent by the sender.
    /// </param>
    public TokenRoyalty(AddressOrAlias account, long numerator, long denominator, long minimum, long maximum, bool assesAsSurcharge = false)
    {
        Account = account;
        Numerator = numerator;
        Denominator = denominator;
        Minimum = minimum;
        Maximum = maximum;
        AssessAsSurcharge = assesAsSurcharge;
    }
    /// <summary>
    /// Internal Helper Constructor converting raw protobuf 
    /// into this royalty definition.
    /// </summary>
    internal TokenRoyalty(CustomFee fee)
    {
        Account = fee.FeeCollectorAccountId.AsAddress();
        var fraction = fee.FractionalFee;
        Numerator = fraction.FractionalAmount.Numerator;
        Denominator = fraction.FractionalAmount.Denominator;
        Minimum = fraction.MinimumAmount;
        Maximum = fraction.MaximumAmount;
        AssessAsSurcharge = fraction.NetOfTransfers;
    }
}