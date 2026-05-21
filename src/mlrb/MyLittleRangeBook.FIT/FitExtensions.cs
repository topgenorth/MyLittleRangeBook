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
using File = System.IO.File;

namespace MyLittleRangeBook.FIT
{
    public static class FitExtensions
    {
        const double MetresToFeet = 3.2808399;
        const byte TimestampFieldId = 253;

        public static DateTimeOffset FitEpoch = new(1989, 12, 31, 0, 0, 0, TimeSpan.Zero);


        public static DateTimeOffset GetTimestampUtc(this ChronoShotSessionMesg msg)
        {
            return msg.GetTimestamp().GetDateTime().ToUniversalTime();
        }

        public static DateTimeOffset GetTimestampUtc(this ChronoShotDataMesg msg)
        {
            return msg.GetTimestamp().GetDateTime().ToUniversalTime();
        }

        public static DateTimeOffset GetTimeCreatedUtc(this FileIdMesg msg)
        {
            return msg.GetTimeCreated().GetDateTime().ToUniversalTime();
        }

        public static DateTimeOffset GetTimestampUtc(this DeviceInfoMesg msg)
        {
            return msg.GetTimestamp().GetDateTime().ToUniversalTime();
        }

        /// <summary>
        ///     Converts a FIT DateTime to a .NET DateTimeOffset. Returns null if the input is null.
        /// </summary>
        /// <param name="fitDateTime"></param>
        /// <returns></returns>
        public static DateTimeOffset? ToDateTimeOffset(this DateTime? fitDateTime)
        {
            if (fitDateTime == null)
            {
                return null;
            }

            return new DateTimeOffset(fitDateTime.GetDateTime().ToUniversalTime(), TimeSpan.Zero);
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

        public static async Task<Result<ReadOnlyMemory<byte>>> LoadFileBytesAsync(this string fileName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return Result.Fail("Filename cannot be null or whitespace.");
            }

            try
            {
                byte[] x = await File.ReadAllBytesAsync(fileName, ct).ConfigureAwait(false);
                ReadOnlyMemory<byte> y = x;

                return Result.Ok(y);
            }
            catch (Exception ex)
            {
                Error? err = new Error(ex.Message).CausedBy(ex).WithMetadata("filename", fileName);

                return Result.Fail(err);
            }
        }
    }
}
