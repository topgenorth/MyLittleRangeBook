// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

namespace net.opgenorth.xero.ShotView.Excel;

public partial class ShotViewExcelSpreadsheetTemplate
{
    private readonly WorkbookSession _shotSession;

    public ShotViewExcelSpreadsheetTemplate(WorkbookSession shotSession, string appVersion)
    {
        _shotSession = shotSession;
        AppVersion = appVersion;
    }

    public string AppVersion { get; }
}
