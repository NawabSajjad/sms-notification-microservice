using SmsNotification.APPLICATION.Interface;
using SmsNotification.APPLICATION.Request_DTO;
using Microsoft.AspNetCore.Mvc;

namespace SmsNotification.SMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SMSController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IRateLimiter _ratelimiter;

        /// <summary>
        /// constructor for SMSController which initializes the ISmsService and IRateLimiter. The ISmsService is used to handle the logic for sending SMS messages, while the IRateLimiter is used to implement rate limiting on the OTP sending functionality to prevent abuse. This setup allows the controller to manage incoming requests for sending OTPs while ensuring that the system is protected from potential abuse through excessive requests.
        /// </summary>
        /// <param name="smsService"></param>
        public SMSController(ISmsService smsService, IRateLimiter ratelimiter)
        {
            _smsService = smsService;
            _ratelimiter = ratelimiter;
        }

        /// <summary>
        /// method to send OTP. It implements rate limiting to prevent abuse of the OTP sending functionality. The method checks if the number of OTP requests for a given mobile number exceeds the allowed limit (1 request per second in this case). If the limit is exceeded, it returns a BadRequest response indicating that there are too many OTP requests and advises the user to try again later. If the request is allowed, it proceeds to call the ISmsService to send the SMS and returns the result in an Ok response. This approach helps to protect the system from potential abuse while still allowing legitimate OTP requests to be processed efficiently.
        /// </summary>
        /// <param name="requestDto"></param>
        /// <returns></returns>
        [HttpPost("SendOTP")]
        public async Task<ActionResult> SendOtp(SmsRequestDto requestDto)
        {
            var allowed = await _ratelimiter.AllowRequestAsync(
                $"ratelimit:otp:{requestDto.mobile}",
                capacity: 1,
                refillRate: 1 // 1 token/sec
            );

            if (!allowed)
                return BadRequest("Too many OTP requests. Try later.");

            var result = await _smsService.SendSmsAsync(requestDto);
            return Ok(result);

        }
        /// <summary>
        /// method for test API
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetTest")]
        public string GetTest()
        {
            return "SMS Api is working fine";
        }

    }
}
