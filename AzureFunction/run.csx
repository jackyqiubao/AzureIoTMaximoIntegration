#r "Azure.Messaging.EventGrid"
#r "System.Memory.Data"
#r "Newtonsoft.Json"

using Azure.Messaging.EventGrid;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Text;
public static async  void Run(EventGridEvent eventGridEvent, ILogger log)
{
    var eventData=eventGridEvent.Data.ToString();
    log.LogInformation("Functionlog: "+eventData);
    JObject json = JObject.Parse(eventData);
    JToken token = json.SelectToken("systemProperties.iothub-connection-device-id");    
    var deviceID=token?.ToString();
    log.LogInformation("Device ID: "+deviceID);
    //token = json.SelectToken("systemProperties.dt-subject");  
    token = json.SelectToken("body.metername");    
    var meterID=token?.ToString();
    log.LogInformation("Meter ID: "+meterID);    
    token = json.SelectToken("body.temperature");
    var meterReading=token?.ToString();
    log.LogInformation("Meter Reading: "+meterReading);
    token = json.SelectToken("systemProperties.iothub-enqueuedtime");
    var eventTime=token?.ToString(Formatting.None);
    log.LogInformation("Event Time: "+eventTime);
    //Call maximo api to get resouce uri
    //Change {your maximo server:port} section to your maximo instance server name and port
    var baseUrl = "http://{your maximo server:port}/maximo/oslc/os/mxasset";    
    HttpClient client = new HttpClient();
    //Change {your maxauth base64 passcode} to your maximo account and password base64 string
    client.DefaultRequestHeaders.Add("maxauth","{your maxauth base64 passcode}");
    var apimUrl=baseUrl+"?lean=1&oslc.where=assetnum=\""+deviceID+"\"";
    log.LogInformation($"Get Url: {apimUrl}");
   
    try
    {

        //The following code is to query Asset from Maximo nextgen rest API 
        //to get resource ID by assetnum (DeviceID in the event message body),
        // which can be used to post meter reading to Maximo through nextgen Rest API.
        using HttpResponseMessage result =await client.GetAsync(apimUrl);
        result.EnsureSuccessStatusCode();
        var content = await result.Content.ReadAsStringAsync();
        log.LogInformation($"Get Request result: {content}");         
        JObject jsonResult = JObject.Parse(content);        
        JToken tokenMember = jsonResult.SelectToken("member");    
        String strMembers= tokenMember?.ToString();
        log.LogInformation($"Query Member result: {strMembers}");         
        JArray jArrayResult= JArray.Parse(strMembers);
        if(jArrayResult is null || jArrayResult.Count==0)
        {
            log.LogError("\nNo result was found for Asset Num:"+deviceID);  
        } 
        else
        {
            
            JObject jObjecturl = jArrayResult.First.ToObject<JObject>();
            JToken tokenUrl=jObjecturl.SelectToken("href");
            log.LogInformation("Found Asset URL:"+tokenUrl?.ToString()); 
            String assetUrl=tokenUrl?.ToString();
            if(assetUrl.Length>0)
            {
                String[] strList=assetUrl.Split(new string[] { "/" }, StringSplitOptions.None);
                String resourceID=strList[strList.Length-1];
                log.LogInformation($"ResourceID:{resourceID}"); 
                //Post meter reading to Maximo
                dynamic jObjectMeter=new JObject();
                jObjectMeter.metername=meterID;
                jObjectMeter.linearassetmeterid=0;
                jObjectMeter.newreading=meterReading;
                jObjectMeter.newreadingdate=eventTime.Substring(1,19)+"+00:00";
                dynamic jObjectAsset=new JObject();
                dynamic jArrayMeters=new JArray();
                jArrayMeters.Add(jObjectMeter);
                jObjectAsset.assetmeter=jArrayMeters;
                String postJson=JsonConvert.SerializeObject(jObjectAsset);
                log.LogInformation($"Post Json Content: {postJson}");
                using (var postContent = new StringContent(postJson, System.Text.Encoding.UTF8, "application/json"))
                    {
                        var postUrl=baseUrl+"/"+resourceID+"?lean=1";
                        log.LogInformation($"Post Url: {postUrl}");
                        //add header for Maximo nextgen rest API update scenario 
                        client.DefaultRequestHeaders.Add("x-method-override","PATCH");
                        client.DefaultRequestHeaders.Add("patchtype","MERGE");                       
                        HttpResponseMessage resultPost =await client.PostAsync(postUrl, postContent);
                        resultPost.EnsureSuccessStatusCode();
                        var postResultContent = await resultPost.Content.ReadAsStringAsync();
                        log.LogInformation($"Post request result: {postResultContent}");
                    }
            }
            else
            {
                log.LogError($"Error: Asset Num:{deviceID} url is empty");  
            }
        
        } 

    }
    catch (HttpRequestException e)
    {
        log.LogError("\nHttp Exception Caught!");
        log.LogError("Message :{0} ", e.ToString());
    }
    catch (Exception e)
    {
        log.LogError("\nUnexpected Exception Caught!");
        log.LogError("Message :{0} ", e.ToString());
    }
    

}
