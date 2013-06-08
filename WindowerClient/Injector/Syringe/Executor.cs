using System;
using System.Collections.Generic;
using System.Text;
using Syringe;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Syringe.Win32;

namespace Syringe
{

    public class Executor
    {
        Injector syringe;

        [StructLayout(LayoutKind.Sequential)]
        public struct MessageStruct
        {
            [CustomMarshalAs(CustomUnmanagedType.LPStr)]
            public string Text;
            [CustomMarshalAs(CustomUnmanagedType.LPStr)]
            public string Caption;
        }

        public Injector getSyringe()
        {
            return syringe;
        }

        public void Inject(String dll, String process)
        {
            
            //String dll = "C:\\Users\\emist\\Documents\\Visual Studio 2008\\Projects\\InjectDLL\\Debug\\InjectDLL.dll";

            Console.WriteLine("Trying to inject " + dll + " into " + process);
            MessageStruct messageData = new MessageStruct() { Text = "Custom Message", Caption = "Custom Message Box" };
            Process[] processes = Process.GetProcessesByName(process);
            Array.Sort(processes, delegate(Process x, Process y) { return -1 * (x.StartTime.CompareTo(y.StartTime)); });
            syringe = new Injector(processes[0]);
            syringe.InjectLibrary(dll);
            Console.WriteLine(dll + " injected into " + process);
         
        }
    }

}
