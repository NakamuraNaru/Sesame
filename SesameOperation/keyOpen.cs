using System;
//using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
using System.Net.Http;  //HttpClientを使うために必要?
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace SesameOperation
{

    //別クラス
    //private HttpClient client = new HttpClient();  //送信機能
    public static class KeyStateChange
    {
        private const String url = "https://api.candyhouse.co/public";    //necessary format
        private const String api_key = "A9QDCiB-RMyOhunAvJLQFqOZW10ALdydLg42xoMySOUqhWTmrBU1fyAgRZoiQYFxCfducc2HEiWa";    //製品によってユニークな値

        [FunctionName("KeyStateChange")]

        /*Sesameの機器情報を取得する*/
        //public static async Task<HttpResponseMessage> receive_sesame_info(HttpClient client) {  //asyncについては後で詳しく
        //    var request = new HttpRequestMessage(HttpMethod.Get, url + "/sesames");    //メッセージ(要求クエリ)
        //    request.Headers.Add("Authorization", api_key);              //ヘッダーを追加
        //    var response = await client.SendAsync(request);             //非同期操作でHTTP要求を送信(awaitは結果待ちの状態を表す。結果を得ると以降の処理が進む。)

        //    return response;
        //}

        ///*Sesameの現状態を取得する*/
        //public HttpClient receive_sesame_state()
        //{

        //    return
        //}

        ///*Sesameを操作(lock/unlock)する*/
        //public HttpClient receive_sesame_operate()
        //{

        //    return
        //}

        ///*Sesameのタスク状況を取得する*/
        //public HttpClient receive_sesame_task_state()
        //{

        //    return
        //}

        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            //String[] id_array = new string[0]; //デバイスIDのみを格納する可変長の一次元配列
            String device_id="";
            //APIキーを用いて、デバイスIDを取得する(後で関数に分ける)
            var client = new HttpClient();  //送信機能
              var request = new HttpRequestMessage(HttpMethod.Get, url + "/sesames");    //メッセージ(要求クエリ)
            request.Headers.Add("Authorization", api_key);              //ヘッダーを追加
            var response = await client.SendAsync(request);             //非同期操作でHTTP要求を送信(awaitは結果待ちの状態を表す。結果を得ると以降の処理が進む。)

            //var response = await receive_sesame_info(client);

            /*セサミの情報取得成功時*/
            if ((int)response.StatusCode == 200)
            {
                string response_body = await response.Content.ReadAsStringAsync();   //HTTP通信で得たbody(device id, serial, nickname)を取得する
                //セサミのデバイスIDを取得する
                string temp = response_body.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");   //不必要な文字を削除
                String[] array = temp.Split(",");    //device id, serial, nicknameの文字列を格納している配列(使うにはsplitが必要)
                device_id = array[0].Split(":")[1].Replace("\"", "").Replace(" ", "");  //不必要な空白とダブルクォーテーションを除去したデバイスID
                //全セサミのデバイスIDを格納する
                //int j = 0;
                //for (int i = 0; i < array.Length; i++)
                //{
                //    if (i % 3 == 0)
                //    {
                //        Array.Resize(ref id_array, id_array.Length + 1);    //要素数を増やす
                //        id_array[j] = array[i].Split(":")[1];
                //        j += 1;
                //    }
                //}
            }
            /*http通信失敗時*/
            else
            {
                return (ActionResult)new OkObjectResult($"Get Sesame informaition Failed");
            }

            string user_select = req.Query["state"];    //ユーザがURLに入力した開け閉めを取得する
            /*URLにlockもしくはunlockが入力された場合*/
            if (user_select == "lock" || user_select == "unlock")
            {
                //Sesameの開錠 +施錠HTTP Client)
                var json = $"{{\"command\":\"{user_select}\"}}";
                var request2 = new HttpRequestMessage(HttpMethod.Post, url + $"/sesame/{device_id}");   //url(要求クエリ)の発行
                request2.Content = new StringContent(json, Encoding.UTF8, "application/json");          //Content-Type+jsonの選択
                request2.Headers.Add("Authorization", api_key);                                         //ヘッダーの指定
                var response2 = await client.SendAsync(request2);                                       //非同期操作でHTTP要求を送信(awaitは結果待ちの状態を表す。結果を得ると以降の処理が進む。)
                string response_body = await response2.Content.ReadAsStringAsync();                     //HTTP通信で得たbody(タスクID)を取得する
                //Console.WriteLine(url + $"/sesame/{device_id}");
                //Console.WriteLine(response_body);
                //Console.WriteLine(response2);
                //Console.WriteLine(json);
                /*lock,unlockの要求が通った時*/
                if ((int)response2.StatusCode == 200)
                {
                    if (user_select == "lock")
                    {
                        return (ActionResult)new OkObjectResult($"Key {user_select} complete");

                    }
                    else if (user_select == "unlock")
                    {
                        return (ActionResult)new OkObjectResult($"Key {user_select} complete");
                    }
                }
                else {
                    return (ActionResult)new OkObjectResult($"{response_body}");
                }
            }
            //lock,unlockのどちらも見当たらない場合
            return new BadRequestObjectResult("Please pass a operation on the query string or in the request body");
        }
    }
}
