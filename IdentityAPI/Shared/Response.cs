namespace IdentityAPI.Shared
{
    public class Response
    {
        public string Message { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public DateTime? ExpireDate { get; set; }
    }
}