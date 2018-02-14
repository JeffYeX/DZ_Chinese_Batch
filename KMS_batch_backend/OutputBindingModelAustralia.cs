

using System;

namespace KMS_batch_backend
{
    class OutputBindingModelAustralia
    {
        public string Message { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string MobileNumber { get; set; }

        public string Address { get; set; }

        public string Suburb { get; set; }

        public string State { get; set; }

        public string PostCode { get; set; }

        public DateTime DateOfBirth { get; set; }

        public bool SourceVerfied { get; set; }

        public string FieldVerifications { get; set; }

        public string NameMatchScore { get; set; }

        public string AddressMatchScore { get; set; }

        public string Color { get; set; }

        public string Reference { get; set; }
    }
}
