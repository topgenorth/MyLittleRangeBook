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
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;

namespace Dynastream.Fit
{
    /// <summary>
    /// Implements the FieldDescription profile message.
    /// </summary>
    public class FieldDescriptionMesg : Mesg
    {
        #region Fields
        #endregion

        /// <summary>
        /// Field Numbers for <see cref="FieldDescriptionMesg"/>
        /// </summary>
        public sealed class FieldDefNum
        {
            public const byte DeveloperDataIndex = 0;
            public const byte FieldDefinitionNumber = 1;
            public const byte FitBaseTypeId = 2;
            public const byte FieldName = 3;
            public const byte Array = 4;
            public const byte Components = 5;
            public const byte Scale = 6;
            public const byte Offset = 7;
            public const byte Units = 8;
            public const byte Bits = 9;
            public const byte Accumulate = 10;
            public const byte FitBaseUnitId = 13;
            public const byte NativeMesgNum = 14;
            public const byte NativeFieldNum = 15;
            public const byte Invalid = Fit.FieldNumInvalid;
        }

        #region Constructors
        public FieldDescriptionMesg() : base(Profile.GetMesg(MesgNum.FieldDescription))
        {
        }

        public FieldDescriptionMesg(Mesg mesg) : base(mesg)
        {
        }
        #endregion // Constructors

