
namespace AWS_Lamda_Example_Api.Models
{
   public class ResourceLockResponse
   {
      public string Id { get; set; }
      public bool Success { get; set; }
      public string Message { get; set; }
      public string LockId { get; set; }
      public string LockType { get; set; }
      public string LockReason { get; set; }
      public Resource Resource { get; set; }
      public DateTime LockTime { get; set; }
      public string LockOwner { get; set; }
      public string LockStatus { get; set; }
      public string LockExpirationDateTime { get; set; }


      
   }


}
