namespace MyLittleRangeBook.Model

open System

module RangeLogTypes = 
    type MeasuredValue =
        | Velocity of value: float * units: string
        | Weight of value: int * units: string
        | CBTO of length: float * units: string
        | COAL of length: float * units: string

        member t.DisplayFieldName =
            match t with
            | Velocity _ -> "Velocity"
            | Weight _ -> "Weight"
            | CBTO _ -> "CBTO"
            | COAL _ -> "COAL"

    type RangeTrip =
        { Id: string
          TripDate: DateTime
          RoundsFired: int
          Ammo: string
          RangeName: string
          Notes: string
          Firearm: string }

    type PowderCharge =
        { Id: string
          PowderName: string
          Weight: float
          Units: string }

    type LoadData =
        { Id: string
          Charge: PowderCharge
          Length: MeasuredValue
          Brass: string }

    [<Struct>]
    type ShotData =
        { Id: string
          MuzzleVelocity: float
          Unit: string
          Omit: bool }
