namespace KMS_batch_backend
{
    public class OutputBindingModel
    {
        public string Message { get; set; }

        public string DZID { get; set; }

        public string CustomerReference { get; set; }

        public string InputFullName { get; set; }

        public string InputDOB { get; set; }

        public bool SourceVerfied { get; set; }

        public bool IdCardNoValid { get; set; }

        public bool DateOfBirthVerified { get; set; }

        public string AddressLocality { get; set; }

        public string Gender { get; set; }

        public string PhotoUrl { get; set; }

        public string WatchListPdf { get; set; }

        public string WatchListCategory { get; set; }

        public string ScanId { get; set; }

        public string ErrorMessages { get; set; }
    }
}