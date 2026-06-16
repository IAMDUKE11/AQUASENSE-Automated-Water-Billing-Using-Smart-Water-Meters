#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

#define SENSOR_PIN 14
#define BUZZER_PIN 12
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
#define OLED_RESET -1

Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, OLED_RESET);

volatile int pulseCount     = 0;
float calibrationFactor     = 7.5;
float flowRate              = 0.0;
float totalLiters           = 0.0;
float totalBill             = 0.0;
const float PRICE_PER_LITER = 0.001;

unsigned long lastMillis     = 0;
unsigned long lastFlowMillis = 0;
bool sessionActive           = false;
const unsigned long HOLD_MS  = 60000;

void ICACHE_RAM_ATTR pulseCounter()
{
  pulseCount++;
}

void setup()
{
  Serial.begin(115200);
  pinMode(SENSOR_PIN, INPUT_PULLUP);
  pinMode(BUZZER_PIN, OUTPUT);
  digitalWrite(BUZZER_PIN, LOW);

  for (int i = 0; i < 2; i++)
  {
    digitalWrite(BUZZER_PIN, HIGH); delay(80);
    digitalWrite(BUZZER_PIN, LOW);  delay(80);
  }

  attachInterrupt(digitalPinToInterrupt(SENSOR_PIN), pulseCounter, FALLING);

  if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C)) { while (true); }

  display.clearDisplay();
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(25, 10); display.println("AQUASENSE v1.0");
  display.drawLine(20, 22, 108, 22, WHITE);
  display.setCursor(20, 35); display.println("SYSTEM CHECK");
  display.setCursor(25, 50); display.println("STATUS: OK");
  display.display();
  delay(3000);

  lastMillis = millis();
}

void showIdle()
{
  display.clearDisplay();
  display.setTextSize(1);
  display.setCursor(20, 10); display.println("AQUASENSE v1.0");
  display.drawLine(0, 22, 128, 22, WHITE);
  display.setCursor(30, 35); display.println("WAITING FOR");
  display.setCursor(35, 48); display.println("WATER FLOW");
  display.display();
}

void showFlow()
{
  display.clearDisplay();
  display.setTextSize(1);

  display.setCursor(0, 0);
  display.print("FLOW: ");
  display.print(flowRate, 2);
  display.print(" L/min");

  display.drawLine(0, 14, 128, 14, WHITE);

  display.setCursor(0, 18);
  display.print("TOTAL: ");
  display.print(totalLiters, 3);
  display.print(" L");

  display.drawLine(0, 32, 128, 32, WHITE);

  display.setCursor(0, 38);
  display.print("BILL: $");
  display.print(totalBill, 6);

  display.display();
}

void loop()
{
  unsigned long now = millis();

  if ((now - lastMillis) >= 1000)
  {
    unsigned long elapsed = now - lastMillis;
    lastMillis = now;

    detachInterrupt(digitalPinToInterrupt(SENSOR_PIN));
    int pulses = pulseCount;
    pulseCount = 0;
    attachInterrupt(digitalPinToInterrupt(SENSOR_PIN), pulseCounter, FALLING);

    flowRate = (pulses / calibrationFactor);

    if (flowRate > 0)
    {
      totalLiters   += flowRate / 60.0;
      totalBill      = totalLiters * PRICE_PER_LITER;
      lastFlowMillis = now;
      sessionActive  = true;

      showFlow();

      Serial.print("DATA|");
      Serial.print(flowRate, 4);
      Serial.print("|");
      Serial.print(totalLiters, 6);
      Serial.print("|");
      Serial.print(totalBill, 6);
      Serial.println("|END");
    }
    else
    {
      if (sessionActive)
      {
        unsigned long timeSinceFlow = millis() - lastFlowMillis;

        if (timeSinceFlow < HOLD_MS)
        {
          showFlow();
        }
        else
        {
          Serial.print("SESSION_END|");
          Serial.print(flowRate, 4);
          Serial.print("|");
          Serial.print(totalLiters, 6);
          Serial.print("|");
          Serial.print(totalBill, 6);
          Serial.println("|END");

          totalLiters   = 0.0;
          totalBill     = 0.0;
          sessionActive = false;

          digitalWrite(BUZZER_PIN, HIGH); delay(200);
          digitalWrite(BUZZER_PIN, LOW);

          showIdle();
        }
      }
      else
      {
        showIdle();
      }
    }
  }
}