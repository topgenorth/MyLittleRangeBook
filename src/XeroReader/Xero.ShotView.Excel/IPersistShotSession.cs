// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace net.opgenorth.xero.shotview
{
    public interface IPersistShotSession
    {
        /// <summary>
        ///     Delete the session
        /// </summary>
        /// <param name="session"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> DeleteSession(WorkbookSession session, CancellationToken ct);

        Task<int> UpsertSession(WorkbookSession session, CancellationToken ct);
    }
}
