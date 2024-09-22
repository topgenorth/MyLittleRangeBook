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


#if !defined(FIT_ACCUMULATOR_HPP)
#define FIT_ACCUMULATOR_HPP

#include <vector>
#include "fit_accumulated_field.hpp"

namespace fit
{

class Accumulator
{
   public:
      FIT_UINT32 Accumulate(const FIT_UINT16 mesgNum, const FIT_UINT8 destFieldNum, const FIT_UINT32 value, const FIT_UINT8 bits);
      void Set(const FIT_UINT16 mesgNum, const FIT_UINT8 destFieldNum, const FIT_UINT32 value );

   private:
      std::vector<AccumulatedField> fields;
};

} // namespace fit

#endif // defined(FIT_ACCUMULATOR_HPP)