        #region Methods
        ///<summary>
        /// Retrieves the DeveloperDataIndex field</summary>
        /// <returns>Returns nullable byte representing the DeveloperDataIndex field</returns>
        public byte? GetDeveloperDataIndex()
        {
            Object val = GetFieldValue(0, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set DeveloperDataIndex field</summary>
        /// <param name="developerDataIndex_">Nullable field value to be set</param>
        public void SetDeveloperDataIndex(byte? developerDataIndex_)
        {
            SetFieldValue(0, 0, developerDataIndex_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the FieldDefinitionNumber field</summary>
        /// <returns>Returns nullable byte representing the FieldDefinitionNumber field</returns>
        public byte? GetFieldDefinitionNumber()
        {
            Object val = GetFieldValue(1, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set FieldDefinitionNumber field</summary>
        /// <param name="fieldDefinitionNumber_">Nullable field value to be set</param>
        public void SetFieldDefinitionNumber(byte? fieldDefinitionNumber_)
        {
            SetFieldValue(1, 0, fieldDefinitionNumber_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the FitBaseTypeId field</summary>
        /// <returns>Returns nullable byte representing the FitBaseTypeId field</returns>
        public byte? GetFitBaseTypeId()
        {
            Object val = GetFieldValue(2, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set FitBaseTypeId field</summary>
        /// <param name="fitBaseTypeId_">Nullable field value to be set</param>
        public void SetFitBaseTypeId(byte? fitBaseTypeId_)
        {
            SetFieldValue(2, 0, fitBaseTypeId_, Fit.SubfieldIndexMainField);
        }
        
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field FieldName</returns>
        public int GetNumFieldName()
        {
            return GetNumFieldValues(3, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the FieldName field</summary>
        /// <param name="index">0 based index of FieldName element to retrieve</param>
        /// <returns>Returns byte[] representing the FieldName field</returns>
        public byte[] GetFieldName(int index)
        {
            byte[] data = (byte[])GetFieldValue(3, index, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the FieldName field</summary>
        /// <param name="index">0 based index of FieldName element to retrieve</param>
        /// <returns>Returns String representing the FieldName field</returns>
        public String GetFieldNameAsString(int index)
        {
            byte[] data = (byte[])GetFieldValue(3, index, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set FieldName field</summary>
        /// <param name="index">0 based index of FieldName element to retrieve</param>
        /// <param name="fieldName_"> field value to be set</param>
        public void SetFieldName(int index, String fieldName_)
        {
            byte[] data = Encoding.UTF8.GetBytes(fieldName_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(3, index, zdata, Fit.SubfieldIndexMainField);
        }

        
        /// <summary>
        /// Set FieldName field</summary>
        /// <param name="index">0 based index of field_name</param>
        /// <param name="fieldName_">field value to be set</param>
        public void SetFieldName(int index, byte[] fieldName_)
        {
            SetFieldValue(3, index, fieldName_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Array field</summary>
        /// <returns>Returns nullable byte representing the Array field</returns>
        public byte? GetArray()
        {
            Object val = GetFieldValue(4, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set Array field</summary>
        /// <param name="array_">Nullable field value to be set</param>
        public void SetArray(byte? array_)
        {
            SetFieldValue(4, 0, array_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Components field</summary>
        /// <returns>Returns byte[] representing the Components field</returns>
        public byte[] GetComponents()
        {
            byte[] data = (byte[])GetFieldValue(5, 0, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the Components field</summary>
        /// <returns>Returns String representing the Components field</returns>
        public String GetComponentsAsString()
        {
            byte[] data = (byte[])GetFieldValue(5, 0, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set Components field</summary>
        /// <param name="components_"> field value to be set</param>
        public void SetComponents(String components_)
        {
            byte[] data = Encoding.UTF8.GetBytes(components_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(5, 0, zdata, Fit.SubfieldIndexMainField);
        }

        
        /// <summary>
        /// Set Components field</summary>
        /// <param name="components_">field value to be set</param>
        public void SetComponents(byte[] components_)
        {
            SetFieldValue(5, 0, components_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Scale field</summary>
        /// <returns>Returns nullable byte representing the Scale field</returns>
        public byte? GetScale()
        {
            Object val = GetFieldValue(6, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set Scale field</summary>
        /// <param name="scale_">Nullable field value to be set</param>
        public void SetScale(byte? scale_)
        {
            SetFieldValue(6, 0, scale_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Offset field</summary>
        /// <returns>Returns nullable sbyte representing the Offset field</returns>
        public sbyte? GetOffset()
        {
            Object val = GetFieldValue(7, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToSByte(val));
            
        }

        /// <summary>
        /// Set Offset field</summary>
        /// <param name="offset_">Nullable field value to be set</param>
        public void SetOffset(sbyte? offset_)
        {
            SetFieldValue(7, 0, offset_, Fit.SubfieldIndexMainField);
        }
        
        
        /// <summary>
        ///
        /// </summary>
        /// <returns>returns number of elements in field Units</returns>
        public int GetNumUnits()
        {
            return GetNumFieldValues(8, Fit.SubfieldIndexMainField);
        }

        ///<summary>
        /// Retrieves the Units field</summary>
        /// <param name="index">0 based index of Units element to retrieve</param>
        /// <returns>Returns byte[] representing the Units field</returns>
        public byte[] GetUnits(int index)
        {
            byte[] data = (byte[])GetFieldValue(8, index, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the Units field</summary>
        /// <param name="index">0 based index of Units element to retrieve</param>
        /// <returns>Returns String representing the Units field</returns>
        public String GetUnitsAsString(int index)
        {
            byte[] data = (byte[])GetFieldValue(8, index, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set Units field</summary>
        /// <param name="index">0 based index of Units element to retrieve</param>
        /// <param name="units_"> field value to be set</param>
        public void SetUnits(int index, String units_)
        {
            byte[] data = Encoding.UTF8.GetBytes(units_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(8, index, zdata, Fit.SubfieldIndexMainField);
        }

        
        /// <summary>
        /// Set Units field</summary>
        /// <param name="index">0 based index of units</param>
        /// <param name="units_">field value to be set</param>
        public void SetUnits(int index, byte[] units_)
        {
            SetFieldValue(8, index, units_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Bits field</summary>
        /// <returns>Returns byte[] representing the Bits field</returns>
        public byte[] GetBits()
        {
            byte[] data = (byte[])GetFieldValue(9, 0, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the Bits field</summary>
        /// <returns>Returns String representing the Bits field</returns>
        public String GetBitsAsString()
        {
            byte[] data = (byte[])GetFieldValue(9, 0, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set Bits field</summary>
        /// <param name="bits_"> field value to be set</param>
        public void SetBits(String bits_)
        {
            byte[] data = Encoding.UTF8.GetBytes(bits_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(9, 0, zdata, Fit.SubfieldIndexMainField);
        }

        
        /// <summary>
        /// Set Bits field</summary>
        /// <param name="bits_">field value to be set</param>
        public void SetBits(byte[] bits_)
        {
            SetFieldValue(9, 0, bits_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the Accumulate field</summary>
        /// <returns>Returns byte[] representing the Accumulate field</returns>
        public byte[] GetAccumulate()
        {
            byte[] data = (byte[])GetFieldValue(10, 0, Fit.SubfieldIndexMainField);
            return data.Take(data.Length - 1).ToArray();
        }

        ///<summary>
        /// Retrieves the Accumulate field</summary>
        /// <returns>Returns String representing the Accumulate field</returns>
        public String GetAccumulateAsString()
        {
            byte[] data = (byte[])GetFieldValue(10, 0, Fit.SubfieldIndexMainField);
            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        ///<summary>
        /// Set Accumulate field</summary>
        /// <param name="accumulate_"> field value to be set</param>
        public void SetAccumulate(String accumulate_)
        {
            byte[] data = Encoding.UTF8.GetBytes(accumulate_);
            byte[] zdata = new byte[data.Length + 1];
            data.CopyTo(zdata, 0);
            SetFieldValue(10, 0, zdata, Fit.SubfieldIndexMainField);
        }

        
        /// <summary>
        /// Set Accumulate field</summary>
        /// <param name="accumulate_">field value to be set</param>
        public void SetAccumulate(byte[] accumulate_)
        {
            SetFieldValue(10, 0, accumulate_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the FitBaseUnitId field</summary>
        /// <returns>Returns nullable ushort representing the FitBaseUnitId field</returns>
        public ushort? GetFitBaseUnitId()
        {
            Object val = GetFieldValue(13, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));
            
        }

        /// <summary>
        /// Set FitBaseUnitId field</summary>
        /// <param name="fitBaseUnitId_">Nullable field value to be set</param>
        public void SetFitBaseUnitId(ushort? fitBaseUnitId_)
        {
            SetFieldValue(13, 0, fitBaseUnitId_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the NativeMesgNum field</summary>
        /// <returns>Returns nullable ushort representing the NativeMesgNum field</returns>
        public ushort? GetNativeMesgNum()
        {
            Object val = GetFieldValue(14, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToUInt16(val));
            
        }

        /// <summary>
        /// Set NativeMesgNum field</summary>
        /// <param name="nativeMesgNum_">Nullable field value to be set</param>
        public void SetNativeMesgNum(ushort? nativeMesgNum_)
        {
            SetFieldValue(14, 0, nativeMesgNum_, Fit.SubfieldIndexMainField);
        }
        
        ///<summary>
        /// Retrieves the NativeFieldNum field</summary>
        /// <returns>Returns nullable byte representing the NativeFieldNum field</returns>
        public byte? GetNativeFieldNum()
        {
            Object val = GetFieldValue(15, 0, Fit.SubfieldIndexMainField);
            if(val == null)
            {
                return null;
            }

            return (Convert.ToByte(val));
            
        }

        /// <summary>
        /// Set NativeFieldNum field</summary>
        /// <param name="nativeFieldNum_">Nullable field value to be set</param>
        public void SetNativeFieldNum(byte? nativeFieldNum_)
        {
            SetFieldValue(15, 0, nativeFieldNum_, Fit.SubfieldIndexMainField);
        }
        
        #endregion // Methods
    } // Class
} // namespace
