#include <Wire.h>
#include <Console.h>
#include <Adafruit_MPL3115A2.h>
#include <Bridge.h>
#include <BridgeServer.h>
#include <BridgeClient.h>

Adafruit_MPL3115A2 baro = Adafruit_MPL3115A2();

BridgeServer server;

void setup() 
{
  Bridge.begin();
  Console.begin();

  pinMode(13, OUTPUT);
  pinMode(10, OUTPUT);
  
  digitalWrite(13, LOW);
  Bridge.begin();
  digitalWrite(13, HIGH);
  
  digitalWrite(10, HIGH);
  delay(250);
  digitalWrite(10, LOW);
  
  server.listenOnLocalhost();
  server.begin();
  Console.println("Setup");
}
void loop() 
{
   BridgeClient client = server.accept();
  if (client) 
  {
    Process_Client(client);
    client.stop();
  }
  if (! baro.begin())
  {
    Console.println("Couldnt find sensor");
    return;
  }
  delay(250);
}

void Process_Client(BridgeClient client)
{
  String command = client.readStringUntil('/');
  if (command == "hello")
  {
    HelloWorld(client);
  }
  if (command == "digital") 
  {
    digitalCommand(client);
  }
}

void HelloWorld(BridgeClient client)
{
  float pascals = baro.getPressure();
  float altm = baro.getAltitude();
  float tempC = baro.getTemperature();

  int state = client.parseInt();
  if(state == 1)
  {
    client.print("{\"temp\": \""+ String(tempC) +"\",\"pression\": \""+ String(pascals) +"\",\"altitude\": \""+ String(altm) +"\"}");
  }
  else
  {
    client.print("motherfucker");
  }
}

void digitalCommand(BridgeClient client) {
  int pin, value;
  pin = client.parseInt();

  if (client.read() == '/') 
  {
    value = client.parseInt();
    digitalWrite(pin, value);
  } 
  else 
  {
    value = digitalRead(pin);
  }
  client.print(F("Pin D"));
  client.print(pin);
  client.print(F(" set to "));
  client.println(value);
  
  String key = "D";
  key += pin;
  Bridge.put(key, String(value));
}

