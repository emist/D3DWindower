using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using EasyHook;

namespace D3DHookingLibrary
{
    public static class Hooking
    {
        //public int pid;
        //public string hookExe, hookDLL;

        /*
        public Hooking(int pid, string hookExe, string hookDLL)
        {
            this.hookExe = hookExe;
            this.hookDLL = hookDLL;
            this.pid = pid;
        }
        */

        public static void hook(int pid, string descriptor, string hookExe, string hookDLL32, string hookDLL64, params object[] parameters)
        {
            try
            {
                
                Config.Register(descriptor, hookDLL32, hookExe);

                if(!hookDLL32.ToLower().Equals(hookDLL64.ToLower()))
                    Config.Register(descriptor, hookDLL32, hookExe);

                Console.WriteLine("Registered!");

                /*
                EasyHook.RemoteHooking.IpcCreateServer<injectorInterface.GetType()>(ref
                ChannelName, WellKnownObjectMode.SingleCall);
                */

                EasyHook.RemoteHooking.Inject(pid, hookDLL32, hookDLL64, parameters);

                Console.ReadLine();
            }
            catch (Exception ExtInfo)
            {
                Console.WriteLine("There was an error while connecting to target:\r\n{0}", ExtInfo.ToString());
            }
        }
    }
}
