using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YmatouMQ.Common
{
    public class ResponseData<T>
    {
        //public bool Success { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
        public T Result { get; set; }
        public static Task<ResponseData<T>> CreateSuccessTask(T val, string message = "")
        {
            TaskCompletionSource<ResponseData<T>> tcs = new TaskCompletionSource<ResponseData<T>>();
            tcs.TrySetResult(CreateSuccess(val, message));
            return tcs.Task;
        }
        public static Task<ResponseData<T>> CreateFailTask(T val, int errorCode = 400, string lastErrorMessage = "")
        {
            TaskCompletionSource<ResponseData<T>> tcs = new TaskCompletionSource<ResponseData<T>>();
            tcs.TrySetResult(CreateFail(val, errorCode, lastErrorMessage));
            return tcs.Task;
        }
        public static Task<ResponseData<T>> CreateTask(T val, bool success, string lastErrorMessage = "", string errorCode = null, string successMsg = null)
        {
            TaskCompletionSource<ResponseData<T>> tcs = new TaskCompletionSource<ResponseData<T>>();
            tcs.TrySetResult(Create(val, success, lastErrorMessage, errorCode, successMsg));
            return tcs.Task;
        }
        public static ResponseData<T> CreateSuccess(T val, string message = "")
        {
            return new ResponseData<T>
            {
                //Success = true,
                Result = val,
                Code = 200,
                Message = message
            };
        }

        public static ResponseData<T> CreateFail(T val, int errorCode = 400, string lastErrorMessage = "")
        {
            return new ResponseData<T>
            {
                //Success = false,
                Result = val,
                Code = errorCode,
                Message = lastErrorMessage,

            };
        }

        public static ResponseData<T> Create(T val, bool success, string lastErrorMessage = "", string errorCode = null, string successMsg = null)
        {
            var code = 400;
            if (!string.IsNullOrEmpty(errorCode))
                int.TryParse(errorCode, out code);

            return new ResponseData<T>
            {
                //Success = success,
                Result = val,
                Message = lastErrorMessage,
                Code = success ? 200 : code,
            };
        }
    }
    public class ResponseNull
    {
        public static readonly ResponseNull _Null = new ResponseNull();
    }
}
