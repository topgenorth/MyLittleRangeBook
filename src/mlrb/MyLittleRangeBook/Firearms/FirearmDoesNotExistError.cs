namespace MyLittleRangeBook.Firearms
{
    public class FirearmDoesNotExistError : Error
    {
        public FirearmDoesNotExistError(string mlrbId)
            : base($"Firearm with id `{mlrbId}` was not found")
        {
            Metadata.Add("MlrbId", mlrbId);
        }
    }
}