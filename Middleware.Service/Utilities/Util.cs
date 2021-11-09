using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
 
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
 
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Builder;
using Middleware.Core.Model;
using Middleware.Service.BAP;
using Middleware.Service.DTOs;
using Middleware.Service.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Middleware.Service.Utilities
{
    public class Util
    {
        static JsonSerializerSettings settings = new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Ignore };

        public static bool IsEmail(string input)
        {
            var regEx = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

            return regEx.IsMatch(input);
        }

        public static string MaskPhoneNumber(string phonenumber)
        {
            if (phonenumber.Length > 3)
            {
                var lastDigits = phonenumber.Substring(phonenumber.Length - 4, 4);
                var maskedPhonenumber = string.Concat(new String('*', phonenumber.Length - lastDigits.Length), lastDigits);
                phonenumber = maskedPhonenumber;
            }

            return phonenumber;
        }

        public static string SerializeAsJson<T>(T item)
        {
            return JsonConvert.SerializeObject(item);
        }

        public static T DeserializeFromJson<T>(string input)
        {
            return JsonConvert.DeserializeObject<T>(input, settings);
        }

        public static string EncodeString(string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return HttpUtility.HtmlEncode(sb.ToString());
        }

        public static string DecodeString(string value)
        {
            value = HttpUtility.HtmlDecode(value);
            return Regex.Replace(value, @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m =>
                {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        public static TransactionStatus GetTransactionStatus(BAPStatus status)
        {
            if (status == BAPStatus.SUCCESS)
            {
                return TransactionStatus.Successful;
            }
            else if (status == BAPStatus.PENDING)
            {
                return TransactionStatus.Pending;
            }
            return TransactionStatus.Failed;
        }

        public static ValidationCommand GetValidationCommand(string command)
        {
            if (command.ToLower().Equals("merge"))
            {
                return ValidationCommand.merge;
            }
            return ValidationCommand.complete;
        }
        public void SetSessionForLoginFailure()
        {

        }
        public static string GetonyNumbers(string input)
        {
            string[] numbers = Regex.Split(input, @"\D+");
            if (numbers.Length <= 0)
            {
                return input;
            }
            else
            {
                var value = string.Join("", numbers);
                return value;
            }

        }
        public static string RemoveCharacterFromBegining(string input, int index)
        {

            var value = input.Substring(index);
            return value;
        }
        public static string Encryptor(string request, string secretKey)
        {
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] vectorKeyBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


            var encryptor = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256,
                BlockSize = 128,
                IV = vectorKeyBytes,
                Key = secretKeyBytes
            };

            var plainBytes = Encoding.UTF8.GetBytes(request);
            var EncryptedInByte = encryptor.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return Convert.ToBase64String(EncryptedInByte);
        }


        public async static Task<string> Encryptor<T>(T tRequest, string secretKey)
        {
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] vectorKeyBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };


            var encryptor = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256,
                BlockSize = 128,
                IV = vectorKeyBytes,
                Key = secretKeyBytes
            };
            string request = SerializeAsJson<T>(tRequest);

            var plainBytes = Encoding.UTF8.GetBytes(request);
            var EncryptedInByte = encryptor.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            return await Task.FromResult(Convert.ToBase64String(EncryptedInByte));
        }
        public int Count{get;}=4;
        public static string Decryptor(string encryptedData, string secretKey)
        {
            var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

            byte[] vectorKeyBytes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            var decryptor = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                KeySize = 256,
                BlockSize = 128,
                IV = vectorKeyBytes,
                Key = secretKeyBytes,
            };

            var encryptedTextByte = Convert.FromBase64String(encryptedData);
            var DecryptedInBytes = decryptor.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(DecryptedInBytes);
        }
        public static Dictionary<string, string> ConvertStringArrayToDictionary(string[] strArray)
        {
            Dictionary<string, string> data_ = strArray.Zip(strArray.Skip(0), (Key, Value) => new { Key, Value })
         .Where((pair, index) => index % 2 == 0)
         .ToDictionary(pair => pair.Key, pair => pair.Value);

            return data_;
        }

        public static string ConvertStringArrayToJsonString(string requestData)
        {
            var strArray = requestData
                .Replace("\\", "")
                .Replace("{", "")
                .Replace("}", "").Split(',');

            List<string> data_ = strArray.ToList();
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            for (int i = 0; i < strArray.Length; ++i)
            {

                if (i == 0)
                {
                    var slitedData = strArray[i].Split(':');
                    sb.Append("\"");
                    sb.Append($"{slitedData[0]}");
                    sb.Append("\\");
                    sb.Append("\":");
                    sb.Append($"{slitedData[1]}");
                }
                else
                {
                    if (i == strArray.Length - 1)
                    {
                        var slitedData = strArray[i].Split(':');
                        sb.Append("\\");
                        sb.Append("\"");
                        sb.Append($"{slitedData[0]}");
                        sb.Append("\\");
                        sb.Append("\":");
                        sb.Append($"{slitedData[1]}");

                    }
                    else
                    {
                        var slitedData = strArray[i].Split(':');
                        sb.Append("\\");
                        sb.Append("\"");
                        sb.Append($"{slitedData[0]}");
                        sb.Append("\\");
                        sb.Append("\":");
                        sb.Append($"{slitedData[1]}");
                        sb.Append("\",");
                    }
                }


            }
            sb.Append("}");
            string JjsoData = sb.ToString().Replace("\\", "");
            return JjsoData;


        }
        //public static T DecryptRequest<T>(string requestData, string secretKey)
        //{
        //    var tType = typeof(T);
        //    var decryptedRequest = Decryptor(requestData, secretKey);
        //    dynamic json = JObject.Parse(decryptedRequest);

        //    var deserializedRequest = DeserializeFromJson<T>(json.ToString());

        //    return deserializedRequest;
        //}

        //public static T DecryptRequest<T>(string requestData, string secretKey)
        //{
        //    var tType = typeof(T);
        //    var decryptedRequest = Decryptor(requestData, secretKey);
        //    dynamic json = JObject.Parse(decryptedRequest);

        //    var deserializedRequest = DeserializeFromJson<T>(json.ToString());

        //    return deserializedRequest;
        //}

        public static T DecryptRequest<T>(string requestData, string secretKey)
        {
            var decryptedRequest = Decryptor(requestData, secretKey);
            var deserializedRequest = DeserializeFromJson<T>(decryptedRequest);

            return deserializedRequest;
        }
        //public static T DecryptRequest<T>(string requestData, string secretKey)
        //{


        //    var decryptedRequest = Decryptor(requestData, secretKey);
        //    var json = ConvertStringArrayToJsonString(decryptedRequest);
        //    var deserializedRequest = DeserializeFromJson<T>(json);

        //    return deserializedRequest;


        //}
        public static bool IsBase64String(string input)
        {
            Span<byte> buffer = new Span<byte>(new byte[input.Length]);
            return Convert.TryFromBase64String(input, buffer, out int bytesParsed);
        }



        //public static T Clone<T>(this T request)
        //{
        //    var serializedRequest = SerializeAsJson(request);
        //    return DeserializeFromJson<T>(serializedRequest);
        //}

        //public static string GetEnumDescription(this Enum enumValue)
        //{
        //    return enumValue.GetType()
        //               .GetMember(enumValue.ToString())
        //               .First()
        //               .GetCustomAttribute<DescriptionAttribute>()?
        //               .Description ?? enumValue.ToString();
        //}

        //public static bool ContainsStringValue<T>(this T dataObject, string input)
        //{
        //    var type = dataObject.GetType();
        //    foreach (var property in type.GetProperties())
        //    {
        //        if (property.PropertyType.Name.ToLower() == "string")
        //        {
        //            var propValue = (string)property.GetValue(dataObject);
        //            if (propValue.ToLower() == input.ToLower())
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}
        public static async Task SetFileAttribute(string directoryPath, string fileNameithPAth)
        {
            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            directory.Attributes = FileAttributes.Normal;

            File.SetAttributes(fileNameithPAth, FileAttributes.Normal);
            File.Delete(fileNameithPAth);
            await Task.CompletedTask;
        }
    }
}
