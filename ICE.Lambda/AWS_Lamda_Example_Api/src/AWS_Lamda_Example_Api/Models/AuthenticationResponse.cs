namespace AWS_Lamda_Example_Api.Models
{
    public class AuthenticationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Response { get; set; }
    }
}
