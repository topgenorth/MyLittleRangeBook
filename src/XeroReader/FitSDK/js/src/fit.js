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


const BaseType = {
    ENUM: 0x00,
    SINT8: 0x01,
    UINT8: 0x02,
    SINT16: 0x83,
    UINT16: 0x84,
    SINT32: 0x85,
    UINT32: 0x86,
    STRING: 0x07,
    FLOAT32: 0x88,
    FLOAT64: 0x89,
    UINT8Z: 0x0A,
    UINT16Z: 0x8B,
    UINT32Z: 0x8C,
    BYTE: 0x0D,
    SINT64: 0x8E,
    UINT64: 0x8F,
    UINT64Z: 0x90
};

const BaseTypeDefinitions = {
    0x00: { size: 1, type: BaseType.ENUM, invalid: 0xFF },
    0x01: { size: 1, type: BaseType.SINT8, invalid: 0x7F },
    0x02: { size: 1, type: BaseType.UINT8, invalid: 0xFF },
    0x83: { size: 2, type: BaseType.SINT16, invalid: 0x7FFF },
    0x84: { size: 2, type: BaseType.UINT16, invalid: 0xFFFF },
    0x85: { size: 4, type: BaseType.SINT32, invalid: 0x7FFFFFFF },
    0x86: { size: 4, type: BaseType.UINT32, invalid: 0xFFFFFFFF },
    0x07: { size: 1, type: BaseType.STRING, invalid: 0x00 },
    0x88: { size: 4, type: BaseType.FLOAT32, invalid: 0xFFFFFFFF },
    0x89: { size: 8, type: BaseType.FLOAT64, invalid: 0xFFFFFFFFFFFFFFFF },
    0x0A: { size: 1, type: BaseType.UINT8Z, invalid: 0x00 },
    0x8B: { size: 2, type: BaseType.UINT16Z, invalid: 0x0000 },
    0x8C: { size: 4, type: BaseType.UINT32Z, invalid: 0x00000000 },
    0x0D: { size: 1, type: BaseType.BYTE, invalid: 0xFF },
    0x8E: { size: 8, type: BaseType.SINT64, invalid: 0x7FFFFFFFFFFFFFFF },
    0x8F: { size: 8, type: BaseType.UINT64, invalid: 0xFFFFFFFFFFFFFFFF },
    0x90: { size: 8, type: BaseType.UINT64Z, invalid: 0x0000000000000000 },
};

const NumericFieldTypes = [
    "sint8",
    "uint8",
    "sint16",
    "uint16",
    "sint32",
    "uint32",
    "float32",
    "float64",
    "uint8z",
    "uint16z",
    "uint32z",
    "byte",
    "sint64",
    "uint64",
    "uint64z"
];

const FieldTypeToBaseType = {
    "sint8": BaseType.SINT8,
    "uint8": BaseType.UINT8,
    "sint16": BaseType.SINT16,
    "uint16": BaseType.UINT16,
    "sint32": BaseType.SINT32,
    "uint32": BaseType.UINT32,
    "string": BaseType.STRING,
    "float32": BaseType.FLOAT32,
    "float64": BaseType.FLOAT64,
    "uint8z": BaseType.UINT8Z,
    "uint16z": BaseType.UINT16Z,
    "uint32z": BaseType.UINT32Z,
    "byte": BaseType.BYTE,
    "sint64": BaseType.SINT64,
    "uint64": BaseType.UINT64,
    "uint64z": BaseType.UINT64Z
};

export default {
    BaseType,
    BaseTypeDefinitions,
    NumericFieldTypes,
    FieldTypeToBaseType
};