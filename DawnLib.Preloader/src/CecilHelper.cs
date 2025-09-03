using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace Dawn.Preloader;
public static class CecilHelper
{
    public static bool AddField(this TypeDefinition self, FieldAttributes fieldAttributes, string name, TypeReference type, Action<bool, string> logCallback = default)
    {
        logCallback?.Invoke(false, $"Adding field '{name}' to {self.FullName}");
        if (self.FindField(name) != null)
        {
            logCallback?.Invoke(true, $"Field '{name}' already exists in {self.FullName}");
            return false;
        }
        self.Fields.Add(new FieldDefinition(name, fieldAttributes, type));
        return true;
    }

    public static bool AddMethod(this TypeDefinition self, string methodName, MethodAttributes attributes = MethodAttributes.Private, TypeReference returnType = null, ParameterDefinition[] parameters = null, Action<bool, string> logCallback = default)
    {
        returnType ??= self.Module.TypeSystem.Void;
        parameters ??= [];

        logCallback?.Invoke(false, $"Adding method {returnType.FullName} {methodName}({string.Join(",", parameters.Select(p => p.ParameterType.FullName))}) to {self.FullName}");
        if (self.FindMethod(methodName) != null)
        {
            logCallback?.Invoke(true, $"Method '{methodName}' already exists in {self.FullName}");
            return false;
        }

        var methodDefinition = new MethodDefinition(methodName, attributes | MethodAttributes.HideBySig, returnType);
        self.Methods.Add(methodDefinition);

        methodDefinition.Parameters.AddRange(parameters);

        var processor = methodDefinition.Body.GetILProcessor();


        var ret = processor.Create(OpCodes.Ret);

        if (returnType.MetadataType != MetadataType.Void)
        {
            var constructorInfo = typeof(NotImplementedException).GetConstructor([typeof(string)]);
            var constructorReference = self.Module.ImportReference(constructorInfo);
            processor.Emit(OpCodes.Ldstr, "This is a Stub");
            processor.Emit(OpCodes.Newobj, constructorReference);
            processor.Emit(OpCodes.Throw);
        }

        processor.Append(ret);
        return true;
    }
}