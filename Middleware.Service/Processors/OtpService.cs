using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Middleware.Service.DAO;
using Middleware.Service.Model;
using Middleware.Service.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Middleware.Service.Processors
{
    public class OtpService : IOtpService
    {
        private readonly IOtpDAO _otpDAO;
        private readonly ICodeGenerator _codeGenerator;
        private readonly ICryptoService _cryptoService;
        private readonly SystemSettings _settings;
        private readonly IMessageProvider _messageProvider;
        private readonly ILogger _log;

        public OtpService(IOtpDAO otpDAO, ICodeGenerator codeGenerator, ICryptoService cryptoService,
            IOptions<SystemSettings> settingsProvider, IMessageProvider messageProvider,ILoggerFactory log)
        {
            _otpDAO = otpDAO;
            _codeGenerator = codeGenerator;
            _cryptoService = cryptoService;
            _settings = settingsProvider.Value;
            _messageProvider = messageProvider;
            _log = log.CreateLogger(typeof(OtpService));
        }
        public async Task<string> CreateOtpMessage(string username, string language, OtpPurpose otpPurpose)
        {

            try
            {
                _log.LogInformation("Inside the CreateOtpMessage method of the OtpService at {0}", DateTime.UtcNow);
      
            var otp = await _otpDAO.Find(username, otpPurpose);
            string code;
                //if (username.Equals(_settings.ReviewProfile))
                //{
                //    code = "01234";
                //}
                if (_settings.IsTest==true)
                {
                    code = "01234";
                }
                else
               {
                    if (username.Equals(_settings.ReviewProfile))
                    {
                        code = "01234";
                    }
                    else
                    {
                        code = _codeGenerator.Generate(_settings.CodeLength);
                    }
                     
                }
                var salt = _cryptoService.GenerateSalt();
            var message = GenerateMessage(code, language, otpPurpose);
            if (otp == null)
            {
                otp = new Otp
                {
                    Code = _cryptoService.GenerateHash(code, salt),
                    Salt = salt,
                    Purpose = otpPurpose,
                    UserId = username,
                    DateCreated = DateTime.UtcNow
                };
                await _otpDAO.Add(otp);
            }
            else
            {
                otp.Salt = salt;
                otp.DateCreated = DateTime.UtcNow;
                otp.Code = _cryptoService.GenerateHash(code, salt);
                await _otpDAO.Update(otp);
            }

            return message; 
            }
            catch(Exception ex)
            {
                _log.LogCritical(ex, "Server error occurred in the CreateOtpMessage method of the OtpService ");
                return null;

            }
        }

       // private string GenerateMessage(string code, string language, OtpPurpose otpPurpose)
        //{
            //incomplete implementation. This method should use language pack.
            //var template = _messageProvider.GetNotificationMessage(otpPurpose, language);
            //return string.Format(template, code);
            //switch (otpPurpose)
            //{
            //    case OtpPurpose.WALLET_OPENING:
            //        return $"Please use code {code} to continue your FBN wallet opening activity"; ;
            //    case OtpPurpose.DEVICE_SWITCH:
            //        return $"Please use code {code} to activate FBNMobile on your new device";
            //    case OtpPurpose.QUESTIONS_RESET:
            //        return $"Please use code {code} to reset your security questions on your new device";
            //    default:
            //        return $"Please use code {code} on your FBNMobile";
            //}
        //}

        private string GenerateMessage(string code, string language, OtpPurpose otpPurpose, string newAccountNumber = "")
        {
            ////incomplete implementation. This method should use language pack.
            //var template = _messageProvider.GetNotificationMessage(otpPurpose, language);
            //return string.Format(template, code);
            switch (otpPurpose)
            {
                case OtpPurpose.WALLET_OPENING:
                    //return $"Please use code {code} to continue your FBN account opening activity"; ;
                    return $"Please use code {code} on your FBN Mobile App";
                case OtpPurpose.DEVICE_SWITCH:
                    //return $"Please use code {code} to activate FBN Mobile on your new device";
                    return $"Please use code {code} on your FBN Mobile App";
                case OtpPurpose.QUESTIONS_RESET:
                    //return $"Please use code {code} to reset your security questions on your new device";
                    return $"Please use code {code} on your FBN Mobile App";
                case OtpPurpose.PASSWORD_RESET:
                    //return $"Please use code {code} to reset your password on your FBN Mobile App";
                    return $"Please use code {code} on your FBN Mobile App";
                default:
                    return $"Please use code {code} on your FBN Mobile App";
            }

        }
    }
}
