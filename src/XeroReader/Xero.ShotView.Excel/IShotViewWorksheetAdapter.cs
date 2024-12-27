// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace net.opgenorth.xero.shotview
{
    public interface IShotViewWorksheetAdapter
    {
        void Dispose();
        string ToString();
        void WriteMetadataToWorksheet(WorkbookSession session);
        WorkbookSession? GetShotSession(int sheetNumber);
        IEnumerable<WorkbookSession> GetAllSessions(CancellationToken ct);
    }
}
