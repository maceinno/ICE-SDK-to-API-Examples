using System.Text.RegularExpressions;

namespace AWS_Lamda_Example_Api.Helpers
{
   public static class ICE
   {
      public static string ExtractLockId(string responseContent)
      {
         var match = Regex.Match(responseContent, @"lockId:'(?<lockId>[^']+)'");
         return match.Success ? match.Groups["lockId"].Value : string.Empty;
      }
      public static string ExtractLockedByUserAccount(string responseContent)
      {
         var match = Regex.Match(responseContent, @"user:'(?<user>[^']+)'");
         return match.Success ? match.Groups["user"].Value : string.Empty;
      }

      public static string RemoveCurlyBracketsFromGuid(string guid)
      {
         return guid.Replace("{", "").Replace("}", "");
      }
   }
}
