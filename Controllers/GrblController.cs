﻿using Microsoft.AspNetCore.Mvc;
using System.Device.Gpio;
using rpi_ws281x;
using System.Threading;
using System.Drawing;
using System.IO.Ports;

namespace ServeGrbl.Controllers
{
    [Route("api/grbl")]
    [ApiController]
    public class GrblController : ControllerBase
    {
        private static string SerialPortName = "/dev/ttyS0";
        private static string SerialPortName2 = "/dev/ttyUSB0";

        static int baudRate = 115200;
        static Parity parity = Parity.None;
        static int dataBits = 8;
        static StopBits stopBits = StopBits.One;
        private const int BaudRate = 115200;
        public bool readNextLine = true;
        static SerialPort serialPort;
        static SerialPort serialPort1;

        private int LINEnUMBER = 0;
        private static int CurrentLine = 0;
        private static int sensorPin = 21;
        private static int testpin = 20;
        public static bool NotPused = true;
        public static bool IRSencorOK = true;
        static int i = 0;
        static double precentage;
        public static bool Stoped = false;
        static GpioController controller = new GpioController();



        static List<string> lines = new List<string>();

        public static bool processing { get; private set; }

        public GrblController()
        {
            processing = false;
            SetColor(Color.White);
        }

        [HttpPost]
        public async Task<IActionResult> SendCommand([FromBody] string command)
        {
            try
            {

                using (SerialPort serialPort = new SerialPort(SerialPortName, baudRate, parity, dataBits, stopBits))
                {
                    serialPort.Open();

                    Console.WriteLine(command);
                    serialPort.WriteLine(command);
                    await Task.Delay(100);
                    string response = serialPort.ReadExisting();
                    Console.WriteLine(response);
                    return Ok(response);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending command: {ex.Message}");
            }
        }

        [HttpPost("Pause")]
        public async Task<IActionResult> Pause()
        {
            if (!Stoped && processing)
            {
                NotPused = false;
                SetColor(Color.RosyBrown);
                Console.WriteLine("Program Paused by user  At line " + i);
                return Ok("Program Paused by user  At line " + i);
            }
            else
            {
                Console.WriteLine("Start First, Your Work is Stopped");
                return Ok("Start First, Your Work is Stopped");
            }
        }
        [HttpPost("SelectPort")]
        public async Task<IActionResult> SelectPort([FromBody] string command)
        {
            SerialPortName = command.Trim();
            return Ok(SerialPortName);
        }

        [HttpPost("Stop")]
        public async Task<IActionResult> Stop()
        {
            if (processing)
            {
                Stoped = true;
                SetColor(Color.Green);
                return Ok("Program Stoped");

            }
            return Ok("Program Hasn't been started Yet");
        }

        [HttpPost("Continue")]
        public async Task<IActionResult> Continue()
        {
            SetColor(Color.Green);

            if (!Stoped)
            {
                NotPused = true;
                serialPort = new SerialPort(SerialPortName, baudRate, parity, dataBits, stopBits);
                Console.WriteLine("Program Continued by user  From line " + i++);
                ProcessLine(serialPort);
                return Ok("Program Continued by user  From line " + i);
            }
            else
            {
                Console.WriteLine("Cannot Be Continued, Program Has been Stopped ");
                return Ok("Cannot Be Continued, Program Has been Stopped");
            }
        }

        [HttpPost("LevelIt")]
        public async Task<IActionResult> Level()
        {
            if (!controller.IsPinOpen(20))
            {
                controller.OpenPin(20, PinMode.Input);

            }
            var status = PinValue.High;
            while (status == PinValue.High)
            {
                using (SerialPort serialPort = new SerialPort(SerialPortName, baudRate, parity, dataBits, stopBits))
                {
                    serialPort.Open();
                    status = controller.Read(20);
                    Console.WriteLine($"{status}");
                    serialPort.WriteLine("$J=G91 G21 Z-0.5 F100");
                    Console.WriteLine("Leveling $J=G91 G21 Z-0.5 F100");
                    Thread.Sleep(100);
                    for (int j = 0; j < 100; j++)
                    {
                        status = controller.Read(20);
                    }
                    status = controller.Read(20);
                    serialPort.Close();
                }
            }
            Console.WriteLine("'Leveled");

            controller.ClosePin(20);
            return Ok();
        }
        [HttpGet("Status")]
        public async Task<IActionResult> getStatus()
        {
            if (!controller.IsPinOpen(sensorPin))
            {
                controller.OpenPin(sensorPin, PinMode.Input);
            }


            var status = controller.Read(sensorPin);


            controller.ClosePin(sensorPin);
            return Ok(status == PinValue.Low ? "1" : "0");
        }


        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            precentage = 0;
            SetColor(Color.Blue);
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                serialPort = new SerialPort(SerialPortName, baudRate, parity, dataBits, stopBits);
                while (!reader.EndOfStream && readNextLine)
                {
                    string line = await reader.ReadLineAsync();
                    lines.Add(line);
                    LINEnUMBER++;
                }
                if (Stoped)
                {
                    Stoped = false;
                }
                if (NotPused == false)
                {
                    NotPused = true;
                }
                Console.WriteLine("The File Will Be started From Line 0");


                ProcessLine(serialPort);

                return Ok("File content processed successfully.");
            }
        }

