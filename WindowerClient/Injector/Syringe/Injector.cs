using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;

using Syringe.Win32;


namespace Syringe
{
    public class Injector : IDisposable
    {
        /// <summary>
        /// Internal class to represent an Injected Module within Injector's target process.
        /// Simply manages finding export addresses for functions, and caches these addresses for
        /// potential future use.
        /// </summary>
        private class InjectedModule
        {
            private Dictionary<string, IntPtr> exports;

            public ProcessModule Module { get; private set; }
            public IntPtr BaseAddress { get { return Module.BaseAddress; } }

            public InjectedModule(ProcessModule module)
            {
                Module = module;
                exports = new Dictionary<string, IntPtr>();
            }

            /// <summary>
            /// Get the address of a given function exported in the Module.
            /// If the function can't be found, this will throw <see cref="Win32Exception"/>
            /// </summary>
            /// <param name="func">Name of function to search for</param>
            /// <returns><see cref="IntPtr"/> representing the address of this function in the target process</returns>
            /// <exception cref="Win32Exception">Thrown if unable to find function address in module</exception>
            public IntPtr this[string func]
            {
                get
                {
                    if (!exports.ContainsKey(func))
                        exports[func] = FindExport(func);
                    return exports[func];
                }
            }

            /**
             * Actual function to find export - loosely modelled off Cypher's idea/code for loading module into this
             * process to find address. Loads module as data, finds RVA of function and uses to find address in target 
             * process
             */
            private IntPtr FindExport(string func)
            {
                IntPtr hModule = IntPtr.Zero;
                try
                {
                    // Load module into local process address space
                    hModule = Imports.LoadLibraryEx(Module.FileName, IntPtr.Zero, LoadLibraryExFlags.DontResolveDllReferences);
                    if (hModule == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Call GetProcAddress to get the address of the function in the module locally
                    IntPtr pFunc = Imports.GetProcAddress(hModule, func);
                    if (pFunc == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // Get RVA of export and add to base address of injected module
                    // hack at the moment to deal with x64
                    bool x64 = IntPtr.Size == 8;
                    IntPtr pExportAddr;
                    if (x64)
                        pExportAddr = new IntPtr(Module.BaseAddress.ToInt64() + (pFunc.ToInt64() - hModule.ToInt64()));
                    else
                        pExportAddr = new IntPtr(Module.BaseAddress.ToInt32() + (pFunc.ToInt32() - hModule.ToInt32()));

                    return pExportAddr;
                }
                finally
                {
                    Imports.CloseHandle(hModule);
                }
            }
        }

        private Process _process;
        private IntPtr _handle;
        private Dictionary<string, InjectedModule> injectedModules;

        public Injector(Process process) : this(process, true) { }
        public Injector(Process process, bool ejectOnDispose)
        {
            if (process == null)
                throw new ArgumentNullException("process");
            if (process.Id == Process.GetCurrentProcess().Id)
                throw new InvalidOperationException("Cannot create an injector for the current process");

            Process.EnterDebugMode();

            _handle = Imports.OpenProcess(
                ProcessAccessFlags.QueryInformation | ProcessAccessFlags.CreateThread |
                ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMWrite |
                ProcessAccessFlags.VMRead, false, process.Id);

            if (_handle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            _process = process;
            EjectOnDispose = ejectOnDispose;
            injectedModules = new Dictionary<string, InjectedModule>();
        }

        public bool EjectOnDispose { get; set; }

        /// <summary>
        /// Injects a library into this Injector's process. <paramref name="libPath"/> can be 
        /// relative or absolute; either way, the injected module will be referred to by module name only.
        /// I.e. "c:\some\directory\library.dll", "library.dll" and "..\library.dll" will all be referred to
        /// as "library.dll"
        /// </summary>
        /// <param name="libPath">Relative or absolute path to the dll to be injected</param>
        public void InjectLibrary(string libPath)
        {
            // (in?)sanity check, pretty sure this is never possible as the constructor will error - left over from how it previously was developed
            if (_process == null)
                throw new InvalidOperationException("This injector has no associated process and thus cannot inject a library");
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("This injector does not have a valid handle to the associated process and thus cannot inject a library");

            if (!File.Exists(libPath))
                throw new FileNotFoundException(string.Format("Unable to find library {0} to inject into process {1}", libPath, _process.ProcessName), libPath);

            // convenience variables
            string fullPath = Path.GetFullPath(libPath);
            string libName = Path.GetFileName(fullPath);

            // declare resources that need to be freed in finally
            IntPtr pLibRemote = IntPtr.Zero; // pointer to allocated memory of lib path string
            IntPtr hThread = IntPtr.Zero; // handle to thread from CreateRemoteThread
            IntPtr pLibFullPathUnmanaged = Marshal.StringToHGlobalUni(fullPath); // unmanaged C-String pointer

            try
            {
                uint sizeUni = (uint)Encoding.Unicode.GetByteCount(fullPath);

                // Get Handle to Kernel32.dll and pointer to LoadLibraryW
                IntPtr hKernel32 = Imports.GetModuleHandle("Kernel32");
                if (hKernel32 == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                IntPtr hLoadLib = Imports.GetProcAddress(hKernel32, "LoadLibraryW");
                if (hLoadLib == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // allocate memory to the local process for libFullPath
                pLibRemote = Imports.VirtualAllocEx(_handle, IntPtr.Zero, sizeUni, AllocationType.Commit, MemoryProtection.ReadWrite);
                if (pLibRemote == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // write libFullPath to pLibPath
                int bytesWritten;
                if (!Imports.WriteProcessMemory(_handle, pLibRemote, pLibFullPathUnmanaged, sizeUni, out bytesWritten) || bytesWritten != (int)sizeUni)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // load dll via call to LoadLibrary using CreateRemoteThread
                hThread = Imports.CreateRemoteThread(_handle, IntPtr.Zero, 0, hLoadLib, pLibRemote, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (Imports.WaitForSingleObject(hThread, (uint)ThreadWaitValue.Infinite) != (uint)ThreadWaitValue.Object0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // get address of loaded module - this doesn't work in x64, so just iterate module list to find injected module
                IntPtr hLibModule;// = IntPtr.Zero;
                if (!Imports.GetExitCodeThread(hThread, out hLibModule))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (hLibModule == IntPtr.Zero)
                    throw new Exception("Code executed properly, but unable to get an appropriate module handle, possible Win32Exception", new Win32Exception(Marshal.GetLastWin32Error()));

                // iterate modules in target process to find our newly injected module
                ProcessModule modFound = null;
                foreach (ProcessModule mod in _process.Modules)
                {
                    if (mod.ModuleName == libName)
                    {
                        modFound = mod;
                        break;
                    }
                }
                if (modFound == null)
                    throw new Exception("Injected module could not be found within the target process!");

                injectedModules.Add(libName, new InjectedModule(modFound));
            }
            finally
            {
                Marshal.FreeHGlobal(pLibFullPathUnmanaged); // free unmanaged string
                Imports.CloseHandle(hThread); // close thread from CreateRemoteThread
                Imports.VirtualFreeEx(_process.Handle, pLibRemote, 0, AllocationType.Release); // Free memory allocated
            }
        }

        /// <summary>
        /// Ejects a library that this Injector has previously injected into the target process. <paramref name="libName"/> is the name of the module to
        /// eject, as per the name stored in <see cref="Injector.InjectLibrary"/>. Passing the same value as passed to InjectLibrary should always work unless a 
        /// relative path was used and the program's working directory has changed.
        /// </summary>
        /// <param name="libName">The name of the module to eject</param>
        public void EjectLibrary(string libName)
        {
            string libSearchName = File.Exists(libName) ? Path.GetFileName(Path.GetFullPath(libName)) : libName;

            if (!injectedModules.ContainsKey(libSearchName))
                throw new InvalidOperationException("That module has not been injected into the process and thus cannot be ejected");

            // resources that need to be freed
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // get handle to kernel32 and FreeLibrary
                IntPtr hKernel32 = Imports.GetModuleHandle("Kernel32");
                if (hKernel32 == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                IntPtr hFreeLib = Imports.GetProcAddress(hKernel32, "FreeLibrary");
                if (hFreeLib == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                hThread = Imports.CreateRemoteThread(_handle, IntPtr.Zero, 0, hFreeLib, injectedModules[libSearchName].BaseAddress, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (Imports.WaitForSingleObject(hThread, (uint)ThreadWaitValue.Infinite) != (uint)ThreadWaitValue.Object0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // get exit code of FreeLibrary
                IntPtr pFreeLibRet;// = IntPtr.Zero;
                if (!Imports.GetExitCodeThread(hThread, out pFreeLibRet))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                if (pFreeLibRet == IntPtr.Zero)
                    throw new Exception("FreeLibrary failed in remote process");
            }
            finally
            {
                Imports.CloseHandle(hThread);
            }
        }

        /// <summary>
        /// Call an export with no parameter in the target process (i.e. SomeFunc( void );). This function will only
        /// return once the remote thread in the target process has returned.
        /// </summary>
        /// <param name="libName">Name of the injected module in which the function should be found</param>
        /// <param name="funcName">Name of the exported function to call</param>
        /// <returns><see cref="IntPtr"/> representing the return value of function <paramref name="funcName"/> in module <paramref name="libName"/></returns>
        public IntPtr CallExport(string libName, string funcName)
        {
            return CallExport((uint)ThreadWaitValue.Infinite, libName, funcName);
        }

        /// <summary>
        /// Call an export with no parameter in the target process (i.e. SomeFunc( void );). This function returns after
        /// <paramref name="timeout"/> ms have elapsed, or the remote function finishes (whichever comes first).
        /// </summary>
        /// <param name="timeout">Number of miliseconds to wait for the remote function to return</param>
        /// <param name="libName">Name of the injected module in which the function should be found</param>
        /// <param name="funcName">Name of the exported function to call</param>
        /// <returns><see cref="IntPtr"/> representing the return value of function <paramref name="funcName"/> in module <paramref name="libName"/></returns>
        public IntPtr CallExport(uint timeout, string libName, string funcName) // param location to avoid possible overload / generic confusion
        {
            return CallExportInternal(timeout, libName, funcName, IntPtr.Zero, null, 0);
        }

        /// <summary>
        /// Call an export with type <typeparamref name="T"/> data parameter. <typeparamref name="T"/> must be a struct (i.e. a 
        /// value type or user-defined struct). User-defined structs must have the <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> set to
        /// <see cref="System.Runtime.InteropServices.LayoutKind.Sequential"/> or <see cref="System.Runtime.InteropServices.LayoutKind.Explicit"/>. User-defined
        /// structs containing variable length C-style strings should adorn strings with <see cref="CustomMarshalAsAttribute"/> and set the value to the appropriate
        /// <see cref="CustomUnmanagedType"/> value.
        /// This function will wait until the remote function finishes.
        /// </summary>
        /// <typeparam name="T">Type of data remote function expects. Value type or struct only.</typeparam>
        /// <param name="libName">Name of the injected module in which the function should be found</param>
        /// <param name="funcName">Name of the exported function to call</param>
        /// <param name="data">Data of type <typeparamref name="T"/> to be sent as argument to remote function</param>
        /// <returns><see cref="IntPtr"/> representing the return value of function <paramref name="funcName"/> in module <paramref name="libName"/></returns>
        public IntPtr CallExport<T>(string libName, string funcName, T data) where T : struct
        {
            return CallExport<T>((uint)ThreadWaitValue.Infinite, libName, funcName, data);
        }

        /// <summary>
        /// Call an export with type <typeparamref name="T"/> data parameter. <typeparamref name="T"/> must be a struct (i.e. a 
        /// value type or user-defined struct). User-defined structs must have the <see cref="System.Runtime.InteropServices.StructLayoutAttribute"/> set to
        /// <see cref="System.Runtime.InteropServices.LayoutKind.Sequential"/> or <see cref="System.Runtime.InteropServices.LayoutKind.Explicit"/>. User-defined
        /// structs containing variable length C-style strings should adorn strings with <see cref="CustomMarshalAsAttribute"/> and set the value to the appropriate
        /// <see cref="CustomUnmanagedType"/> value.
        /// This function will wait for <paramref name="timeout"/> miliseconds or until the remote function finishes (whichever comes first).
        /// </summary>
        /// <typeparam name="T">Type of data remote function expects. Value type or struct only.</typeparam>
        /// <param name="timeout">Number of miliseconds to wait for the remote function to return</param>
        /// <param name="libName">Name of the injected module in which the function should be found</param>
        /// <param name="funcName">Name of the exported function to call</param>
        /// <param name="data">Data of type <typeparamref name="T"/> to be sent as argument to remote function</param>
        /// <returns><see cref="IntPtr"/> representing the return value of function <paramref name="funcName"/> in module <paramref name="libName"/></returns>
        public IntPtr CallExport<T>(uint timeout, string libName, string funcName, T data) where T : struct
        {
            IntPtr pData = IntPtr.Zero;
            try
            {
                int dataSize = CustomMarshal.SizeOf(data);
                pData = Marshal.AllocHGlobal(dataSize);
                CustomMarshal.StructureToPtr(data, pData, true);
                return CallExportInternal(timeout, libName, funcName, pData, typeof(T), (uint)dataSize);
            }
            finally
            {
                Marshal.FreeHGlobal(pData);
            }
        }

        private IntPtr CallExportInternal(uint timeout, string libName, string funcName, IntPtr data, Type dataType, uint dataSize)
        {
            string libSearchName = File.Exists(libName) ? Path.GetFileName(Path.GetFullPath(libName)) : libName;

            if (!injectedModules.ContainsKey(libSearchName))
                throw new InvalidOperationException("That module has not been injected into the process and thus cannot be ejected");

            IntPtr pFunc = injectedModules[libSearchName][funcName];
            // resources that need to be cleaned
            IntPtr pDataRemote = IntPtr.Zero;
            IntPtr hThread = IntPtr.Zero;

            try
            {
                // check if we have all required parameters to pass a data parameter
                // if we don't, assume we aren't passing any data
                if (!(data == IntPtr.Zero || dataSize == 0 || dataType == null))
                {
                    // allocate memory in remote process for parameter
                    pDataRemote = Imports.VirtualAllocEx(_handle, IntPtr.Zero, dataSize, AllocationType.Commit, MemoryProtection.ReadWrite);
                    if (pDataRemote == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    // rebase the data so that pointers point to valid memory locations for the target process
                    // this renders the unmanaged structure useless in this process - should be able to re-rebase back to
                    // this target process by calling CustomMarshal.RebaseUnmanagedStructure(data, data, dataType); but not tested
                    CustomMarshal.RebaseUnmanagedStructure(data, pDataRemote, dataType);

                    int bytesWritten;
                    if (!Imports.WriteProcessMemory(_handle, pDataRemote, data, dataSize, out bytesWritten))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                hThread = Imports.CreateRemoteThread(_handle, IntPtr.Zero, 0, pFunc, pDataRemote, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                uint singleObject = Imports.WaitForSingleObject(hThread, timeout);
                if (!(singleObject == (uint)ThreadWaitValue.Object0 || singleObject == (uint)ThreadWaitValue.Timeout))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                IntPtr pRet;
                if (!Imports.GetExitCodeThread(hThread, out pRet))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                return pRet;
            }
            finally
            {
                Imports.VirtualFreeEx(_process.Handle, pDataRemote, 0, AllocationType.Release);
                Imports.CloseHandle(hThread);
            }

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (EjectOnDispose)
            {
                foreach (string key in injectedModules.Keys)
                {
                    EjectLibrary(key);
                }
            }
            if (_handle != IntPtr.Zero)
                Imports.CloseHandle(_handle);
            _handle = IntPtr.Zero;

            Process.LeaveDebugMode();
        }

        #endregion
    }
}
