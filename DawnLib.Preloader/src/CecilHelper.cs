using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace Dawn.Preloader;

internal static class CecilHelpers
{
    public static T GetAttributeInstance<T>(this CustomAttribute ceciAttribute) where T : Attribute
    {
        var attrType = typeof(T);
        var constructorArgs = ceciAttribute.ConstructorArguments.Select(ca => ca.Value).ToArray();
        return (T)Activator.CreateInstance(attrType, constructorArgs);
    }

    internal static bool AddRaise(this TypeDefinition self, string eventName, Action<bool, string>? logCallback = null)
    {
        var methodName = $"call_{eventName}";
        logCallback?.Invoke(false, $"Adding caller for event '{eventName}' to {self.FullName}");
        var eventDefinition = self.FindEvent(eventName);
        if (eventDefinition == null)
        {
            logCallback?.Invoke(true, $"Event '{eventName}' does not exists in {self.FullName}");
            return false;
        }

        var field = self.FindField(eventName);
        if (field == null)
        {
            logCallback?.Invoke(true, $"Field '{eventName}' does not exists in {self.FullName}");
            return false;
        }

        if (self.FindMethod(methodName) != null)
        {
            logCallback?.Invoke(true, $"Method '{methodName}' already exists in {self.FullName}");
            return false;
        }

        var fieldInvoker = field!.FieldType.Resolve().FindMethod("Invoke");
        var fieldInvokerReference = self.Module.ImportReference(fieldInvoker);

        var isStatic = false;
        MethodAttributes methodAttributes = 0;
        if ((field.Attributes & FieldAttributes.Static) != 0)
        {
            methodAttributes |= MethodAttributes.Static;
            isStatic = true;
        }
        if ((field.Attributes & FieldAttributes.Private) != 0)
        {
            methodAttributes |= MethodAttributes.Private;
        }

        var methodDefinition = new MethodDefinition(methodName, methodAttributes, field.FieldType);
        self.Methods.Add(methodDefinition);
        methodDefinition.Parameters.AddRange(fieldInvokerReference.Parameters);

        var instructions = methodDefinition.Body.Instructions;

        var pop = Instruction.Create(OpCodes.Pop);
        var ret = Instruction.Create(OpCodes.Ret);

        instructions.AddRange([
            Instruction.Create(isStatic ? OpCodes.Nop : OpCodes.Ldarg_0),
            Instruction.Create(isStatic ? OpCodes.Ldsfld : OpCodes.Ldfld, field),
            Instruction.Create(OpCodes.Dup),
            Instruction.Create(OpCodes.Ldnull),
            Instruction.Create(OpCodes.Cgt_Un),
            Instruction.Create(OpCodes.Brfalse, pop)
        ]);

        foreach (var param in methodDefinition.Parameters)
        {
            instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
        }

        instructions.Add(Instruction.Create(OpCodes.Callvirt, fieldInvokerReference));
        instructions.Add(Instruction.Create(OpCodes.Br, ret));

        instructions.AddRange([
            pop,
            ret
        ]);
        return true;
    }

    internal static bool ImplementInterface(this TypeDefinition self, Type @interface, Action<bool, string>? logCallback = null)
    {
        //check if it's an interface duh
        if (!@interface.IsInterface)
        {
            logCallback?.Invoke(true, $"Type '{@interface.FullName}' is not an interface!");
            return false;
        }

        return self.ImplementInterface(self.Module.ImportReference(@interface), logCallback);
    }

    internal static bool ImplementInterface(this TypeDefinition self, TypeReference @interface, Action<bool, string>? logCallback = null)
    {
        var definition = @interface.Resolve();

        //check if it's an interface duh
        if (!definition.IsInterface)
        {
            logCallback?.Invoke(true, $"Type '{@interface.FullName}' is not an interface!");
            return false;
        }

        logCallback?.Invoke(false, $"Adding '{@interface.FullName}' to {self.FullName}'");

        //import it in the target assembly
        var newRef = self.Module.ImportReference(@interface);

        var blacklist = new HashSet<IMemberDefinition>();

        if (!Interfaces.ImplementProperties(self, definition, blacklist, out var properties, logCallback))
            return false;

        if (!Interfaces.ImplementEvents(self, definition, blacklist, out var events, logCallback))
            return false;

        if (!Interfaces.ImplementMethods(self, definition, blacklist, out var methods, logCallback))
            return false;

        self.Interfaces.Add(new InterfaceImplementation(newRef));

        return true;
    }

