// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace net.opgenorth.xero.shotview;

public interface IPersistShotSession
{
    Task<int> DeleteSession(WorkbookSession session);
    Task UpsertSession(WorkbookSession session);
}
