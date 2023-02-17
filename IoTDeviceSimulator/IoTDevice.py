# -------------------------------------------------------------------------
# Copyright (c) Jacky Qiu. All rights reserved.
# Licensed under the MIT License. 
# --------------------------------------------------------------------------
import asyncio
import time
import uuid
import random
import json
from azure.iot.device.aio import IoTHubDeviceClient
from azure.iot.device import Message


async def send_recurring_telemetry(device_client):
    # Connect the client.
    await device_client.connect()

    # Send recurring telemetry
    i = 0
    while True:
        i += 1
        curr_temp= random.randrange(10, 50)
        temperature_msg = {"temperature": curr_temp,"metername":"TEMP-C"}
        msg = Message(json.dumps(temperature_msg))
        msg.message_id = uuid.uuid4()
        msg.correlation_id = "correlation-1234"
        msg.content_encoding = "utf-8"
        msg.content_type = "application/json"
        print("sending message #" + str(i) + ", temperature:"+str(curr_temp))
        await device_client.send_message(msg)
        time.sleep(10)


def main():
    # The connection string for a device, you can get it from your IOT Hub -> Devices
    #conn_str = "{Your Device Connection String}"

    # The client object is used to interact with your Azure IoT hub.
    #device_client = IoTHubDeviceClient.create_from_connection_string(conn_str)

    print("IoTHub Device Client Recurring Telemetry Sample")
    print("Press Ctrl+C to exit")
    loop = asyncio.get_event_loop()
    try:
        loop.run_until_complete(send_recurring_telemetry(device_client))
    except KeyboardInterrupt:
        print("User initiated exit")
    except Exception:
        print("Unexpected exception!")
        raise
    finally:
        loop.run_until_complete(device_client.shutdown())
        loop.close()


if __name__ == "__main__":
    main()
