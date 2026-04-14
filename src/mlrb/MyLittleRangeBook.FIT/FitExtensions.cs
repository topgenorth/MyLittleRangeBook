/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using Dynastream.Fit;
using FluentResults;
using NanoidDotNet;
using DateTime = Dynastream.Fit.DateTime;
using File = System.IO.File;

namespace MyLittleRangeBook.FIT
{
    static class FitExtensions
    {
        const double MetresToFeet = 3.2808399;
        const byte TimestampFieldId = 253;

        public static DateTimeOffset FitEpoch = new(1989, 12, 31, 0, 0, 0, TimeSpan.Zero);

        internal static string ToShotSessionId(this uint serialNumber)
        {
            return serialNumber == 0 ? Nanoid.Generate() : $"{serialNumber.ToString()}-0";
        }


        public static async Task<Result<ReadOnlyMemory<byte>>> LoadFitFileBytesAsync(this string filename,
            CancellationToken ct)
        {
            try
            {
                byte[] result = await File.ReadAllBytesAsync(filename, ct).ConfigureAwait(false);

                return Result.Ok<ReadOnlyMemory<byte>>(result);
            }
            catch (OperationCanceledException oce)
            {
                Error? err = new Error($"Failed to read file {filename}").CausedBy(oce);

                return Result.Fail<ReadOnlyMemory<byte>>(err);
            }
            catch (Exception e)
            {
                return Result.Fail<ReadOnlyMemory<byte>>(new Error($"Failed to read file {filename}").CausedBy(e));
            }
        }


        public static TimeSpan? TimezoneOffset(this ActivityMesg activity)
        {
            if (activity == null)
            {
                return null;
            }

            if (!activity.GetLocalTimestamp().HasValue)
            {
                return null;
            }

            return TimeSpan.FromSeconds(
                (int)activity.GetLocalTimestamp()! - (int)activity.GetTimestamp().GetTimeStamp());
        }

        public static DateTimeOffset LocalTimestampAsDateTimeOffset(this ActivityMesg activity)
        {
            return FitEpoch.AddTicks((activity.GetLocalTimestamp() ?? 0) * 10000000L);
        }

        public static DateTime LocalTimestampAsFitDateTime(this ActivityMesg activity)
        {
            return new DateTime(activity.GetLocalTimestamp() ?? 0);
        }

        public static DateTime? GetTimestamp(this Mesg mesg)
        {
            object? val = mesg.GetFieldValue(TimestampFieldId);
            if (val == null)
            {
                return null;
            }

            return mesg.TimestampToDateTime(Convert.ToUInt32(val));
        }

        public static DateTime? GetStartTime(this Mesg mesg)
        {
            object? val = mesg.GetFieldValue("StartTime");
            if (val == null)
            {
                return null;
            }

            return mesg.TimestampToDateTime(Convert.ToUInt32(val));
        }

        public static DateTime? GetEndTime(this Mesg mesg)
        {
            DateTime? startTime = mesg.GetStartTime();
            if (startTime == null)
            {
                return null;
            }

            object? val = mesg.GetFieldValue("TotalElapsedTime");
            if (val == null)
            {
                return null;
            }

            startTime.Add(Convert.ToUInt32(val));

            return startTime;
        }

        public static string? GetValueAsString(this Mesg mesg, string name)
        {
            Field? field = mesg.GetField(name, false);
            if (field == null)
            {
                return null;
            }

            var data = (byte[])field.GetValue();

            return data != null ? Encoding.UTF8.GetString(data, 0, data.Length - 1) : null;
        }

        public static bool Overlaps(this Mesg mesg, SessionMesg session)
        {
            if (mesg.GetStartTime() == null || mesg.GetEndTime() == null || session.GetStartTime() == null ||
                session.GetEndTime() == null)
            {
                return false;
            }

            return Math.Max(mesg.GetStartTime()!.GetTimeStamp(), session.GetStartTime().GetTimeStamp()) <=
                   Math.Min(mesg.GetEndTime()!.GetTimeStamp(), session.GetEndTime()!.GetTimeStamp());
        }

        public static bool? Within(this Mesg mesg, SessionMesg session)
        {
            if (mesg.GetTimestamp() == null || session.GetStartTime() == null || session.GetEndTime() == null)
            {
                return false;
            }

            return mesg.GetTimestamp()!.GetDateTime() >= session.GetStartTime().GetDateTime()
                   && mesg.GetTimestamp()!.GetDateTime() <= session.GetEndTime()!.GetDateTime();
        }

        public static int ToFps(this int metresPerSecond)
        {
            return (int)Math.Round(metresPerSecond * MetresToFeet);
        }

        public static double ToFps(this double metresPerSecond)
        {
            return metresPerSecond * MetresToFeet;
        }
    }
}
