namespace AWS_Lamda_Example_Api.Models
{
   public class FieldReaderRequest
   {
      public string Guid { get; set; }
      public List<string> LoanFields { get; set; }
   }
}
