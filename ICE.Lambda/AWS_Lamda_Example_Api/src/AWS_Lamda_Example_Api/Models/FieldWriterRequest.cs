namespace AWS_Lamda_Example_Api.Models
{
   public class FieldWriterRequest
   {
      public string Guid { get; set; }

      public string LockId { get; set; }
      public List<FieldWriterContract> LoanFieldsToUpdate { get; set; }
   }
}
