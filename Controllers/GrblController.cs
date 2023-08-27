using Microsoft.AspNetCore.Mvc;
using System.IO.Ports;

namespace ServeGrbl.Controllers
{
    [Route("api/grbl")]
    [ApiController]
    public class GrblController : ControllerBase
    {
        private const string SerialPortName = "COM4";
        private const int BaudRate = 115200;
        public bool readNextLine = true;
        static SerialPort serialPort;
        private int LINEnUMBER = 0;
        private static int CurrentLine = 0;
        public static bool NotPused = true;
        static int i = 0;
        static double precentage;
        public static bool Stoped = false;

        static List<string> lines = new List<string>();

        [HttpPost]
        public async Task<IActionResult> SendCommand([FromBody] string command)
        {
            try
            {
                using (serialPort = new SerialPort(SerialPortName, BaudRate))
                {
                    serialPort.Open();
                    serialPort.WriteLine(command);
                    await Task.Delay(100);
                    string response = serialPort.ReadExisting();
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
            if (!Stoped)
            {
                NotPused = false;
                Console.WriteLine("Program Paused by user  At line " + i);
                return Ok("Program Paused by user  At line " + i);
            }
            else
            {
                Console.WriteLine("Start First, Your Work is Stopped");
                return Ok("Start First, Your Work is Stopped");
            }
        }

        [HttpPost("Stop")]
        public async Task<IActionResult> Stop()
        {
            Stoped = true;
            return Ok("Program Stoped");
        }

        [HttpPost("Continue")]
        public async Task<IActionResult> Continue()
        {
            if (!Stoped)
            {
                NotPused = true;
                serialPort = new SerialPort(SerialPortName, BaudRate);
                Console.WriteLine("Program Continued by user  From line " + CurrentLine++);
                ProcessLine(serialPort);
                return Ok("Program Continued by user  From line " + CurrentLine++);
            }
            else
            {
                Console.WriteLine("Cannot Be Continued, Program Has been Stopped ");
                return Ok("Cannot Be Continued, Program Has been Stopped");
            }
        }


        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                serialPort = new SerialPort(SerialPortName, BaudRate);
                while (!reader.EndOfStream && readNextLine)
                {
                    string line = await reader.ReadLineAsync();
                    lines.Add(line);
                    LINEnUMBER++;
                }
                ProcessLine(serialPort);

                return Ok("File content processed successfully.");
            }
        }

        [HttpGet("percentage")]
        public async Task<IActionResult> GetPercentage()
        {
            return Ok(precentage);
        }

        async void ProcessLine(SerialPort serialPort)
        {
            List<string> lines2 = lines.GetRange(CurrentLine, (lines.Count) - CurrentLine);
            foreach (var line in lines2)
            {
                if (NotPused && !Stoped)
                {
                    try
                    {
                        if (!serialPort.IsOpen)
                        {
                            serialPort.Open();
                        }
                        if (NotPused && !Stoped)
                        {
                            serialPort.WriteLine(line);
                            var response = serialPort.ReadLine();
                            response = response.Trim();
                            Console.WriteLine(response + " " + i);
                            double cnt = lines2.Count;
                            precentage=((double)i/ cnt) *100;
                            i++;

                            if (response.StartsWith("error") || response.StartsWith("Alarm"))
                            {
                                CurrentLine = i;
                                break;
                            }
                            ;
                        }
                    }
                    catch (Exception ex) { }
                }
                else if (!NotPused)
                {
                    CurrentLine = i;
                    serialPort.Close();
                    return;
                }
                else if (Stoped)
                {
                    CurrentLine = 0;
                    Console.WriteLine("Program Stopped by user  At line " + i);
                    serialPort.Close();

                    return;
                }
            }
        }
    }
}
