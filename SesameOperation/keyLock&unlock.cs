using System;
//using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
using System.Net.Http;  //Use HttpClient Required
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace SesameOperation
{

    //Separate Class
    //private HttpClient client = new HttpClient();  //Send Function
    public static class KeyStateChange
    {
        private const String url = "https://api.candyhouse.co/public";    //necessary format
        //Set API Key(See ToDo.txt)
        private const String api_key = "";    //unique value per product

        [FunctionName("KeyStateChange")]

        /* Get sesame product information */
        //public static async Task<HttpResponseMessage> receive_sesame_info(HttpClient client) {  // Research async information datailed
        //    var request = new HttpRequestMessage(HttpMethod.Get, url + "/sesames");    // message(Required Query)
        //    request.Headers.Add("Authorization", api_key);              // add header
        //    var response = await client.SendAsync(request);             // Send HTTP Requirement by async(await is result wait state. When get result, advance afterwards process.)

        //    return response;
        //}

        ///* Get sesame current state */
        //public HttpClient receive_sesame_state()
        //{

        //    return
        //}


        ///* Operate(lock/unlock) sesame */
        //public HttpClient receive_sesame_operate()
        //{

        //    return
        //}

        ///* Get sesame task statement */
        //public HttpClient receive_sesame_task_state()
        //{

        //    return
        //}

        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            //String[] id_array = new string[0]; //�f�o�C�XID�݂̂��i�[����ϒ��̈ꎟ���z��
            String device_id="";
            //Get device ID by API key (later separate function)
            var client = new HttpClient();  //Send Function
            var request = new HttpRequestMessage(HttpMethod.Get, url + "/sesames");    // message(Required Query)
            request.Headers.Add("Authorization", api_key);              // add header
            var response = await client.SendAsync(request);             // Send HTTP Requirement by async(await is result wait state. When get result, advance afterwards process.)

            //var response = await receive_sesame_info(client);

            /* Get sesame product information success */
            if ((int)response.StatusCode == 200)
            {
                string sesame_info_body = await response.Content.ReadAsStringAsync();   // body(device ID, serial, nickname) by Http communication
                // Get sesame device ID
                string info_temp = sesame_info_body.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", "");   // Replace unnecessary character
                String[] info_array = info_temp.Split(",");    // Store device ID and serial, nickname(when use, split is necessary)
                device_id = info_array[0].Split(":")[1].Replace("\"", "").Replace(" ", "");  // device ID(unnecessary space and double quartation is replaced)
                // Store all sesame device ID
                //int j = 0;
                //for (int i = 0; i < array.Length; i++)
                //{
                //    if (i % 3 == 0)
                //    {
                //        Array.Resize(ref id_array, id_array.Length + 1);    //�v�f���𑝂₷
                //        id_array[j] = array[i].Split(":")[1];
                //        j += 1;
                //    }
                //}
            }
            /* Http communication Failed */
            else
            {
                return (ActionResult)new OkObjectResult($"Get Sesame informaition Failed");
            }

            //Get sesame current state
            request = new HttpRequestMessage(HttpMethod.Get, url + $"/sesame/{device_id}");    // message(Required Query)
            request.Headers.Add("Authorization", api_key);               // add header
            response = await client.SendAsync(request);             // Send HTTP Requirement by async(await is result wait state. When get result, advance afterwards process.)
            string status_body = await response.Content.ReadAsStringAsync();   // body(locked, battery, responsive) by Http communication
            
            // Get sesame current state(lock or unlock)
            string status_temp = status_body.Replace("{", "").Replace("}", "");   // Replace unnecessary character
            String[] status_array = status_temp.Split(",");     // Store locked and battery, responsive(when use, split is necessary)
            var locked = status_array[0].Split(":")[1].Replace(" ", "");         // locked
            var battery = status_array[1].Split(":")[1].Replace(" ", "");        // battery
            var responsive = status_array[2].Split(":")[1].Replace(" ", "");     // responsive

            //Console.WriteLine(locked);
            //Console.WriteLine(battery);
            //Console.WriteLine(responsive);
            
            //Get user input(lock or unlock)
            string user_select = req.Query["operation"];
            Console.WriteLine(locked);
            Console.WriteLine(user_select);
            /* case lock or unlock in URL */
            if ((locked == "false" && user_select == "lock") || (locked == "true" && user_select == "unlock"))
            {
                // lock or unlock sesame
                var json = $"{{\"command\":\"{user_select}\"}}";
                var request2 = new HttpRequestMessage(HttpMethod.Post, url + $"/sesame/{device_id}");   // message(Required Query)
                request2.Content = new StringContent(json, Encoding.UTF8, "application/json");          // Select Content-Type+json
                request2.Headers.Add("Authorization", api_key);                                         // add header
                var response2 = await client.SendAsync(request2);                                       // Send HTTP Requirement by async(await is result wait state. When get result, advance afterwards process.)
                string taskid_body = await response2.Content.ReadAsStringAsync();                       // body(taskID) by Http communication
                //Console.WriteLine(url + $"/sesame/{device_id}");
                //Console.WriteLine(response_body);
                //Console.WriteLine(response2);
                //Console.WriteLine(json);

                /* lock or unlock success */
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
                else
                {
                    return (ActionResult)new OkObjectResult($"{locked}");
                }
            }
            // Already locked
            else if(locked == "true" && user_select == "lock") {
                return new BadRequestObjectResult("Already Locked!!");
            }

            // Already unlocked
            else if (locked == "false" && user_select == "unlock"){
                return new BadRequestObjectResult("Already Unlocked!!");
            }
            Console.WriteLine(locked);
            Console.WriteLine(user_select);
            Console.WriteLine("a");

            // None lock or unlock in URL
            return new BadRequestObjectResult("Please pass a operation on the query string or in the request body");
        }
    }
}
