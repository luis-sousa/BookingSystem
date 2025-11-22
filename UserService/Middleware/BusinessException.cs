namespace UserService.Middleware
{
    public class BusinessException : Exception
    {
        public int Code { get; }

        public int HttpCode { get; }

        public BusinessException(int httpcode, int code, string message) : base(message)
        {
            Code = code;
            HttpCode = httpcode;
        }
    }
}
