namespace BagiraWebApi.Dtos.Auth
{
    public class SignInDTO : SignInAbstractDTO
    {
        public required string Login { get; set; }
        public required string Code { get; set; }
    }
}
