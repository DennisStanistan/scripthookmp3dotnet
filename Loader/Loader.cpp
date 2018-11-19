#include "stdafx.h"
#include "Loader.h"
#pragma managed

#pragma region Implicit PInvoke Definitions
[Runtime::InteropServices::DllImport("kernel32", CharSet = Runtime::InteropServices::CharSet::Unicode, SetLastError = false)]
extern int DeleteFile(String ^name);
#pragma endregion

ref struct LoaderData {
	static bool scriptsInitializationComplete = false;
	static List<Loader::ScriptDomain^>^ scriptDomains;
};

bool CompileScript(String^ file) {
	
	String^ filename = Path::GetFileName(file);

	FileInfo^ sourceFile = gcnew FileInfo(file);
	CodeDomProvider^ provider = nullptr;
	
	Dictionary<String^, String^>^ d = gcnew Dictionary<String^, String^>;
	d->Add("CompilerVersion", "v4.0");

	CSharpCodeProvider^ csc = gcnew CSharpCodeProvider(d);
	CompilerParameters^ parameters = gcnew CompilerParameters();

	array<Assembly^>^ assemblies = AppDomain::CurrentDomain->GetAssemblies();

	for each(Assembly^ ass in assemblies) {
		if (!ass->IsDynamic) {
			parameters->ReferencedAssemblies->Add(ass->Location);
		}
	}

	parameters->GenerateExecutable = false;
	parameters->GenerateInMemory = true;

	if (sourceFile->Extension->ToUpper(CultureInfo::InvariantCulture) == ".CS") {
		provider = CodeDomProvider::CreateProvider("CSharp");
	} else if (sourceFile->Extension->ToUpper(CultureInfo::InvariantCulture) == ".VB") {
		provider = CodeDomProvider::CreateProvider("VisualBasic");
	}

	CompilerResults^ results = provider->CompileAssemblyFromFile(parameters, file);
	switch (results->Errors->HasErrors) {
	case true:
		MP3ScriptHookNet::Log::Print("ERROR", String::Format("{0} could not be compiled.", filename), true);
		for each (CompilerError^ ce in results->Errors) {
			MP3ScriptHookNet::Log::Print(String::Format("{0} COMPILER ERROR", filename), ce->ErrorText, true);
		}
		return false;

	case false:
		
		Assembly^ ass = results->CompiledAssembly;
		Loader::ScriptDomain^ script = gcnew Loader::ScriptDomain(ass, filename);
		LoaderData::scriptDomains->Add(script);

		MP3ScriptHookNet::Log::Print("INFO", String::Format("{0} was loaded.", filename), true);
		return true;
	}

	return false;
}

void LoadScripts() {
	LoaderData::scriptDomains = gcnew List<Loader::ScriptDomain^>();

	if (!Directory::Exists("scripts"))
		return;

	array<String^>^ dllFiles = Directory::GetFiles("scripts", "*.dll");
	array<String^>^ csFiles = Directory::GetFiles("scripts", "*.cs");
	array<String^>^ vbFiles = Directory::GetFiles("scripts", "*.vb");

	int compiledScripts = 0;

	for (int i = 0; i < dllFiles->Length; i++) {
		if (!Loader::NReflec::IsValidAssembly(dllFiles[i]))
			continue;

		Loader::ScriptDomain^ scriptDomain = gcnew Loader::ScriptDomain(dllFiles[i]);
		LoaderData::scriptDomains->Add(scriptDomain);
		compiledScripts++;
	}

	for (int i = 0; i < csFiles->Length; i++) {
		if (!CompileScript(csFiles[i]))
			continue;

		compiledScripts++;
	}

	for (int i = 0; i < vbFiles->Length; i++) {
		if (!CompileScript(vbFiles[i]))
			continue;

		compiledScripts++;
	}

	LoaderData::scriptsInitializationComplete = true;
}

void ReloadScripts() {
	Loader::ScriptDomain::Cleanup();
	LoadScripts();
}

void InitNetInstance() {
	MP3ScriptHookNet::Instance::GameProcess = Process::GetCurrentProcess();
}

void ManagedInit() {
	for (int i = 0; i < LoaderData::scriptDomains->Count; i++) {
		LoaderData::scriptDomains[i]->CallScriptFunction(Loader::ScriptMethod::Init);
	}
}

bool ManagedTick()
{
	for (int i = 0; i < LoaderData::scriptDomains->Count; i++) {
		LoaderData::scriptDomains[i]->CallScriptFunction(Loader::ScriptMethod::OnTick);
	}

	return true;
}

void ManagedKeyboardMessage(unsigned long key, bool status, bool statusCtrl, bool statusShift, bool statusAlt)
{
	array<System::Object ^> ^args = gcnew array<System::Object ^>(5) { static_cast<WinForms::Keys>(key), status, statusCtrl, statusShift, statusAlt };
	for (int i = 0; i < LoaderData::scriptDomains->Count; i++) {
		LoaderData::scriptDomains[i]->CallScriptFunction(Loader::ScriptMethod::OnKey, args);
	}
	
}

#pragma unmanaged
#pragma warning(disable:4717) // disable recursion warning

bool sGameReloaded = false;
PVOID sMainFib = nullptr;
PVOID sScriptFib = nullptr;

__inline PVOID GetCurrentFiber(void) { return (PVOID)(UINT_PTR)__readfsdword(0x10); }

void ScriptMain() {
	srand(GetTickCount());
	sGameReloaded = true;
	sMainFib = GetCurrentFiber();

	LoadScripts();
	if (sScriptFib == nullptr) {
		const LPFIBER_START_ROUTINE FiberMain = [](LPVOID lpFiberParameter) {
			while (true) {
				ManagedInit();
				while (ManagedTick()) {
					ManagedTick();
					SwitchToFiber(sMainFib);
				}
			}
		};

		sScriptFib = CreateFiber(0, FiberMain, nullptr);
	}

	while (true) {
		scriptWait(0);
		SwitchToFiber(sScriptFib);
	}
}

static void ScriptKeyboardMessage(DWORD key, WORD repeats, BYTE scanCode, BOOL isExtended, BOOL isWithAlt, BOOL wasDownBefore, BOOL isUpNow)
{
	ManagedKeyboardMessage(key, isUpNow == FALSE, (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0, (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0, isWithAlt != FALSE);
}

BOOL WINAPI DllMain(HMODULE hModule, DWORD fdwReason, LPVOID lpvReserved) {
	switch (fdwReason) {
		case DLL_PROCESS_ATTACH:
			DisableThreadLibraryCalls(hModule);
			scriptRegister(hModule, ScriptMain);
			keyboardHandlerRegister(&ScriptKeyboardMessage);
			break;

		case DLL_PROCESS_DETACH:
			scriptUnregister(hModule);
			keyboardHandlerUnregister(OnKeyboardMessage);
			break;
	}

	return TRUE;
}