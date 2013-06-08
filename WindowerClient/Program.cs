using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syringe;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices;

using D3DHookingLibrary;

namespace DLLTestUtil
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MessageStruct
        {
            public IntPtr reset_address;
            public int width;
            public int height;
        }

       
        static void Main(string[] args)
        {
            Executor injector = new Executor();
            string dllName = "D3DWindower.dll";
            injector.Inject(dllName, args[0].Substring(0, args[0].Length-4));

            D3DFuncLookup d3d9Util = new D3DHookingLibrary.D3DFuncLookup(args[0], args[1]);
            
            int input_width = Convert.ToInt32(args[2]);
            int input_height = Convert.ToInt32(args[3]);

            IntPtr resetAddress = (IntPtr)d3d9Util.GetD3DFunction(D3DFuncLookup.D3D9Functions.Reset);
            MessageStruct mes = new MessageStruct() { reset_address = resetAddress, height = input_height, width = input_width };
            injector.getSyringe().CallExport(dllName, "InstallHook", mes);
            while (true) Thread.Sleep(300);
        }

    }
}
