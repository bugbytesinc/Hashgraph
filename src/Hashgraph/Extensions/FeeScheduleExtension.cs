#pragma warning disable CS8604
using System;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

/// <summary>
/// Extends the client functionality to include retrieving the
/// Exchange Rate directly from the network.
/// </summary>
public static class FeeScheduleExtension
{
    /// <summary>
    /// Well known address of the fee schedule file.
    /// </summary>
    public static readonly Address FEE_SCHEDULE_FILE_ADDRESS = new Address(0, 0, 111);
    /// <summary>
    /// Retrieves the metrics for calculating fees from the network.
    /// network.
    /// </summary>
    /// <remarks>
    /// NOTE: this method incours a charge to retrieve the file from the network.
    /// </remarks>
    /// <param name="client">Client Object</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The structure representing the metrics of the Network's Fee Schedule.
    /// </returns>
    public static async Task<FeeSchedules> GetFeeScheduleAsync(this Client client, Action<IContext>? configure = null)
    {
        var file = await client.GetFileContentAsync(FEE_SCHEDULE_FILE_ADDRESS, configure).ConfigureAwait(false);
        var set = Proto.CurrentAndNextFeeSchedule.Parser.ParseFrom(file.ToArray());
        return new FeeSchedules(set.CurrentFeeSchedule?.ToFeeSchedule(), set.NextFeeSchedule?.ToFeeSchedule());
    }
}