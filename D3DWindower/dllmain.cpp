// dllmain.cpp : Defines the entry point for the DLL application.
#include "stdafx.h"
#include <stdlib.h>
#include <Windows.h>
#include <direct.h>
#include <d3d9.h>
#include <d3dx9.h>
#include <Detours.h>
#include <thread>

#pragma comment(lib, "Detours.lib")
#pragma comment(lib, "d3d9.lib")

typedef struct _INIT_STRUCT 
{
	long reset_address;
	int width;
	int height;
} INIT_STRUCT, *PINIT_STRUCT;

using namespace std;

HMODULE dll;


int width;
int height;

extern "C" typedef HRESULT (WINAPI *pReset)(LPDIRECT3DDEVICE9 pDevice, D3DPRESENT_PARAMETERS *pPresentationParameters);
pReset Reset;

extern "C" HRESULT WINAPI MyReset(LPDIRECT3DDEVICE9 pDevice, D3DPRESENT_PARAMETERS *params)
{
	params->Windowed = true;
	params->FullScreen_RefreshRateInHz = 0;
	params->BackBufferWidth = width;
	params->BackBufferHeight = height;
	return Reset(pDevice, params);
}

extern "C" __declspec(dllexport) void InstallHook(LPVOID message)
{
	PINIT_STRUCT messageStruct = reinterpret_cast<PINIT_STRUCT>(message);	  
	
	Reset = (pReset)messageStruct->reset_address;
	width = (int)messageStruct->width;
	height = (int)messageStruct->height;
	DetourTransactionBegin();
    DetourUpdateThread(GetCurrentThread());
	DetourAttach(&(PVOID&)Reset, MyReset);
	DetourTransactionCommit();
}

BOOL APIENTRY DllMain( HMODULE hModule, DWORD  ul_reason_for_call,LPVOID lpReserved)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		dll = hModule;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