    private static class Interfaces
    {
        internal static bool ImplementProperties(in TypeDefinition type, TypeDefinition @interface, in HashSet<IMemberDefinition> blacklist, out List<PropertyDefinition> properties, Action<bool, string>? logCallback = null)
        {
            properties = [];
            foreach (var property in @interface.Properties)
            {
                if (!ImplementProperty(type, property, blacklist, out var implementation, logCallback))
                    continue;

                properties.Add(implementation);
            }
            return true;
        }

        internal static bool ImplementEvents(in TypeDefinition type, TypeDefinition @interface, in HashSet<IMemberDefinition> blacklist, out List<EventDefinition> events, Action<bool, string>? logCallback = null)
        {
            events = [];
            foreach (var @event in @interface.Events)
            {
                if (!ImplementEvent(type, @event, blacklist, out var implementation, logCallback))
                    continue;

                events.Add(implementation);
            }
            return true;
        }

        internal static bool ImplementMethods(in TypeDefinition type, TypeDefinition @interface, in HashSet<IMemberDefinition> blacklist, out List<MethodDefinition> methods, Action<bool, string>? logCallback = null)
        {
            methods = [];
            foreach (var method in @interface.Methods)
            {
                if (!ImplementMethod(type, method, blacklist, out var implementation, logCallback))
                    continue;

                methods.Add(implementation);
            }

            return true;
        }

