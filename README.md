# AzureIoTMaximoIntegration
Azure IoT integration with Maximo exmaple
IBM Maximo is the major player in EAM market, and Azure IoT is the most popular IoT platform, In this POC, I would introduce how to integrate Azure IoT platform with Maximo through Maximo NextGen Rest API. The purpose of this POC is to demonstrate sending IoT device sensor data (for example, temperature data) to Azure IoT hub, through Azure Event Grid to pass telemetry to Azure Function. In Azure Function, call Maximo NexGen Rest API to post telemetry data to Maximo Asset meter reading. The following is the high level of the architecture diagram:
![alt text](https://miro.medium.com/v2/resize:fit:1100/format:webp/1*nWfVeFLfhBx3q-ZUc6WdZA.png)

You can following the below blog to have hand-on experience on building Azure IoT solution integrating with Maximo. 

https://jackyqiubao.medium.com/azure-iot-integration-with-maximo-part-1-b141c4210db0?sk=5345c390bd704aff8349231bbef7ec7d

This repository contains the code for the blog. The AzureFunction folder contains the code for building Azure function section. The IoTDeviceSimulator contains the code for building local IoT device simulator to connect and send ramdom temperature data to Azure IoT hub.
