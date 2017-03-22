using System;

namespace KMS_batch_backend
{
    public class InputBindingModel
    {
        public string DZID { get; set; }

        public string CustomerReference { get; set; }

        public bool ShowPhoto { get; set; }

        public string FullName { get; set; }

        public string IdCardnumber { get; set; }

        public DateTime DateOfBirth { get; set; }
    }
}