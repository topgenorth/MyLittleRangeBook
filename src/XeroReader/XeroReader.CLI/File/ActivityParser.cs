/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////

using Dynastream.Fit;

namespace net.opgenorth.xero.File
{
    public class ActivityParser
    {
        readonly FitMessages _messages;

        public ActivityParser(FitMessages messages)
        {
            _messages = messages;
        }

        public bool IsActivityFile => _firstFileIdMesg != null
            ? (_firstFileIdMesg.GetType() ?? System.IO.File.Invalid) == System.IO.File.Activity
            : false;

        public FileIdMesg _firstFileIdMesg => _messages.FileIdMesgs.FirstOrDefault();
        public ActivityMesg _activityMesg => _messages.ActivityMesgs.FirstOrDefault();

        public List< /*SessionMessages*/ object> ParseSessions()
        {
            if (!IsActivityFile)
            {
                throw new Exception(
                    $"Expected FIT File Type: Activity, recieved File Type: {_firstFileIdMesg?.GetType()}");
            }

            // Create a read/write list of session messages
            var sessionMesgs = new List<SessionMesg>(_messages.SessionMesgs);

            // When there are no Sessions but there are Records create a Session message to recover as much data as possible
            if (sessionMesgs.Count == 0 && _messages.RecordMesgs.Count > 0)
            {
                var startTime = _messages.RecordMesgs[0].GetTimestamp();
                var timestamp =
                    _messages.RecordMesgs[_messages.RecordMesgs.Count - 1].GetTimestamp();

                var session = new SessionMesg();
                session.SetStartTime(startTime);
                session.SetTimestamp(timestamp);
                session.SetTotalElapsedTime(timestamp.GetTimeStamp() - startTime.GetTimeStamp());
                session.SetTotalTimerTime(timestamp.GetTimeStamp() - startTime.GetTimeStamp());

                sessionMesgs.Add(session);
            }

            var recordsTaken = 0;

            var sessions = new List<SessionMessages>(sessionMesgs.Count);
            foreach (var sessionMesg in sessionMesgs)
            {
                var session = new SessionMessages(sessionMesg)
                {
                    Laps =
                        _messages.LapMesgs.Skip(sessionMesg.GetFirstLapIndex() ?? 0)
                            .Take(sessionMesg.GetNumLaps() ?? 0)
                            .ToList(),
                    ClimbPros = _messages.ClimbProMesgs.Where(climb => climb.Within(sessionMesg)).ToList(),
                    Events = _messages.EventMesgs.Where(evt => evt.Within(sessionMesg)).ToList(),
                    DeviceInfos =
                        _messages.DeviceInfoMesgs.Where(deviceInfo => deviceInfo.Within(sessionMesg)).ToList(),
                    Lengths = _messages.LengthMesgs.Where(length => length.Overlaps(sessionMesg)).ToList(),
                    Records =
                        _messages.RecordMesgs.Skip(recordsTaken)
                            .Where(record => record.Within(sessionMesg))
                            .ToList(),
                    SegmentLaps =
                        _messages.SegmentLapMesgs.Where(segmentLap => segmentLap.Overlaps(sessionMesg)).ToList(),
                    TimerEvents =
                        _messages.EventMesgs.Where(evt => evt.GetEvent() == Event.Timer && evt.Within(sessionMesg))
                            .ToList(),
                    FrontGearChangeEvents =
                        _messages.EventMesgs.Where(evt =>
                                evt.GetEvent() == Event.FrontGearChange && evt.Within(sessionMesg))
                            .ToList(),
                    RearGearChangeEvents =
                        _messages.EventMesgs.Where(evt =>
                                evt.GetEvent() == Event.RearGearChange && evt.Within(sessionMesg))
                            .ToList(),
                    RiderPositionChangeEvents =
                        _messages.EventMesgs.Where(evt =>
                                evt.GetEvent() == Event.RiderPositionChange && evt.Within(sessionMesg))
                            .ToList(),
                    FileId = _firstFileIdMesg,
                    Activity = _activityMesg
                };

                recordsTaken += session.Records.Count;
                sessions.Add(session);
            }

            return new List<object>();
        }

        public List<DeviceInfoMesg> DevicesWhereBatteryStatusIsLow()
        {
            var batteryStatus = new List<byte> { BatteryStatus.Critical, BatteryStatus.Low };
            var deviceInfos = new List<DeviceInfoMesg>();

            deviceInfos = _messages.DeviceInfoMesgs
                .Where(info => batteryStatus.Contains(info.GetBatteryStatus() ?? BatteryStatus.Unknown))
                .ToList();

            return deviceInfos;
        }
    }
}
