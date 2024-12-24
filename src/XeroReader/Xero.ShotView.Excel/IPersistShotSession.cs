// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using net.opgenorth.xero.shotview;

namespace net.opgenorth.xero.data.sqlite
{
    public interface IPersistShotSession
    {
        Task<int> DeleteSession(WorkbookSession session);
        Task UpsertSession(WorkbookSession session);
    }
}
