#pragma once

using namespace System;
using namespace System::Diagnostics;
using namespace System::Text;
using namespace System::IO;
using namespace System::Collections::Generic;
using namespace System::Reflection;
using namespace System::ComponentModel;
using namespace System::Globalization;
using namespace System::CodeDom::Compiler;
using namespace Microsoft::CSharp;
using namespace MP3ScriptHookNet;

namespace WinForms = System::Windows::Forms;

namespace Loader {
	// My classic NReflec class, written in C++ CLI! And I hate it.
	ref class NReflec {
	public:
		static array<Type^>^ GetTypesFromDLL(String^ dllFilePath) {
			return Assembly::LoadFile(dllFilePath)->GetTypes();
		}

		static array<Type^>^ GetTypesFromAssembly(Assembly^ ass) {
			return ass->GetTypes();
		}

		static array<FieldInfo^>^ GetFieldsFromDLL(String^ dllFilePath) {
			List<FieldInfo^>^ fields = gcnew List<FieldInfo^>();
			array<Type^>^ types = Assembly::LoadFile(dllFilePath)->GetTypes();
			for (int i = 0; i < types->Length; i++)
				fields->AddRange(types[i]->GetFields());

			return fields->ToArray();
		}

		static array<MethodInfo^>^ GetMethodsFromDLL(String^ dllFilePath) {
			List<MethodInfo^>^ methods = gcnew List<MethodInfo^>();
			array<Type^>^ types = Assembly::LoadFile(dllFilePath)->GetTypes();
			for (int i = 0; i < types->Length; i++)
				methods->AddRange(types[i]->GetMethods());

			return methods->ToArray();
		}

		static array<FieldInfo^>^ GetFieldsFromAssembly(Assembly^ ass) {
			List<FieldInfo^>^ fields = gcnew List<FieldInfo^>();
			array<Type^>^ types = ass->GetTypes();
			for (int i = 0; i < types->Length; i++)
				fields->AddRange(types[i]->GetFields());

			return fields->ToArray();
		}

		static array<MethodInfo^>^ GetMethodsFromAssembly(Assembly^ ass) {
			List<MethodInfo^>^ methods = gcnew List<MethodInfo^>();
			array<Type^>^ types = ass->GetTypes();
			for (int i = 0; i < types->Length; i++)
				methods->AddRange(types[i]->GetMethods());

			return methods->ToArray();
		}

		static System::Object^ CallMethodFromType(Type^ t, String^ methodName, ... array<System::Object^>^ o) {
			MethodInfo^ methodInfo = t->GetMethod(methodName);
			array<ParameterInfo^>^ parameters = methodInfo->GetParameters();

			return methodInfo->Invoke(System::Activator::CreateInstance(t, nullptr), parameters->Length == 0 ? nullptr : o);
		}

		static System::Object^ CallMethodFromFile(String^ dllFilePath, String^ typeName, String^ methodName, ... array<System::Object^>^ o) {
			Type^ type = Assembly::LoadFile(dllFilePath)->GetType(typeName);
			MethodInfo^ methodInfo = type->GetMethod(methodName);
			array<ParameterInfo^>^ parameters = methodInfo->GetParameters();

			return methodInfo->Invoke(System::Activator::CreateInstance(type, nullptr), parameters->Length == 0 ? nullptr : o);
		}

		static bool IsValidAssembly(String^ dllFile) {
			return Assembly::LoadFrom(dllFile) != nullptr;
		}
	};

	public enum class ScriptMethod {
		None,
		[Description("Init")]
		Init,
		[Description("OnTick")]
		OnTick,
		[Description("OnKey")]
		OnKey
	};

	public ref class ScriptDomain {
		public:
			String^ Filename;
			Script^ Instance;

			ScriptDomain(String^ dllFile) {
				Filename = dllFile;
				String^ path = Path::Combine(AppDomain::CurrentDomain->BaseDirectory, Filename);

				types = NReflec::GetTypesFromDLL(path);
				methods = NReflec::GetMethodsFromDLL(path);
				fields = NReflec::GetFieldsFromDLL(path);

				scriptType = GetScriptType();
				Instance = GetScript();
			}

			ScriptDomain(Assembly^ ass, String^ filename) {
				Filename = filename;

				types = NReflec::GetTypesFromAssembly(ass);
				methods = NReflec::GetMethodsFromAssembly(ass);
				fields = NReflec::GetFieldsFromAssembly(ass);

				scriptType = GetScriptType();
				Instance = GetScript();
			}

			void CallScriptFunction(ScriptMethod scriptMethod, ... array<System::Object^>^ o) {
				try {
					NReflec::CallMethodFromType(scriptType, scriptMethod.ToString("G"), o);
				}
				catch (Exception^ ex) {
					MP3ScriptHookNet::Log::Print(String::Format("{0} ERROR", Filename), String::Format("Could not call plugin method {0} Exception: {1}", scriptMethod.ToString("G"), ex->ToString()), true);
				}
			}

			System::Object^ GetScriptVariable(String^ variablename) {
				return scriptType->GetField(variablename)->GetValue(Instance);
			}

			static void Cleanup() {
				MP3::Native::Memory::CleanupPinnedStrings();
			}

			virtual String^ ToString() override {
				StringBuilder^ sb = gcnew StringBuilder();
				sb->AppendLine(String::Format("ScriptDomain: {0}", Filename));
				sb->AppendLine(String::Format("Amount of types: {1}", types->Length));
				sb->AppendLine("Methods: ");
				for (int i = 0; i < methods->Length; i++) {
					sb->AppendLine(String::Format("Method {0}: {1}", i, methods[i]->Name));
				}

				return sb->ToString();
			}

		private:
			array<Type^>^ types;
			array<MethodInfo^>^ methods;
			array<FieldInfo^>^ fields;
			Type^ scriptType;

			Type^ GetScriptType() {
				for (int i = 0; i < types->Length; i++) {
					if (types[i]->IsSubclassOf(Script::typeid)) {
						return types[i];
					}
				}

				return nullptr;
			}

			Script^ GetScript() {
				return (Script^)Activator::CreateInstance(scriptType);
			}
	};

	public ref class Class1
	{
	};
}
