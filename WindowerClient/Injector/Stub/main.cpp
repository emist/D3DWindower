#include "main.h"

DWORD WINAPI DllMain( HMODULE, DWORD_PTR, LPVOID ) {
	return TRUE;
}

extern "C" __declspec(dllexport) void Initialise() {
	::MessageBox(NULL, L"Default Initialise Method", L"Default Initialise Caption", MB_OK);
}

extern "C" __declspec(dllexport) void InitWithMessage( PVOID message ) {
	PINIT_STRUCT messageStruct = reinterpret_cast<PINIT_STRUCT>(message);
	::MessageBox(NULL, messageStruct->Message, messageStruct->Title, MB_OK);
	unsigned int p = STILL_ACTIVE;
}