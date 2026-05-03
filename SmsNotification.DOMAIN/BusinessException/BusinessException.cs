
namespace SmsNotification.DOMAIN
{
    public class BusinessException : Exception
    {
        public BusinessException(string message):base(message)
        {
            //No code required its for middile ware exception handling.
        }
        public BusinessException(string message, Exception innerexception):base(message, innerexception) 
        {
            //No code required its for middile ware exception handling.
        }  
    }
}
