using iText.Commons.Utils;
using System;
using System.Collections.Generic;

namespace Pregiato.API.Response
{
    public class ApiResponse<T>
    {
        public bool STATUSSUCCESS { get; set; }
        public T? DATA { get; set; }
        public string MESSAGE { get; set; }
        public List<string> ERRORS { get; set; }
        public string FILEBASE64 { get; set; }
        public string FILENAME { get; set; }
        public string FILEURL { get; set; }
        public ApiResponse()
        {
            ERRORS = (List<string>) []; 
        }

        public static ApiResponse<T?> Success(T? data, string message)
        {
            return new ApiResponse<T?>
            {
                STATUSSUCCESS = true,
                DATA = data,
                MESSAGE = message,
                ERRORS = []
            };
        }

      
        public static ApiResponse<T> CreateResponse(bool success, string message, T data, string fileBase64 = null, string fileName = null, string fileUrl = null)
        {
            return new ApiResponse<T>
            {
                STATUSSUCCESS = success,
                MESSAGE = message,
                DATA = data,
                FILEBASE64 = fileBase64,
                FILENAME = fileName,
                FILEURL = fileUrl,
                ERRORS = []
            };
        }

        public static ApiResponse<T> Fail(string message, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                STATUSSUCCESS = false,
                MESSAGE = message,
                ERRORS = errors ?? []
            };
        }

        public static ApiResponse<object> Info(string message)
        {
            return new ApiResponse<object>
            {
                STATUSSUCCESS = false,
                MESSAGE = message,
            };
        }
    }
}
