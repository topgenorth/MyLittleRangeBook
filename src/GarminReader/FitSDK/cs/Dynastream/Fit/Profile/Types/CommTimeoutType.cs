#region Copyright
/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2024 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////
// ****WARNING****  This file is auto-generated!  Do NOT edit this file.
// Profile Version = 21.141.0Release
// Tag = production/release/21.141.0-0-g2aa27e1
/////////////////////////////////////////////////////////////////////////////////////////////

#endregion

namespace Dynastream.Fit
{
    /// <summary>
    /// Implements the profile CommTimeoutType type as a class
    /// </summary>
    public static class CommTimeoutType 
    {
        public const ushort WildcardPairingTimeout = 0; // Timeout pairing to any device
        public const ushort PairingTimeout = 1; // Timeout pairing to previously paired device
        public const ushort ConnectionLost = 2; // Temporary loss of communications
        public const ushort ConnectionTimeout = 3; // Connection closed due to extended bad communications
        public const ushort Invalid = (ushort)0xFFFF;


    }
}

