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

        /// <summary>
        ///     Generate a unique ID for a shot session based on the Xero serial number and a random Nanoid. If the serial number
        ///     is less than 1, only the Nanoid will be used as the ID.
        /// </summary>
        /// <remarks>The Nanoid is enclosed in square brackets to distinguish it as a child of the serial number.</remarks>
        /// <param name="serialNumber"></param>
        /// <returns>A unique string.</returns>
        internal static string ToShotSessionId(this uint serialNumber)
        {
            string? id = Nanoid.Generate();

            return serialNumber < 1 ? id : $"{serialNumber.ToString()}[{id}]";
        }

        internal static string ToShotSessionId(this uint? serialNumber)
        {
            return serialNumber is null ? Nanoid.Generate() : serialNumber.Value.ToShotSessionId();
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
