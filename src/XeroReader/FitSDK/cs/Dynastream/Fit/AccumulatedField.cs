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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynastream.Fit
{
    public class AccumulatedField
    {
        public int mesgNum;
        public int destFieldNum;
        private long lastValue;
        private long accumulatedValue;

        public AccumulatedField(int mesgNum, int destFieldNum)
        {
            this.mesgNum = mesgNum;
            this.destFieldNum = destFieldNum;
            this.lastValue = 0;
            this.accumulatedValue = 0;
        }

        public long Accumulate(long value, int bits)
        {
            long mask = (1L << bits) - 1;

            accumulatedValue += (value - lastValue) & mask;
            lastValue = value;

            return accumulatedValue;
        }

        public long Set(long value)
        {
            accumulatedValue = value;
            this.lastValue = value;
            return accumulatedValue;
        }
    }
}