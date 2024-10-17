using AWS_Lamda_Example_Api.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace AWS_Lamda_Example_Api.Controllers
{
   [Route("api/[controller]")]
   public class IceController : ControllerBase
   {
      private static string iceAuthenticationUrl = "https://api.elliemae.com/oauth2/v1/token";
      private static string loanFieldManagementUrl = "https://api.elliemae.com/encompass/v3/loans/";
      private static string iceV3BaseUrl = "https://api.elliemae.com/encompass/v3";
      private readonly AuthenticationSettings _authSettings;
      public IceController(IConfiguration configuration)
      {
         _authSettings = configuration.GetSection("Authentication").Get<AuthenticationSettings>();
      }

      // GET api/ice
      [HttpGet]
      public IActionResult Get()
      {
         var response = new
         {
            status = "Success!",
            message = "Your new function is now running on AWS Lamda. Now lets do some work!"
         };

         return new JsonResult(response);
      }

      // POST api/ice/authenticate
      [HttpPost("authenticate")]
      public async Task<IActionResult> AuthenticateAsync([FromBody] AuthenticationRequest request)
      {
         if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.Secret))
         {
            return BadRequest("Invalid client ID or secret.");
         }

         var authResponse = await PerformAuthenticationAsync(request);

         return new JsonResult(authResponse);
      }

      // POST api/ice/loan-field-reader
      [HttpPost("loan-field-reader")]
      public async Task<IActionResult> GetLoanFieldValuesAsync([FromBody] FieldReaderRequest request)
      {
         var stopwatch = Stopwatch.StartNew(); // Start the stopwatch

         if (request == null || string.IsNullOrEmpty(request.Guid) && (request.LoanFields == null || request.LoanFields.Count == 0))
         {
            return BadRequest("Loan GUID and/or LoanFields were not provided. Please try again.");
         }

         if (_authSettings == null || string.IsNullOrEmpty(_authSettings.ClientId) || string.IsNullOrEmpty(_authSettings.Secret))
         {
            return BadRequest("Invalid client ID or secret. Please check your appsettings.json configuration");
         }

         var authRequest = new AuthenticationRequest
         {
            ClientId = _authSettings.ClientId,
            Secret = _authSettings.Secret
         };

         var authResponse = await PerformAuthenticationAsync(authRequest);

         if (authResponse is { Success: true, Token: string token })
         {
            var loanFieldsResponse = await GetLoanFieldDataAsync(token, request);
            stopwatch.Stop(); // Stop the stopwatch
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds; // Get the elapsed time in milliseconds
            var elapsedSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2); // Get the elapsed time in seconds, rounded to the hundredths place

            return new JsonResult(new
            {
               loanFieldsResponse,
               elapsedMilliseconds,
               elapsedSeconds
            });
         }

         stopwatch.Stop(); // Stop the stopwatch if authentication fails
         var elapsedMillisecondsOnFailure = stopwatch.ElapsedMilliseconds; // Get the elapsed time in milliseconds
         var elapsedSecondsOnFailure = Math.Round(stopwatch.Elapsed.TotalSeconds, 2); // Get the elapsed time in seconds, rounded to the hundredths place

         return new JsonResult(new
         {
            authResponse,
            elapsedMilliseconds = elapsedMillisecondsOnFailure,
            elapsedSeconds = elapsedSecondsOnFailure
         });
      }

      // POST api/ice/loan-field-writer
      [HttpPost("loan-field-writer")]
      public async Task<IActionResult> PostLoanFieldValuesAsync([FromBody] FieldWriterRequest writerRequest)
      {
         var stopwatch = Stopwatch.StartNew(); // Start the stopwatch

         if (_authSettings == null || string.IsNullOrEmpty(_authSettings.ClientId) || string.IsNullOrEmpty(_authSettings.Secret))
         {
            return BadRequest("Invalid client ID or secret. Please check your appsettings.json configuration");
         }

         if (writerRequest == null || string.IsNullOrEmpty(writerRequest.Guid) || writerRequest.LoanFieldsToUpdate == null || writerRequest.LoanFieldsToUpdate.Count == 0)
         {
            return BadRequest("Loan GUID, and/or LoanFieldsToUpdate were not provided. Please try again.");
         }

         var authRequest = new AuthenticationRequest
         {
            ClientId = _authSettings.ClientId,
            Secret = _authSettings.Secret
         };

         var authResponse = await PerformAuthenticationAsync(authRequest);

         if (authResponse is { Success: true, Token: string token })
         {

            //try to obtain an exclusive lock on the loan
            //if the lock is obtained, write the loan fields
            var lockRequest = new ResourceLockRequest
            {
               Resource = new Resource
               {
                  EntityId = writerRequest.Guid,
                  EntityType = "Loan"
               },
               LockType = "exclusive"

            };

            var lockResponse = await LockResourceAsync(token, lockRequest);
            if (lockResponse is { Success: true, LockId: string lockId })
            {
               writerRequest.LockId = lockId;
            }
            else
            {
               if (Debugger.IsAttached)
               {
                  await UnlockResourceExclusiveLoanLock(token, lockResponse.LockId, lockRequest.Resource.EntityId);
                  lockResponse = await LockResourceAsync(token, lockRequest);
                  if (lockResponse is { Success: true, LockId: string lockIdSecondAttempt })
                  {
                     writerRequest.LockId = lockIdSecondAttempt;
                  }
                  else
                  {
                     return new JsonResult(lockResponse);
                  }
               }


            }

            var loanFieldsResponse = await WriteLoanFieldDataAsync(token, writerRequest);
            stopwatch.Stop(); // Stop the stopwatch
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds; // Get the elapsed time in milliseconds
            var elapsedSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2); // Get the elapsed time in seconds, rounded to the hundredths place

            return new JsonResult(new
            {
               loanFieldsResponse,
               elapsedMilliseconds,
               elapsedSeconds
            });
         }

         stopwatch.Stop(); // Stop the stopwatch if authentication fails
         var elapsedMillisecondsOnFailure = stopwatch.ElapsedMilliseconds; // Get the elapsed time in milliseconds
         var elapsedSecondsOnFailure = Math.Round(stopwatch.Elapsed.TotalSeconds, 2); // Get the elapsed time in seconds, rounded to the hundredths place

         return new JsonResult(new
         {
            authResponse,
            elapsedMilliseconds = elapsedMillisecondsOnFailure,
            elapsedSeconds = elapsedSecondsOnFailure
         });
      }

      //Private Methods
      private async Task<AuthenticationResponse> PerformAuthenticationAsync(AuthenticationRequest request)
      {
         using (var client = new HttpClient())
         {
            var authHeader = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{request.ClientId}:{request.Secret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            // Your scope will be different as lender, the scope is "password" and you must provide username and password header values
            // For more information on scopes, please visit: https://developer.icemortgagetechnology.com/developer-connect/docs/authentication#resource-owner-password-credentials
            // Also you can find all the documention on the ICE Developer Connect site: https://developer.icemortgagetechnology.com/developer-connect/docs/authentication

            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("instance_id", "be11144930"),
                    new KeyValuePair<string, string>("scope", "lp")
                });

            var response = await client.PostAsync(iceAuthenticationUrl, content);

            if (response.IsSuccessStatusCode)
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               var authResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
               return new AuthenticationResponse
               {
                  Success = true,
                  Message = "Auth Success!",
                  Token = authResult?.access_token?.ToString() ?? string.Empty // Fix for CS8602
               };
            }
            else
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               return new AuthenticationResponse
               {
                  Success = false,
                  Message = "Auth Failed!",
                  Response = responseContent
               };
            }
         }
      }
      private async Task<bool> UnlockResourceExclusiveLoanLock(string token, string lockId, string loanGuid)
      {
         using (var client = new HttpClient())
         {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestUrl = $"{iceV3BaseUrl}/resourceLocks/{lockId}?resourceType=loan&resourceId={loanGuid.Replace("{", string.Empty).Replace("}", string.Empty)}";

            HttpResponseMessage response = await client.DeleteAsync(requestUrl);

            return response.IsSuccessStatusCode;
         }
      }
      private async Task<ResourceLockResponse> LockResourceAsync(string token, ResourceLockRequest lockRequest)
      {
         using (var client = new HttpClient())
         {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestUrl = $"{iceV3BaseUrl}/resourcelocks?view=entity";

            var jsonContent = JsonConvert.SerializeObject(lockRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(requestUrl, content);

            if (response.IsSuccessStatusCode)
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               var lockResponse = JsonConvert.DeserializeObject<ResourceLockResponse>(responseContent);
               return new ResourceLockResponse
               {
                  Resource = lockRequest.Resource,
                  LockType = lockRequest.LockType,
                  LockReason = lockResponse?.LockReason ?? string.Empty,
                  Success = true,
                  Message = "Resource locked successfully!",
                  LockId = lockResponse?.Id ?? string.Empty
               };
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               string lockId = Helpers.ICE.ExtractLockId(responseContent);
               string userId = Helpers.ICE.ExtractLockedByUserAccount(responseContent);
               return new ResourceLockResponse
               {
                  Resource = lockRequest.Resource,
                  LockType = lockRequest.LockType,
                  LockReason = responseContent,
                  Success = false,
                  LockId = lockId,
                  LockOwner = userId,
                  LockStatus = "exclusive",
                  Message = "Resource/Loan is already locked!"
               };
            }
            else
            {
               var responseContent = await response.Content.ReadAsStringAsync();
               return new ResourceLockResponse
               {
                  Resource = lockRequest.Resource,
                  LockType = lockRequest.LockType,
                  LockReason = responseContent,
                  Success = false,
                  Message = "Failed to lock resource!"
               };
            }
         }
      }
      private async Task<object> GetLoanFieldDataAsync(string token, FieldReaderRequest readerRequest)
      {
         #region Response Object Example
         /* response Object example
         {
             "loanFieldsResponse": {
                 "success": true,
                 "message": "Loan Fields Retrieved Successfully!",
                 "loanFields": {
                     "4000": "John",
                     "4001": ""
                 },
                 "headers": {
                     "X-Concurrency-Limit-Limit": "30",
                     "X-Concurrency-Limit-Remaining": "30",
                     "X-Rate-Limit-Limit": "500000",
                     "X-Rate-Limit-Remaining": "500000",
                     "X-Rate-Limit-Reset": "1729123200"
                 },
                 "retryCount": 0
             },
             "elapsedMilliseconds": 905,
             "elapsedSeconds": 0.91
          }
         */
         #endregion

         using (var client = new HttpClient())
         {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestUrl = $"{loanFieldManagementUrl}{readerRequest.Guid}/fieldReader";

            var jsonContent = JsonConvert.SerializeObject(readerRequest.LoanFields);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            int retryCount = 0;
            const int maxRetries = 200;

            do
            {
               response = await client.PostAsync(requestUrl, content);

               var headers = response.Headers
                   .Where(h => h.Key.Contains("X-Rate") || h.Key.Contains("X-Concurrency"))
                   .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

               // Check "X-Concurrency-Limit-Remaining" header
               if (headers.TryGetValue("X-Concurrency-Limit-Remaining", out var concurrencyLimitRemainingValue))
               {
                  if (int.TryParse(concurrencyLimitRemainingValue, out var concurrencyLimitRemaining) && concurrencyLimitRemaining < 5)
                  {
                     retryCount++;
                     await Task.Delay(1000); // Wait for 1 second before retrying
                     continue;
                  }
               }

               if (response.IsSuccessStatusCode)
               {
                  var responseContent = await response.Content.ReadAsStringAsync();
                  var loanFields = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent) ?? new Dictionary<string, string>();
                  return new
                  {
                     success = true,
                     message = "Loan Fields Retrieved Successfully!",
                     loanFields,
                     headers,
                     retryCount = retryCount
                  };
               }
               else
               {
                  var responseContent = await response.Content.ReadAsStringAsync();
                  return new
                  {
                     success = false,
                     message = "Failed to Retrieve Loan Fields!",
                     response = responseContent,
                     headers,
                     retryCount = retryCount
                  };
               }
            } while (retryCount < maxRetries);

            return new
            {
               success = false,
               message = "Failed to Retrieve Loan Fields after multiple retries.",
               headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
            };
         }
      }
      private async Task<object> WriteLoanFieldDataAsync(string token, FieldWriterRequest writerRequest)
      {
         using (var client = new HttpClient())
         {
            writerRequest.Guid = Helpers.ICE.RemoveCurlyBracketsFromGuid(writerRequest.Guid);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestUrl = $"{loanFieldManagementUrl}{writerRequest.Guid}/fieldWriter?{writerRequest.LockId}";

            var jsonContent = JsonConvert.SerializeObject(writerRequest.LoanFieldsToUpdate);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            int retryCount = 0;
            const int maxRetries = 200;

            do
            {
               response = await client.PostAsync(requestUrl, content);

               var headers = response.Headers
                   .Where(h => h.Key.Contains("X-Rate") || h.Key.Contains("X-Concurrency"))
                   .ToDictionary(h => h.Key, h => string.Join(", ", h.Value));

               // Check "X-Concurrency-Limit-Remaining" header
               if (headers.TryGetValue("X-Concurrency-Limit-Remaining", out var concurrencyLimitRemainingValue))
               {
                  if (int.TryParse(concurrencyLimitRemainingValue, out var concurrencyLimitRemaining) && concurrencyLimitRemaining < 5)
                  {
                     retryCount++;
                     await Task.Delay(1000); // Wait for 1 second before retrying
                     continue;
                  }
               }

               if (response.IsSuccessStatusCode)
               {
                  var responseContent = await response.Content.ReadAsStringAsync();
                  var loanFields = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent) ?? new Dictionary<string, string>();
                  return new
                  {
                     success = true,
                     message = "Loan Fields Written Successfully!",
                     loanFields,
                     headers,
                     retryCount = retryCount
                  };
               }
               else
               {
                  var responseContent = await response.Content.ReadAsStringAsync();
                  return new
                  {
                     success = false,
                     message = "Failed to Write Loan Fields!",
                     response = responseContent,
                     headers,
                     retryCount = retryCount
                  };
               }
            } while (retryCount < maxRetries);

            return new
            {
               success = false,
               message = "Failed to Write Loan Fields after multiple retries.",
               headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(", ", h.Value))
            };
         }
      }
   }

}
