using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Syringe;
using System.Runtime.InteropServices;

namespace Tester
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        struct MessageStruct
        {
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string Text;
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string Caption;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Trying to inject dll into notepad.exe");
            MessageStruct messageData = new MessageStruct() { Text = "Custom Message", Caption = "Custom Message Box" };
            using (Injector syringe = new Injector(Process.GetProcessesByName("notepad")[0]))
            {
                syringe.InjectLibrary("Stub.dll");

                Console.WriteLine("Stub.dll injected into notepad, trying to call void Initialise() with no parameters");
                Console.ReadLine();
                syringe.CallExport("Stub.dll", "Initialise");
                Console.WriteLine("Waiting...");
                Console.ReadLine();
                Console.WriteLine("Trying to call InitWithMessage( PVOID ) with custom data {0}", messageData);
                Console.ReadLine();
                syringe.CallExport("Stub.dll", "InitWithMessage", messageData);
                Console.WriteLine("Waiting...");
                Console.ReadLine();
            }
            Console.WriteLine("Stub.dll should be ejected");
            Console.ReadLine();
        }
    }
}
