using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;

namespace CodeRebirthLib.Preloader;
class CodeRebirthLibPreloader
{
    internal static ManualLogSource Log { get; } = Logger.CreateLogSource("CodeRebirthLib.Preloader");

    public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };
    
    public static void Patch(AssemblyDefinition assembly)
    {
        Log.LogInfo($"Patching {assembly.Name.Name}");

        Dictionary<string, List<Type>> interfacesToInject = [];
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if(!type.IsInterface) continue;
            var attributes = type.GetCustomAttributes(typeof(InjectInterfaceAttribute)).Cast<InjectInterfaceAttribute>();
            if (!attributes.Any()) continue;
            
            foreach (InjectInterfaceAttribute attribute in attributes)
            {
                if (!interfacesToInject.TryGetValue(attribute.FullName, out List<Type> existing))
                {
                    existing = [];
                }
                existing.Add(type);
                interfacesToInject[attribute.FullName] = existing;
            }
        }
        
        Log.LogInfo($"Injecting interfaces into '{interfacesToInject.Count}' different types.");
        
        foreach (TypeDefinition type in assembly.MainModule.Types)
        {
            if (!interfacesToInject.TryGetValue(type.FullName, out List<Type> interfaces)) continue;
            Log.LogInfo($"{type.Name} has {interfaces.Count} interface(s) to inject.");
            
            foreach (Type @interface in interfaces)
            {
                ImplementInterface(type, @interface);
            }
        }
    }

    static void ImplementInterface(TypeDefinition type, Type @interface)
    {
        Log.LogDebug($"Injecting {@interface.FullName} into {type.FullName}.");

        var module = type.Module;
        
        foreach (MethodInfo method in @interface.GetMethods())
        {
            Log.LogDebug($"trying to inject {method.Name} ({method.Attributes})");
            if(method.Attributes.HasFlag(System.Reflection.MethodAttributes.SpecialName)) continue;
            
            type.AddMethod(
                method.Name, 
                MethodAttributes.Public | MethodAttributes.Virtual, 
                module.ImportReference(method.ReturnType), 
                method.GetParameters().Select(it => ToMonoCecilParameter(it, type.Module)).ToArray()
            );
        }

        foreach (PropertyInfo property in @interface.GetProperties())
        {
            Log.LogDebug($"Injecting {property.Name}");

            var propertyType = module.ImportReference(property.PropertyType);
            
            var field = new FieldDefinition($"<{property.Name}>k__BackingField", FieldAttributes.Private | FieldAttributes.InitOnly, propertyType);
            var propertyDef = new PropertyDefinition(property.Name, 0, propertyType);
        
            var attributes = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.VtableLayoutMask | MethodAttributes.Virtual;
            var getter = new MethodDefinition($"get_{property.Name}", attributes, propertyType);
            var getterProcessor = getter.Body.GetILProcessor();
            getterProcessor.Emit(OpCodes.Ldarg_0);
            getterProcessor.Emit(OpCodes.Ldfld, field);
            getterProcessor.Emit(OpCodes.Ret);

            var setter = new MethodDefinition($"set_{property.Name}", attributes, module.TypeSystem.Void);
            setter.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, propertyType));
            var setterProcessor = setter.Body.GetILProcessor();
            setterProcessor.Emit(OpCodes.Ldarg_0);
            setterProcessor.Emit(OpCodes.Ldarg_1);
            setterProcessor.Emit(OpCodes.Stfld, field);
            setterProcessor.Emit(OpCodes.Ret);
            
            type.Methods.Add(getter);
            type.Methods.Add(setter);

            propertyDef.GetMethod = getter;
            propertyDef.SetMethod = setter;
            
            type.Properties.Add(propertyDef);
            type.Fields.Add(field);
        }
        
        type.Interfaces.Add(new InterfaceImplementation(type.Module.ImportReference(@interface)));
    }

    static ParameterDefinition ToMonoCecilParameter(ParameterInfo pi, ModuleDefinition module)
    {
        var typeRef = module.ImportReference(pi.ParameterType);
        var def = new ParameterDefinition(pi.Name, (ParameterAttributes)pi.Attributes, typeRef);

        return def;
    }

    // Cannot be renamed, method name is important
    public static void Initialize()
    {
        Log.LogInfo($"Prepatcher Started");
    }

    // Cannot be renamed, method name is important
    public static void Finish()
    {
        Log.LogInfo($"Prepatcher Finished");
    }
}
