/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using Dynastream.Fit;
using FluentResults;
using DateTime = Dynastream.Fit.DateTime;

namespace MyLittleRangeBook.Cli
{
    public static class FitExtensions
    {
        const double MetresToFeet = 3.2808399;
        const byte TimestampFieldId = 253;
        public static System.DateTime FitEpoch = new(1989, 12, 31, 0, 0, 0, DateTimeKind.Utc);


        public static async Task<Result<ReadOnlyMemory<byte>>> LoadBytesAsync(this string filename, CancellationToken ct)
        {
            try
            {
                byte[] result = await System.IO.File.ReadAllBytesAsync(filename, ct).ConfigureAwait(false);
                return Result.Ok<ReadOnlyMemory<byte>>(result);
            }
            catch (OperationCanceledException oce)
            {
                var err = new Error($"Failed to read file {filename}").CausedBy(oce);
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

        public static System.DateTime LocalTimestampAsSystemDateTime(this ActivityMesg activity)
        {
            return new System.DateTime((activity.GetLocalTimestamp() ?? 0) * 10000000L + FitEpoch.Ticks,
                DateTimeKind.Local);
        }

        public static DateTime LocalTimestampAsFitDateTime(this ActivityMesg activity)
        {
            return new DateTime(activity.GetLocalTimestamp() ?? 0);
        }

        public static DateTime? GetTimestamp(this Mesg mesg)
        {
            var val = mesg.GetFieldValue(TimestampFieldId);
            if (val == null)
            {
                return null;
            }

            return mesg.TimestampToDateTime(Convert.ToUInt32(val));
        }

        public static DateTime? GetStartTime(this Mesg mesg)
        {
            var val = mesg.GetFieldValue("StartTime");
            if (val == null)
            {
                return null;
            }

            return mesg.TimestampToDateTime(Convert.ToUInt32(val));
        }

        public static DateTime? GetEndTime(this Mesg mesg)
        {
            var startTime = mesg.GetStartTime();
            if (startTime == null)
            {
                return null;
            }

            var val = mesg.GetFieldValue("TotalElapsedTime");
            if (val == null)
            {
                return null;
            }

            startTime.Add(Convert.ToUInt32(val));

            return startTime;
        }

        public static string? GetValueAsString(this Mesg mesg, string name)
        {
            var field = mesg.GetField(name, false);
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

        public static int ToFps(this int metresPerSecond) => (int)Math.Round(metresPerSecond*MetresToFeet);
        public static double ToFps(this double metresPerSecond) => metresPerSecond*MetresToFeet;
    }
}
