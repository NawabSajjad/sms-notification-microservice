using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGA.PFMS.DOMAIN
{
    public class BusinessException : Exception
    {
        public BusinessException(string message):base(message)
        {
        }
        public BusinessException(string message, Exception innerexception):base(message, innerexception) 
        {
        
        }  
    }
}
