using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syringe.Win32
{
    /// <summary>
    /// Memory allocation type - taken from #defines in WinNT.h
    /// </summary>
    [Flags]
    public enum AllocationType : uint
    {
        Commit = 0x1000,       //#define MEM_COMMIT           0x1000     
        Reserve = 0x2000,       //#define MEM_RESERVE          0x2000     
        Decommit = 0x4000,       //#define MEM_DECOMMIT         0x4000     
        Release = 0x8000,       //#define MEM_RELEASE          0x8000     
        Free = 0x10000,      //#define MEM_FREE            0x10000     
        Private = 0x20000,      //#define MEM_PRIVATE         0x20000     
        Mapped = 0x40000,      //#define MEM_MAPPED          0x40000     
        Reset = 0x80000,      //#define MEM_RESET           0x80000     
        TopDown = 0x100000,     //#define MEM_TOP_DOWN       0x100000     
        WriteWatch = 0x200000,     //#define MEM_WRITE_WATCH    0x200000     
        Physical = 0x400000,     //#define MEM_PHYSICAL       0x400000     
        Rotate = 0x800000,     //#define MEM_ROTATE         0x800000     
        LargePages = 0x20000000,   //#define MEM_LARGE_PAGES  0x20000000     
        FourMbPages = 0x80000000    //#define MEM_4MB_PAGES    0x80000000
    }

    /// <summary>
    /// Memory protection type - taken from #defines in WinNT.h
    /// </summary>
    public enum MemoryProtection : uint
    {
        NoAccess = 0x001,    //#define PAGE_NOACCESS          0x01     
        ReadOnly = 0x002,    //#define PAGE_READONLY          0x02     
        ReadWrite = 0x004,    //#define PAGE_READWRITE         0x04     
        WriteCopy = 0x008,    //#define PAGE_WRITECOPY         0x08     
        Execute = 0x010,    //#define PAGE_EXECUTE           0x10     
        ExecuteRead = 0x020,    //#define PAGE_EXECUTE_READ      0x20     
        ExecuteReadWrite = 0x040,    //#define PAGE_EXECUTE_READWRITE 0x40     
        ExecuteWriteCopy = 0x080,    //#define PAGE_EXECUTE_WRITECOPY 0x80     
        PageGuard = 0x100,    //#define PAGE_GUARD            0x100     
        NoCache = 0x200,    //#define PAGE_NOCACHE          0x200     
        WriteCombine = 0x400,    //#define PAGE_WRITECOMBINE     0x400
    }

    /// <summary>
    /// Process access flags - taken from #defines in WinNT.h
    /// </summary>
    [Flags]
    public enum ProcessAccessFlags : uint
    {
        Terminate = 0x0001,     //#define PROCESS_TERMINATE                  (0x0001)  
        CreateThread = 0x0002,     //#define PROCESS_CREATE_THREAD              (0x0002) 
        SetSessionID = 0x0004,     //#define PROCESS_SET_SESSIONID              (0x0004)
        VMOperation = 0x0008,     //#define PROCESS_VM_OPERATION               (0x0008)  
        VMRead = 0x0010,     //#define PROCESS_VM_READ                    (0x0010) 
        VMWrite = 0x0020,     //#define PROCESS_VM_WRITE                   (0x0020)
        DUPHandle = 0x0040,     //#define PROCESS_DUP_HANDLE                 (0x0040)
        CreateProcess = 0x0080,     //#define PROCESS_CREATE_PROCESS             (0x0080)  
        SetQuota = 0x0100,     //#define PROCESS_SET_QUOTA                  (0x0100)  
        SetInformation = 0x0200,     //#define PROCESS_SET_INFORMATION            (0x0200)
        QueryInformation = 0x0400,     //#define PROCESS_QUERY_INFORMATION          (0x0400)
        SuspendResume = 0x0800,     //#define PROCESS_SUSPEND_RESUME             (0x0800)
        QueryLimitedInformation = 0x1000,     //#define PROCESS_QUERY_LIMITED_INFORMATION  (0x1000)
        AllAccess = Synchronize | StandardRightsRequired | 0xFFFF,
        //#define PROCESS_ALL_ACCESS        (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFFF)

        Synchronize = 0x100000,     //#define SYNCHRONIZE                      (0x00100000L)
        StandardRightsRequired = 0x0F0000      //#define STANDARD_RIGHTS_REQUIRED         (0x000F0000L)
    }

    /// <summary>
    /// Flags used in LoadLibraryEx to determine behaviour when loading library into process
    /// </summary>
    [Flags]
    public enum LoadLibraryExFlags : uint
    {
        DontResolveDllReferences = 0x00000001,     //#define DONT_RESOLVE_DLL_REFERENCES         0x00000001
        LoadLibraryAsDatafile = 0x00000002,     //#define LOAD_LIBRARY_AS_DATAFILE            0x00000002
        LoadLibraryWithAlteredSearchPath = 0x00000008,     //#define LOAD_WITH_ALTERED_SEARCH_PATH       0x00000008
        LoadIgnoreCodeAuthzLevel = 0x00000010,     //#define LOAD_IGNORE_CODE_AUTHZ_LEVEL        0x00000010
        LoadLibraryAsImageResource = 0x00000020,     //#define LOAD_LIBRARY_AS_IMAGE_RESOURCE      0x00000020
        LoadLibraryAsDatafileExclusive = 0x00000040      //#define LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE  0x00000040
    }

    [Flags]
    public enum ProcessCreationFlags : uint
    {
        None = 0x00000000,

        DebugProcess = 0x00000001,
        DebugOnlyThisProcess = 0x00000002,
        CreateSuspended = 0x00000004,
        DetachedProcess = 0x00000008,
        CreateNewConsole = 0x00000010,

        CreateNewProcessGroup = 0x00000200,
        CreateUnicodeEnvironment = 0x00000400,
        CreateSeparateWowVDM = 0x00000800,
        CreateSharedWowVDM = 0x00001000,

        InheritParentAffinity = 0x00010000,
        CreateProtectedProcess = 0x00040000,
        ExtendedStartupInfoPresent = 0x00080000,

        CreateBreakawayFromJob = 0x01000000,
        CreatePreserveCodeAuthzLevel = 0x02000000,
        CreateDefaultErrorMode = 0x04000000,
        CreateNoWindow = 0x08000000,
    }

    public enum ThreadWaitValue : uint
    {
        Object0 = 0x00000000,
        Abandoned = 0x00000080,
        Timeout = 0x00000102,
        Failed = 0xFFFFFFFF,
        Infinite = 0xFFFFFFFF
    }
}