        [HttpGet("percentage")]
        public async Task<IActionResult> GetPercentage()
        {
            if (precentage >= 100)
            {
                precentage = 100;
            }
            return Ok(precentage);
        }

        private async Task SendSMS(string msg)
        {
            serialPort1 = new SerialPort(SerialPortName2, 9600, parity, dataBits, stopBits);
            serialPort1.Open();

            // Clear any data in the receive buffer
            serialPort1.DiscardInBuffer();

            // Send AT command to set up SMS mode
            serialPort1.WriteLine("AT+CMGF=1");
            Thread.Sleep(1000);

            // Send AT command to set the recipient phone number
            serialPort1.WriteLine("AT+CMGS=\"+970592935116\"");
            Thread.Sleep(1000);

            // Send the message content
            serialPort1.Write($"{msg}.\x1A");
            Thread.Sleep(1000);

            // Close the serial port
            serialPort1.Close();


        }
        void SetColor(Color color)
        {
            var settings = Settings.CreateDefaultSettings();

            settings.Channels[0] = new Channel(29, 18, 255, false, StripType.WS2812_STRIP);
            using (var rpi = new WS281x(settings))
            {


                // Set the LED colors.
                for (int i = 0; i < 29; i++)
                {
                    rpi.SetLEDColor(0, i, color);
                }

                // Render the LED colors.
                rpi.Render();

                // Sleep for a few seconds to display the colors.
                Thread.Sleep(5000);
            }
        }
        async void Read_IR()
        {
            List<PinValue> pins = new List<PinValue>();
            for (int i = 0; i < 3; i++)
            {
                pins.Add(PinValue.Low);
            }

            if (controller.IsPinOpen(26))
            {
                pins[0] = controller.Read(26);
            }
            if (controller.IsPinOpen(19))
            {
                pins[1] = controller.Read(19);
            }
            if (controller.IsPinOpen(19))
            {
                pins[2] = controller.Read(13);
            }
            if (pins.Any(x => x.Equals(PinValue.High)))
            {
                IRSencorOK = false;
            }
            else
            {
                IRSencorOK = true;
            }


        }

        async void ProcessLine(SerialPort serialPort)
        {

            List<string> lines2 = lines.GetRange(CurrentLine, (lines.Count) - CurrentLine);
            if (!controller.IsPinOpen(20))
            {
                controller.OpenPin(20, PinMode.Output);

            }
            Console.WriteLine("Starting Spindle");
            controller.Write(20, PinValue.Low);
controller.ClosePin(20);
var allowed=true;   
         foreach (var line in lines2)
            {
                Read_IR();

                if (NotPused && !Stoped)
                {
                    try
                    {
                        if (!serialPort.IsOpen)
                        {
                            serialPort.Open();

                        }
                        if (NotPused && !Stoped && IRSencorOK)
                        {
                            processing = true;
                            serialPort.WriteLine(line);
                            var response = serialPort.ReadLine();
                            response = response.Trim();
  if (response.StartsWith("ok")) {
      allowed = true;
  }
  else
  {
      allowed = false;
  }                            
Console.WriteLine(response + " " + i);
                            double cnt = lines2.Count;
                            precentage = ((double)i / cnt) * 100;
                            i++;
                            if (line.StartsWith("M30") || response.Contains("End"))
                            {
                                if (!controller.IsPinOpen(21))
                                {
                                    controller.OpenPin(21, PinMode.Output);

                                }
                                controller.Write(21, PinValue.Low);
                                Thread.Sleep(3000);
                                SetColor(Color.Blue);
                                Console.WriteLine("Program Finished");
                                SendSMS("YOUR PCB IS READY :");

                                if (serialPort.IsOpen)
                                {
                                    serialPort.Close();
                                }
                                Thread.Sleep(3000);
                                SetColor(Color.Green);


                                break;
                            }

                            if (response.StartsWith("error") || response.StartsWith("ALARM"))
                            {
                                CurrentLine = i;
                                SetColor(Color.Red);
                                SendSMS($"Machine Has Been Stoped :{response}");
                                if (!controller.IsPinOpen(20))
                                {
                                    controller.OpenPin(20, PinMode.Output);

                                }
                                controller.Write(20, PinValue.Low);
                                Thread.Sleep(300);
controller.ClosePin(20);

                                if (!controller.IsPinOpen(21))
                                {
                                    controller.OpenPin(21, PinMode.Output);

                                }
                                controller.Write(21, PinValue.Low);
                                Thread.Sleep(5000);
                                if (serialPort.IsOpen)
                                {
                                    serialPort.Close();
                                }

                                break;
                            }

                        }
                    }
                    catch (Exception ex) { }
                }
                else if (!NotPused)
                {
                    CurrentLine = i;
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    return;
                }
                else if (Stoped)
                {
                    CurrentLine = 0;
                    Console.WriteLine("Program Stopped by user  At line " + i);
                    if (!controller.IsPinOpen(20))
                    {
                        controller.OpenPin(20, PinMode.Output);

                    }
                    controller.Write(20, PinValue.Low);
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                    return;
                }
            }
            if (!controller.IsPinOpen(20))
            {
                controller.OpenPin(20, PinMode.Input);

            }
            controller.Write(20, PinValue.Low);
            controller.ClosePin(20);
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }

        }


    }
}
