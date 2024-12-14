// Licensed to the.NET Foundation under one or more agreements.
// The.NET Foundation licenses this file to you under the MIT license.

using net.opgenorth.xero.device;

namespace net.opgenorth.xero
{
    public partial class ShotSessionTemplate
    {
        readonly ShotSession _shotSession;

        public ShotSessionTemplate(ShotSession shotSession) => _shotSession = shotSession;
    }
}
