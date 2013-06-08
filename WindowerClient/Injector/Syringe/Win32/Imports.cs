using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Syringe.Win32
{
    /// <summary>
    /// Static class containing all Win32 Import functions
    /// </summary>
    public static class Imports
    {
        #region Process

        /// <summary>
        /// Open process and retrieve handle for manipulation
        /// </summary>
        /// <param name="dwDesiredAccess"><see cref="ProcessAccessFlags"/> for external process.</param>
        /// <param name="bInheritHandle">Indicate whether to inherit a handle.</param>
        /// <param name="dwProcessId">Unique process ID of process to open</param>
        /// <returns>Returns a handle to opened process if successful or <see cref="IntPtr.Zero"/> if unsuccessful.
        /// Use <see cref="Marshal.GetLastWin32Error" /> to get Win32 Error on failure</returns>
        [DllImport("kernel32.dll", EntryPoint = "OpenProcess", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            ProcessAccessFlags dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] 
            bool bInheritHandle,
            int dwProcessId);

        /// <summary>
        /// Creates a new process and its primary thread. The new process runs in the security context of the calling process.
        /// </summary>
        /// <param name="lpApplicationName">The name of the module to be executed. The string can specify the full path and file name of hte module to execute
        /// or it can specify a partial name.</param>
        /// <param name="lpCommandLine">The command line to be executed.</param>
        /// <param name="lpProcessAttributes">A pointer to a SECURITY_ATTRIBUTES structure that determines whether the returned handle to the new process object can be inherited by child processes. If lpProcessAttributes is <see cref="IntPtr.Zero"/>, the handle cannot be inherited.</param>
        /// <param name="lpThreadAttributes">A pointer to a SECURITY_ATTRIBUTES structure that determines whether the returned handle to the new thread object can be inherited by child processes. If lpThreadAttributes is <see cref="IntPtr.Zero"/>, the handle cannot be inherited.</param>
        /// <param name="bInheritHandles">If this parameter is true, each inheritable handle in the calling process is inherited by the new process. If the parameter is FALSE, the handles are not inherited. Note that inherited handles have the same value and access rights as the original handles.</param>
        /// <param name="dwCreationFlags">The flags that control the priority class and the creation of the process. See <see cref="ProcessCreationFlags"/></param>
        /// <param name="lpEnvironment">A pointer to the environment block for the new process. If this parameter is <see cref="IntPtr.Zero"/>, the new process uses the environment of the calling process.</param>
        /// <param name="lpCurrentDirectory">The full path to the current directory for the process. The string can also specify a UNC path.</param>
        /// <param name="lpStartupInfo">A pointer to a <see cref="STARTUPINFO"/> structure.</param>
        /// <param name="lpProcessInformation">A pointer to a <see cref="PROCESS_INFORMATION"/> structure that receives identification information about the new process.</param>
        /// <returns>If the function succeeds, the return value is true. If the function fails, the return value is false. Call <see cref="Marshal.GetLastWin32Error"/> to get the Win32 Error.</returns>
        [DllImport("kernel32.dll", EntryPoint = "CreateProcessW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateProcess(
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            ProcessCreationFlags dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);



        #endregion

        #region Module

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="lpFileName">
        /// <para>The name of the module. This can be either a library module (.dll) or an executable module (.exe).</para>
        /// <para>If the string specifies a full path, the function searches only that path for the module. 
        /// Relative paths or files without a path will be searched for using standard strategies.</para>
        /// </param>
        /// <returns>If the function succeeds, a handle to the module is returned. 
        /// Otherwise, <see cref="IntPtr.Zero"/> is returned. Call <see cref="Marshal.GetLastWin32Error"/> on failure to get Win32 error.</returns>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="lpFileName"><para>The name of the module. This can be either a library module (.dll) or an executable module (.exe).</para>
        /// <para>If the string specifies a full path, the function searches only that path for the module. 
        /// Relative paths or files without a path will be searched for using standard strategies.</para></param>
        /// <param name="hFile">This parameter is reserved for future use. It must be NULL (<see cref="IntPtr.Zero"/>)</param>
        /// <param name="dwFlags">The action to be taken when loading the module. If no flags are specified, the behaviour is identical to <see cref="LoadLibrary"/>.
        /// The parameter can be one of the values defined in <see cref="LoadLibraryExFlags"/></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryExW", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(
            [MarshalAs(UnmanagedType.LPWStr)]
            string lpFileName,
            IntPtr hFile,
            LoadLibraryExFlags dwFlags);

        /// <summary>
        /// Frees the loaded Dll module.
        /// </summary>
        /// <param name="hModule">Handle to the loaded library to free</param>
        /// <returns>True if the function succeeds, otherwise false. Call <see cref="Marshal.GetLastWin32Error"/> on failure to get Win32 error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Retrieve a module handle for the specified module. The module must have been loaded by the calling process.
        /// </summary>
        /// <param name="lpModuleName">The name of the loaded module.</param>
        /// <returns>If the function succeeds, a handle to the module is returned. Otherwise, <see cref="IntPtr.Zero"/> is returned. Call <see cref="Marshal.GetLastWin32Error"/> on failure to get last Win32 error</returns>
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandleW", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(
            [MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

        /// <summary>
        /// Retrieves the address of an exported function from the specified Dll.
        /// </summary>
        /// <param name="hModule">Handle to the Dll module that contains the exported function</param>
        /// <param name="procName">The function name.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);


        #endregion

        #region Thread

        /// <summary>
        /// Create a thread that runs in the virtual address space of another process
        /// </summary>
        /// <param name="hProcess">A handle to the process in which the thread is to be created</param>
        /// <param name="lpThreadAttributes">A pointer to a SECURITY_ATTRIBUTES structure that specifies a security descriptor for the new thread and determines whether child processes can inherit the returned handle.</param>
        /// <param name="dwStackSize">The initial size of the stack, in bytes. The system rounds this value to the nearest page. If this parameter is 0 (zero), the new thread uses the default size for the executable.</param>
        /// <param name="lpStartAddress">A pointer to the application-defined function of type LPTHREAD_START_ROUTINE to be executed by the thread and represents the starting address of the thread in the remote process. The function must exist in the remote process.</param>
        /// <param name="lpParameter">A pointer to a variable to be passed to the thread function</param>
        /// <param name="dwCreationFlags">The flags that control the creation of the thread</param>
        /// <param name="lpThreadId">A pointer to a variable that receives the thread identifier. If this parameter is <see cref="IntPtr.Zero"/>, the thread identifier is not returned.</param>
        /// <returns>If the function succeeds, the return value is a handle to the new thread. If the function fails, the return value is <see cref="IntPtr.Zero"/>. Call <see cref="Marshal.GetLastWin32Error"/> to get Win32 Error.</returns>
        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread", SetLastError = true)]
        public static extern IntPtr CreateRemoteThread(
            IntPtr hProcess,
            IntPtr lpThreadAttributes,
            uint dwStackSize,
            IntPtr lpStartAddress,
            IntPtr lpParameter,
            uint dwCreationFlags,
            [Out] IntPtr lpThreadId);

        /// <summary>
        /// Waits until the specified object is in the signaled state or the time-out interval elapses.
        /// </summary>
        /// <param name="hObject">A handle to the object. For a list of the object types whose handles can be specified, see the following Remarks section.</param>
        /// <param name="dwMilliseconds">The time-out interval, in milliseconds. The function returns if the interval elapses, even if the object's state is nonsignaled. If dwMilliseconds is zero, the function tests the object's state and returns immediately. If dwMilliseconds is INFINITE, the function's time-out interval never elapses.</param>
        /// <returns>If the function succeeds, the return value indicates the event that caused the function to return. If the function fails, the return value is WAIT_FAILED ((DWORD)0xFFFFFFFF).</returns>
        [DllImport("kernel32", EntryPoint = "WaitForSingleObject")]
        public static extern uint WaitForSingleObject(IntPtr hObject, uint dwMilliseconds);

        /// <summary>
        /// Retrieves the termination status of the specified thread.
        /// </summary>
        /// <param name="hThread">Handle to the thread</param>
        /// <param name="lpExitCode">A pointer to a variable to receive the thread termination status. If this works properly, this should be the return value from the thread function of <see cref="CreateRemoteThread"/></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

        /// <summary>
        /// Retrieves the termination status of the specified thread.
        /// </summary>
        /// <param name="hThread">Handle to the thread</param>
        /// <param name="lpExitCode">A pointer to a variable to receive the thread termination status. If this works properly, this should be the return value from the thread function of <see cref="CreateRemoteThread"/></param>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);


        #endregion

        #region Handle

        /// <summary>
        /// Close an open handle
        /// </summary>
        /// <param name="hObject">Object handle to close</param>
        /// <returns>True if success, false if failure. Use <see cref="Marshal.GetLastWin32Error"/> on failure to get Win32 error.</returns>
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);


        #endregion

        #region Memory

        /// <summary>
        /// Reserves or commits a region of memory within the virtual address space of a specified process.
        /// The function initializes the memory it allocates to zero, unless <see cref="AllocationType.Reset"/> is used.
        /// </summary>
        /// <param name="hProcess">The handle to a process. The function allocated memory within the virtual address space of this process.
        /// The process must have the <see cref="ProcessAccessFlags.VMOperation"/> access right.</param>
        /// <param name="lpAddress">Optional desired address to begin allocation from. Use <see cref="IntPtr.Zero"/> to let the function determine the address</param>
        /// <param name="dwSize">The size of the region of memory to allocate, in bytes</param>
        /// <param name="flAllocationType">
        /// <see cref="AllocationType"/> type of allocation. Must contain one of <see cref="AllocationType.Commit"/>, <see cref="AllocationType.Reserve"/> or <see cref="AllocationType.Reset"/>.
        /// Can also specify <see cref="AllocationType.LargePages"/>, <see cref="AllocationType.Physical"/>, <see cref="AllocationType.TopDown"/>.
        /// </param>
        /// <param name="flProtect">One of <see cref="MemoryProtection"/> constants.</param>
        /// <returns>If the function succeeds, the return value is the base address of the allocated region of pages.
        /// If the function fails, the return value is <see cref="IntPtr.Zero"/>. Call <see cref="Marshal.GetLastWin32Error"/> to get Win32 error.</returns>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect);

        /// <summary>
        /// Releases, decommits, or releases and decommits a region of memory within the virtual address space of a specified process
        /// </summary>
        /// <param name="hProcess">A handle to a process. The function frees memory within the virtual address space of this process.
        /// The handle must have the <see cref="ProcessAccessFlags.VMOperation"/> access right</param>
        /// <param name="lpAddress">A pointer to the starting address of the region of memory to be freed.
        /// If the <paramref name="dwFreeType"/> parameter is <see cref="AllocationType.Release"/>, this address must be the base address
        /// returned by <see cref="VirtualAllocEx"/>.</param>
        /// <param name="dwSize">The size of the region of memory to free, in bytes.
        /// If the <paramref name="dwFreeType"/> parameter is <see cref="AllocationType.Release"/>, this parameter must be 0. The function
        /// frees the entire region that is reserved in the initial allocation call to <see cref="VirtualAllocEx"/></param>
        /// <param name="dwFreeType">The type of free operation. This parameter can be one of the following values: 
        /// <see cref="AllocationType.Decommit"/> or <see cref="AllocationType.Release"/></param>
        /// <returns>If the function is successful, it returns true. If the function fails, it returns false. Call <see cref="Marshal.GetLastWin32Error"/> to get Win32 error.</returns>
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            AllocationType dwFreeType);

        /// <summary>
        /// Reads data from an area of memory in the specified process.
        /// </summary>
        /// <param name="hProcess">Handle to the process from which the memory is being read. 
        /// The handle must have <see cref="ProcessAccessFlags.VMRead"/> access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process to begin reading from</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the process</param>
        /// <param name="dwSize">The number of bytes to be read</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes read into the specified buffer</param>
        /// <returns>If the function succeeds, it returns true. Otherwise, false is returned and calling <see cref="Marshal.GetLastWin32Error"/> will retrieve the error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            out byte[] lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead);

        /// <summary>
        /// Reads data from an area of memory in the specified process.
        /// </summary>
        /// <param name="hProcess">Handle to the process from which the memory is being read. 
        /// The handle must have <see cref="ProcessAccessFlags.VMRead"/> access to the process.</param>
        /// <param name="lpBaseAddress">A pointer to the base address in the specified process to begin reading from</param>
        /// <param name="lpBuffer">A pointer to a buffer that receives the contents from the address space of the process</param>
        /// <param name="dwSize">The number of bytes to be read</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes read into the specified buffer</param>
        /// <returns>If the function succeeds, it returns true. Otherwise, false is returned and calling <see cref="Marshal.GetLastWin32Error"/> will retrieve the error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            out IntPtr lpBuffer,
            uint dwSize,
            out int lpNumberOfBytesRead);

        /// <summary>
        /// Writes data to an area of memory in a specified process.
        /// </summary>
        /// <param name="hProcess">Handle to the process to write memory to.
        /// The handle must have <see cref="ProcessAccessFlags.VMWrite"/> and <see cref="ProcessAccessFlags.VMOperation"/> access to the process</param>
        /// <param name="lpBaseAddress">A pointer to the base address to write to in the specified process</param>
        /// <param name="lpBuffer">A pointer to a buffer that contains the data to be written</param>
        /// <param name="nSize">The number of bytes to write</param>
        /// <param name="lpNumberOfBytesWritten">The number of bytes written.</param>
        /// <returns>If the function succeeds, it returns true. Otherwise false is returned and calling <see cref="Marshal.GetLastWin32Error"/> will retrieve the error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten);

        /// <summary>
        /// Writes data to an area of memory in a specified process.
        /// </summary>
        /// <param name="hProcess">Handle to the process to write memory to.
        /// The handle must have <see cref="ProcessAccessFlags.VMWrite"/> and <see cref="ProcessAccessFlags.VMOperation"/> access to the process</param>
        /// <param name="lpBaseAddress">A pointer to the base address to write to in the specified process</param>
        /// <param name="lpBuffer">A pointer to a buffer that contains the data to be written</param>
        /// <param name="nSize">The number of bytes to write</param>
        /// <param name="lpNumberOfBytesWritten">The number of bytes written.</param>
        /// <returns>If the function succeeds, it returns true. Otherwise false is returned and calling <see cref="Marshal.GetLastWin32Error"/> will retrieve the error.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            uint nSize,
            out int lpNumberOfBytesWritten);

        #endregion

        #region Window
        #endregion
    }
}
