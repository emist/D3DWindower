#pragma once

#ifndef _MAIN_H
#define _MAIN_H

#include <Windows.h>

typedef struct _INIT_STRUCT {
	LPCWSTR Title;
	LPCWSTR Message;
} INIT_STRUCT, *PINIT_STRUCT;

DWORD WINAPI DllMain( HMODULE, DWORD_PTR, LPVOID );
extern "C" __declspec(dllexport) void Initialise( void );
extern "C" __declspec(dllexport) void InitWithMessage( PVOID );

#endif