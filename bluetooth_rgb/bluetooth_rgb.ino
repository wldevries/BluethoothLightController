#include <SoftwareSerial.h>

SoftwareSerial bluetooth(10, 11); // RX, TX

const int RedPin = 5;
const int GreenPin = 9;
const int BluePin = 3;

void setup() {
  // put your setup code here, to run once:
  analogWrite (RedPin, 0);
  analogWrite (GreenPin, 0);
  analogWrite (BluePin, 0);

  Serial.begin(9600);
  bluetooth.begin(9600);
}

#define bufferLen 16
char buffer[bufferLen];

// komt van https://hackingmajenkoblog.wordpress.com/2016/02/01/reading-serial-on-the-arduino/
int readline(int readch, char *buffer, int len) {
  static int pos = 0;
  int rpos;

  if (readch > 0) {
    switch (readch) {
      case '\n': // Ignore new-lines
        break;
      case '\r': // Return on CR
        rpos = pos;
        pos = 0;  // Reset position index ready for next time
        return rpos;
      default:
        if (pos < len-1) {
          buffer[pos++] = readch;
          buffer[pos] = 0;
        }
    }
  }
  // No end of line has been found, so return -1.
  return -1;
}

void loop() {
  if (bluetooth.available() > 0)
  {
    char recv = bluetooth.read();
    int lineLength = readline(recv, buffer, bufferLen) > 0;
    if (lineLength) {
      int value = atoi(buffer + 2);
      //value = value / 20;
      switch (buffer[0]) {
        case 'R':
        case 'r':
          analogWrite(RedPin, value);
        case 'G':
        case 'g':
          analogWrite(GreenPin, value);
        case 'B':
        case 'b':
          analogWrite(BluePin, value);
      }
    }
  }
}
