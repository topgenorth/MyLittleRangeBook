/////////////////////////////////////////////////////////////////////////////////////////////
// Copyright 2023 Garmin International, Inc.
// Licensed under the Flexible and Interoperable Data Transfer (FIT) Protocol License; you
// may not use this file except in compliance with the Flexible and Interoperable Data
// Transfer (FIT) Protocol License.
/////////////////////////////////////////////////////////////////////////////////////////////

using Dynastream.Fit;

namespace net.opgenorth.xero.File
{
    public class FitDecoder
    {
        readonly FitListener _fitListener = new();

        readonly Dynastream.Fit.File fileType;

        readonly Stream inputStream;

        public HashSet<string> RecordDeveloperFieldNames = new();

        public HashSet<string> RecordFieldNames = new();

        public FitDecoder(Stream stream, Dynastream.Fit.File fileType)
        {
            inputStream = stream;
            this.fileType = fileType;
        }

        public FitMessages FitMessages => _fitListener.FitMessages;

        public bool Decode()
        {
            // Create the Decode Object
            var decoder = new Decode();

            // Check that this is a FIT file
            if (!decoder.IsFIT(inputStream))
            {
                throw new FileTypeException($"Expected FIT File Type: {fileType}, received a non FIT file.");
            }

            // Create the Message Broadcaster Object
            var mesgBroadcaster = new MesgBroadcaster();

            // Connect the the Decode and Message Broadcaster Objects
            decoder.MesgEvent += mesgBroadcaster.OnMesg;

            // Connect Message Broadcaster Events to their onMesg delegates
            mesgBroadcaster.FileIdMesgEvent += OnFileIdMesg;
            mesgBroadcaster.RecordMesgEvent += OnRecordMesg;

            // Connect FitListener to get lists of each message type with FitMessages
            decoder.MesgEvent += _fitListener.OnMesg;

            // Decode the FIT File
            try
            {
                var readOK = decoder.Read(inputStream);

                // If there are HR messages, merge the heart-rate data with the Record messages.
                // if (readOK && FitMessages.HrMesgs.Count > 0)
                // {
                //     HrToRecordMesgWithoutPlugin.MergeHeartRates(FitMessages);
                //     RecordFieldNames.Add("Heart Rate");
                // }

                return readOK;
            }
            catch (FileTypeException ex)
            {
                throw ex;
            }
            catch (FitException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void OnFileIdMesg(object sender, MesgEventArgs e)
        {
            if ((e.mesg as FileIdMesg).GetType() != fileType)
            {
                throw new FileTypeException(
                    $"Expected FIT File Type: {fileType}, recieved File Type: {(e.mesg as FileIdMesg).GetType()}");
            }
        }

        public void OnRecordMesg(object sender, MesgEventArgs e)
        {
            foreach (var field in e.mesg.Fields)
            {
                if (field.Name.ToLower() != "unknown")
                {
                    RecordFieldNames.Add(field.Name);
                }
            }

            foreach (var devField in e.mesg.DeveloperFields)
            {
                RecordDeveloperFieldNames.Add(devField.Name);
            }
        }
    }
}
