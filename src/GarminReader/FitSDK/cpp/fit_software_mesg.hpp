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


#if !defined(FIT_SOFTWARE_MESG_HPP)
#define FIT_SOFTWARE_MESG_HPP

#include "fit_mesg.hpp"

namespace fit
{

class SoftwareMesg : public Mesg
{
public:
    class FieldDefNum final
    {
    public:
       static const FIT_UINT8 MessageIndex = 254;
       static const FIT_UINT8 Version = 3;
       static const FIT_UINT8 PartNumber = 5;
       static const FIT_UINT8 Invalid = FIT_FIELD_NUM_INVALID;
    };

    SoftwareMesg(void) : Mesg(Profile::MESG_SOFTWARE)
    {
    }

    SoftwareMesg(const Mesg &mesg) : Mesg(mesg)
    {
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of message_index field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsMessageIndexValid() const
    {
        const Field* field = GetField(254);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns message_index field
    ///////////////////////////////////////////////////////////////////////
    FIT_MESSAGE_INDEX GetMessageIndex(void) const
    {
        return GetFieldUINT16Value(254, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set message_index field
    ///////////////////////////////////////////////////////////////////////
    void SetMessageIndex(FIT_MESSAGE_INDEX messageIndex)
    {
        SetFieldUINT16Value(254, messageIndex, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of version field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsVersionValid() const
    {
        const Field* field = GetField(3);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns version field
    ///////////////////////////////////////////////////////////////////////
    FIT_FLOAT32 GetVersion(void) const
    {
        return GetFieldFLOAT32Value(3, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set version field
    ///////////////////////////////////////////////////////////////////////
    void SetVersion(FIT_FLOAT32 version)
    {
        SetFieldFLOAT32Value(3, version, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Checks the validity of part_number field
    // Returns FIT_TRUE if field is valid
    ///////////////////////////////////////////////////////////////////////
    FIT_BOOL IsPartNumberValid() const
    {
        const Field* field = GetField(5);
        if( FIT_NULL == field )
        {
            return FIT_FALSE;
        }

        return field->IsValueValid();
    }

    ///////////////////////////////////////////////////////////////////////
    // Returns part_number field
    ///////////////////////////////////////////////////////////////////////
    FIT_WSTRING GetPartNumber(void) const
    {
        return GetFieldSTRINGValue(5, 0, FIT_SUBFIELD_INDEX_MAIN_FIELD);
    }

    ///////////////////////////////////////////////////////////////////////
    // Set part_number field
    ///////////////////////////////////////////////////////////////////////
    void SetPartNumber(FIT_WSTRING partNumber)
    {
        SetFieldSTRINGValue(5, partNumber, 0);
    }

};

} // namespace fit

#endif // !defined(FIT_SOFTWARE_MESG_HPP)