        private static bool ImplementProperty(in TypeDefinition type, PropertyDefinition property, in HashSet<IMemberDefinition> blacklist, [NotNullWhen(true)] out PropertyDefinition? implementation, Action<bool, string>? logCallback = null)
        {
            implementation = property;
            FieldDefinition? backingField = null;

            var hasImplementation = false;

            if (!blacklist.Add(property))
                return true;

            logCallback?.Invoke(false, $"Adding Property '{property.Name}' to {type.FullName}'");

            if (property.GetMethod is { IsAbstract: true } && blacklist.Add(property.GetMethod))
            {
                if (!Init(type, out implementation))
                    return false;

                logCallback?.Invoke(false, $"Adding getter of '{property.Name}' to {type.FullName}'");

                if (type.FindMethod($"get_{property.Name}") != null)
                {
                    logCallback?.Invoke(true, $"Method 'get_{property.Name}' is already defined in '{type.FullName}'");
                    return false;
                }

                var getMethod = new MethodDefinition(
                    $"get_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    property.PropertyType
                );
                var getIL = getMethod.Body.GetILProcessor();
                getIL.Emit(OpCodes.Ldarg_0);
                getIL.Emit(OpCodes.Ldfld, backingField);
                getIL.Emit(OpCodes.Ret);
                type.Methods.Add(getMethod);
                implementation.GetMethod = getMethod;
            }

            if (property.SetMethod is { IsAbstract: true } && blacklist.Add(property.SetMethod))
            {
                if (!hasImplementation && !Init(type, out implementation))
                {
                    return false;
                }

                logCallback?.Invoke(false, $"Adding setter of '{property.Name}' to {type.FullName}'");

                if (type.FindMethod($"set_{property.Name}") != null)
                {
                    logCallback?.Invoke(true, $"Method 'set_{property.Name}' is already defined in '{type.FullName}'");
                    return false;
                }

                var setMethod = new MethodDefinition(
                    $"set_{property.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    type.Module.TypeSystem.Void
                );
                setMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, property.PropertyType));
                var setIL = setMethod.Body.GetILProcessor();
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_1);
                setIL.Emit(OpCodes.Stfld, backingField);
                setIL.Emit(OpCodes.Ret);
                type.Methods.Add(setMethod);
                implementation.SetMethod = setMethod;
            }

            if (!hasImplementation)
                return true;

            // add backing field and implementation if they have been created
            type.Fields.Add(backingField);
            type.Properties.Add(implementation);

            return true;

            bool Init(in TypeDefinition @type, [NotNullWhen(true)] out PropertyDefinition? @implementation)
            {
                @implementation = null;

                //create the backing field
                if (type.FindField($"<{property.Name}>k__BackingField") != null)
                {
                    logCallback?.Invoke(true, $"Field '<{property.Name}>k__BackingField' already exists in {type.FullName}");
                    return false;
                }

                backingField = new FieldDefinition(
                    $"<{property.Name}>k__BackingField",
                    FieldAttributes.Private,
                    property.PropertyType
                );

                // Create the property
                if (type.FindProperty(property.Name) != null)
                {
                    logCallback?.Invoke(true, $"Property '{property.Name}' already exists in {type.FullName}");
                    return false;
                }

                @implementation = new PropertyDefinition(property.Name, PropertyAttributes.None, property.PropertyType);

                hasImplementation = true;
                return true;
            }
        }

        private static bool ImplementEvent(in TypeDefinition type, EventDefinition @event, in HashSet<IMemberDefinition> blacklist, [NotNullWhen(true)] out EventDefinition? implementation, Action<bool, string>? logCallback = null)
        {
            implementation = @event;
            FieldDefinition? backingField = null;

            var hasImplementation = false;

            if (!blacklist.Add(@event))
                return true;

            logCallback?.Invoke(false, $"Adding Event '{@event.Name}' to {type.FullName}'");

            if (@event.AddMethod is { IsAbstract: true } && blacklist.Add(@event.AddMethod))
            {
                if (!Init(type, out implementation))
                    return false;

                logCallback?.Invoke(false, $"Adding registration of '{@event.Name}' to {type.FullName}'");

                if (type.FindMethod($"add_{@event.Name}") != null)
                {
                    logCallback?.Invoke(true, $"Method 'add_{@event.Name}' is already defined in '{type.FullName}'");
                    return false;
                }

                var addMethod = new MethodDefinition(
                    $"add_{@event.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    type.Module.TypeSystem.Void
                );
                addMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, @event.EventType));
                var addIL = addMethod.Body.GetILProcessor();
                addIL.Emit(OpCodes.Ldarg_0);
                addIL.Emit(OpCodes.Ldarg_0);
                addIL.Emit(OpCodes.Ldfld, backingField);
                addIL.Emit(OpCodes.Ldarg_1);
                addIL.Emit(OpCodes.Call, type.Module.ImportReference(typeof(Delegate).GetMethod("Combine", [typeof(Delegate), typeof(Delegate)])));
                addIL.Emit(OpCodes.Castclass, @event.EventType);
                addIL.Emit(OpCodes.Stfld, backingField);
                addIL.Emit(OpCodes.Ret);
                type.Methods.Add(addMethod);
                implementation.AddMethod = addMethod;
            }

            if (@event.RemoveMethod is { IsAbstract: true } && blacklist.Add(@event.RemoveMethod))
            {
                if (!hasImplementation && !Init(type, out implementation))
                {
                    return false;
                }

                logCallback?.Invoke(false, $"Adding de-registration of '{@event.Name}' to {type.FullName}'");

                if (type.FindMethod($"remove_{@event.Name}") != null)
                {
                    logCallback?.Invoke(true, $"Method 'remove_{@event.Name}' is already defined in '{type.FullName}'");
                    return false;
                }

                var removeMethod = new MethodDefinition(
                    $"remove_{@event.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    type.Module.TypeSystem.Void
                );
                removeMethod.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, @event.EventType));
                var removeIL = removeMethod.Body.GetILProcessor();
                removeIL.Emit(OpCodes.Ldarg_0);
                removeIL.Emit(OpCodes.Ldarg_0);
                removeIL.Emit(OpCodes.Ldfld, backingField);
                removeIL.Emit(OpCodes.Ldarg_1);
                removeIL.Emit(OpCodes.Call, type.Module.ImportReference(typeof(Delegate).GetMethod("Remove", [typeof(Delegate), typeof(Delegate)])));
                removeIL.Emit(OpCodes.Castclass, @event.EventType);
                removeIL.Emit(OpCodes.Stfld, backingField);
                removeIL.Emit(OpCodes.Ret);
                type.Methods.Add(removeMethod);
                implementation.RemoveMethod = removeMethod;
            }

            if (@event.InvokeMethod is { IsAbstract: true } && blacklist.Add(@event.InvokeMethod))
            {
                if (!hasImplementation && !Init(type, out implementation))
                {
                    return false;
                }

                logCallback?.Invoke(false, $"Adding invocation of '{@event.Name}' to {type.FullName}'");

                if (type.FindMethod($"raise_{@event.Name}") != null)
                {
                    logCallback?.Invoke(true, $"Method 'raise_{@event.Name}' is already defined in '{type.FullName}'");
                    return false;
                }

                var invokeMethod = new MethodDefinition(
                    $"raise_{@event.Name}",
                    MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                    type.Module.TypeSystem.Void
                );
                // Invoke method parameters depend on the delegate signature
                foreach (var param in @event.InvokeMethod.Parameters)
                {
                    invokeMethod.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
                }
                var invokeIL = invokeMethod.Body.GetILProcessor();
                invokeIL.Emit(OpCodes.Ldarg_0);
                invokeIL.Emit(OpCodes.Ldfld, backingField);
                for (var i = 0; i < invokeMethod.Parameters.Count; i++)
                {
                    invokeIL.Emit(OpCodes.Ldarg, i + 1);
                }
                invokeIL.Emit(OpCodes.Callvirt, type.Module.ImportReference(@event.EventType.Resolve().Methods.First(m => m.Name == "Invoke")));
                invokeIL.Emit(OpCodes.Ret);
                type.Methods.Add(invokeMethod);
                implementation.InvokeMethod = invokeMethod;
            }

            if (!hasImplementation)
                return true;

            // add backing field and implementation if they have been created
            type.Fields.Add(backingField);
            type.Events.Add(implementation);

            return true;

            bool Init(in TypeDefinition @type, [NotNullWhen(true)] out EventDefinition? @implementation)
            {
                @implementation = null;

                //create the backing field
                if (type.FindField(@event.Name) != null)
                {
                    logCallback?.Invoke(true, $"Field '{@event.Name}' already exists in {type.FullName}");
                    return false;
                }

                backingField = new FieldDefinition(
                    @event.Name,
                    FieldAttributes.Private,
                    @event.EventType
                );

                // Create the property
                if (type.FindEvent(@event.Name) != null)
                {
                    logCallback?.Invoke(true, $"Event '{@event.Name}' already exists in {type.FullName}");
                    return false;
                }

                @implementation = new EventDefinition(@event.Name, EventAttributes.None, @event.EventType);

                hasImplementation = true;
                return true;
            }
        }

        private static bool ImplementMethod(in TypeDefinition type, MethodDefinition method, in HashSet<IMemberDefinition> blacklist, out MethodDefinition implementation, Action<bool, string>? logCallback = null)
        {
            implementation = method;

            if (!blacklist.Add(method))
                return true;

            logCallback?.Invoke(false, $"Adding Method '{method.Name}' to {type.FullName}'");

            //only implement methods that do not have a default implementation!
            if (!method.IsAbstract)
            {
                implementation = method;
                return true;
            }

            if (type.FindMethod(method.Name) != null)
            {
                logCallback?.Invoke(true, $"Method '{method.Name}' is already defined in '{type.FullName}'");
                return false;
            }

            // Create a new method to implement the interface method
            implementation = new MethodDefinition(
                method.Name,
                MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final,
                method.ReturnType
            );

            // Copy parameters
            foreach (var param in method.Parameters)
            {
                implementation.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));
            }

            // Create method body (simple return or throw for now)
            var il = implementation.Body.GetILProcessor();

            if (method.ReturnType.FullName != "System.Void")
            {
                var constructorInfo = typeof(NotImplementedException).GetConstructor([typeof(string)]);
                var constructorReference = type.Module.ImportReference(constructorInfo);
                il.Emit(OpCodes.Ldstr, "This is a Stub");
                il.Emit(OpCodes.Newobj, constructorReference);
                il.Emit(OpCodes.Throw);
            }
            else
            {
                il.Emit(OpCodes.Ret);
            }

            // Add the method to the type
            type.Methods.Add(implementation);

            return true;
        }
    }
}