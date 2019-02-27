using System;

namespace KMS_batch_backend
{
    public class InputBindingModel
    {
        public string DZID { get; set; }

        public string CustomerReference { get; set; }

        public bool ShowPhoto { get; set; }

        public string FullName { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string IdCardnumber { get; set; }

        public string StreetNumber { get; set; }

        public string StreetName { get; set; }

        public string City { get; set; }

        public string DLNumber { get; set; }

        public string DLVersion { get; set; }

        public string PostCode { get; set; }


        public DateTime DateOfBirth { get; set; }


    }
}