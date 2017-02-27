using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YmatouMQ.Common;
using YmatouMQ.Common.Dto;
using YmatouMQ.Common.Extensions.Serialization;

namespace YmatouMQServerConsoleApp.owin
{
    public static class _OWinResponse
    {
        public static void OutPut<TResponse>(this IDictionary<string, object> env, TResponse reponse)
        {
            Stream responseBody = Util.ResponseBody(env);
            try
            {
                var json = reponse.JSONSerializationToByte();
                Util.ResponseHeaders(env)["Content-Type"] = new[] { "application/json;charset=utf-8" };
                responseBody.Write(json, 0, json.Length);
            }
            catch (Exception ex)
            {
                var by = ex.JSONSerializationToByte();
                responseBody.Write(by, 0, by.Length);
            }
            finally
            {
                responseBody.Close();
            }
        }
        public static async Task OutPutAsync<TResponse>(this IDictionary<string, object> env, TResponse reponse)
        {
            try
            {
                var json = reponse.JSONSerializationToByte();
                Util.ResponseHeaders(env)["Content-Type"] = new[] { "application/json;charset=utf-8" };
                Stream responseBody = Util.ResponseBody(env);
                await responseBody.WriteAsync(json, 0, json.Length);
                responseBody.Close();
            }
            catch
            { }
        }
        public static async Task OutPut<TResponse>(this OwinContext env, ResponseData<TResponse> reponse)
        {
            var json = reponse.JSONSerializationToString();
            Util.ResponseHeaders(env.Environment)["Content-Type"] = new[] { "application/json;charset=utf-8" };
            Stream output = Util.ResponseBody(env.Environment);
            using (var writer = new StreamWriter(output))
            {
                await writer.WriteAsync(json).ConfigureAwait(false);
            }
        }

        public static async Task OutPut(this OwinContext env, string jsonString)
        {
            Util.ResponseHeaders(env.Environment)["Content-Type"] = new[] { "application/json;charset=utf-8" };
            Stream output = Util.ResponseBody(env.Environment);
            using (var writer = new StreamWriter(output))
            {
                await writer.WriteAsync(jsonString).ConfigureAwait(false);
            }
        }
    }
}
