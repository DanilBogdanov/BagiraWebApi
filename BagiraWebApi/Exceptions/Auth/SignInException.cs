namespace BagiraWebApi.Exceptions.Auth
{
    public class SignInException : Exception
    {
        public int AttemptsLeft { get; set; }
        public bool IsWrongCode { get; set; }
        public bool NeedToRequestVerifyCode { get; set; }
    }
}
