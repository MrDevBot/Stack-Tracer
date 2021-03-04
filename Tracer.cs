using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

//Source https://github.com/MrDevBot/Stack-Tracer
//Last update @ Friday, 5th March 2021 1:09 AM

namespace Server
{
    internal class Debugger
    {
        private static string WriteLineBuffer = "";
        //the buffer (message queue) is used to we dont get events / messages half way through a stack trace.
        private static bool BufferState = false;

        public static void Initilise()
        {
            try
            {
                ConsoleTraceListener listener = new ConsoleTraceListener();
                Trace.Listeners.Add(listener);
                Trace.TraceInformation("[POST] Added \"Debugger\" trace listner, now listening for events through Post(int type, string Message)");
            }
            catch(Exception ex)
            {
                //invoke MessageBox.Show() on new thread to prevent continue execution of primary thread (since we cant modify its method, and it force suspends)
                var thread = new Thread(
                () => {  System.Windows.Forms.MessageBox.Show("Failed to attach trace listener! now self terminating process");  });
                thread.Start();

                Process[] processCollection = Process.GetProcesses();
                foreach (Process Index in processCollection)
                {
                    Index.Kill();
                }
            }
        }

        public static void Post(int type, string Message)
        {
            string Timestamp = DateTime.UtcNow.ToString();
            StackTrace stackTrace = new StackTrace();
            string Class = stackTrace.GetFrame(1).GetMethod().Name;

            if (!BufferState)
            {
                #region Formatter
                switch (type)
                {
                    case 0: //writes directly to console, may not always exist.
                        Console.WriteLine("[DEBUGGER] " + " [TIMESTAMP] " + Timestamp + " [TYPE] " + type.ToString() + " [MESSAGE] " + Message + " [CLASS] " + Class);
                        break;
                    case 1: //writes directly to trace listener
                        Trace.WriteLine("[DEBUGGER] " + " [TIMESTAMP] " + Timestamp + " [TYPE] " + type.ToString() + " [MESSAGE] " + Message + " [CLASS] " + Class);
                        break;
                    case 2: //output to trace listener as "infomational message"
                        Trace.TraceInformation("[DEBUGGER] " + " [TIMESTAMP] " + Timestamp + " [TYPE] " + type.ToString() + " [MESSAGE] " + Message + " [CLASS] " + Class);
                        break;
                    case 3: //output to CSV
                        if(!File.Exists(@"outfile.dbg")) { File.WriteAllText(@"outfile.dbg", Timestamp + ',' + type.ToString() + ',' + Message + ',' + Class); };
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                //queue message to the buffer
                WriteLineBuffer = WriteLineBuffer + "[DEBUGGER] " + "[TIMESTAMP] " + Timestamp + "[TYPE] " + type.ToString() + "[MESSAGE] " + Message + "[CLASS] " + Class + Environment.NewLine;
            }

            #endregion Formatter

            GC.Collect();
        }

        private static void FlushBuffer()
        {
            Console.WriteLine(WriteLineBuffer); //print buffer
            WriteLineBuffer = ""; //clear buffer
        }

        public static void LiveTracer()
        {
            //register WriteLine() buffer
            BufferState = true;

            Console.ForegroundColor = ConsoleColor.Red;
            string Header = "▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓";

            Console.WriteLine(Header);

            StackTrace stackTrace = new StackTrace();
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                StackFrame sf = stackTrace.GetFrame(i);
                Console.WriteLine();
                Console.WriteLine("Method: {0}", sf.GetMethod());

                Console.WriteLine("Line Number: {0}", sf.GetFileLineNumber());

                if (sf.GetILOffset() != StackFrame.OFFSET_UNKNOWN)
                {
                    Console.WriteLine("In MSIL Region: {0}",
                       sf.GetILOffset());
                }
                if (sf.GetNativeOffset() != StackFrame.OFFSET_UNKNOWN)
                {
                    Console.WriteLine("In Native Region: {0}",
                       sf.GetNativeOffset());
                }
            }

            Console.WriteLine(Header);

            Console.ForegroundColor = ConsoleColor.Black;

            //Deregister buffer to prepare for buffer flush
            BufferState = false;
            FlushBuffer();
        }
    }
}
