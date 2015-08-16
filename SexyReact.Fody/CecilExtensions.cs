using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SexyReact.Fody
{
    public static class CecilExtensions
    {
        public static AssemblyNameReference FindAssembly(this ModuleDefinition module, string name)
        {
            return module.AssemblyReferences
                .Where(x => x.Name == name)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();
        }

        public static void Emit(this MethodBody body, Action<ILProcessor> il)
        {
            il(body.GetILProcessor());
        }

        public static GenericInstanceMethod MakeGenericMethod(this MethodReference method, params TypeReference[] genericArguments)
        {
            var result = new GenericInstanceMethod(method);
            foreach (var argument in genericArguments)
                result.GenericArguments.Add(argument);
            return result;
        }

        public static bool IsAssignableFrom(this TypeReference baseType, TypeReference type, Action<string> logger = null)
        {
            return baseType.Resolve().IsAssignableFrom(type.Resolve(), logger);
        }

        public static bool IsAssignableFrom(this TypeDefinition baseType, TypeDefinition type, Action<string> logger = null)
        {
            logger = logger ?? (x => {});

            Queue<TypeDefinition> queue = new Queue<TypeDefinition>();
            queue.Enqueue(type);

            while (queue.Any())
            {
                var current = queue.Dequeue();
                logger(current.FullName);

                if (baseType.FullName == current.FullName)
                    return true;

                if (current.BaseType != null)
                    queue.Enqueue(current.BaseType.Resolve());

                foreach (var @interface in current.Interfaces)
                {
                    queue.Enqueue(@interface.Resolve());
                }
            }

            return false;
        }

        public static TypeDefinition GetEarliestAncestorThatDeclares(this TypeDefinition type, TypeReference attributeType)
        {
            var current = type;
            TypeDefinition result = null;
            while (current != null)
            {
                if (current.IsDefined(attributeType))
                {
                    result = current;
                }
                current = current.BaseType == null ? null : current.BaseType.Resolve();
            }
            return result;
        }

        public static bool IsDefined(this IMemberDefinition member, TypeReference attributeType, bool inherit = false)
        {
            var typeIsDefined = member.HasCustomAttributes && member.CustomAttributes.Any(x => x.AttributeType.FullName == attributeType.FullName);
            if (inherit && member.DeclaringType.BaseType != null)
            {
                typeIsDefined = member.DeclaringType.BaseType.Resolve().IsDefined(attributeType, true);
            }
            return typeIsDefined;
        }

        public static MethodReference Bind(this MethodReference method, GenericInstanceType genericType)
        {
            var reference = new MethodReference(method.Name, method.ReturnType, genericType);
            reference.HasThis = method.HasThis;
            reference.ExplicitThis = method.ExplicitThis;
            reference.CallingConvention = method.CallingConvention;

            foreach (var parameter in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            return reference;
        }
        /*
        public static MethodReference BindDefinition(this MethodReference method, TypeReference genericTypeDefinition)
        {
            if (!genericTypeDefinition.HasGenericParameters)
                return method;

            var genericDeclaration = new GenericInstanceType(genericTypeDefinition);
            foreach (var parameter in genericTypeDefinition.GenericParameters)
            {
                genericDeclaration.GenericArguments.Add(parameter);
            }
            var reference = new MethodReference(method.Name, method.ReturnType, genericDeclaration);
            reference.HasThis = method.HasThis;
            reference.ExplicitThis = method.ExplicitThis;
            reference.CallingConvention = method.CallingConvention;

            foreach (var parameter in method.Parameters)
                reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));

            return reference;
        }
        */
        public static FieldReference BindDefinition(this FieldReference field, TypeReference genericTypeDefinition)
        {
            if (!genericTypeDefinition.HasGenericParameters)
                return field;

            var genericDeclaration = new GenericInstanceType(genericTypeDefinition);
            foreach (var parameter in genericTypeDefinition.GenericParameters)
            {
                genericDeclaration.GenericArguments.Add(parameter);
            }
            var reference = new FieldReference(field.Name, field.FieldType, genericDeclaration);
            return reference;
        }

        public static TypeReference FindType(this ModuleDefinition currentModule, string @namespace, string typeName, IMetadataScope scope = null, params string[] typeParameters)
        {
            var result = new TypeReference(@namespace, typeName, currentModule, scope);
            foreach (var typeParameter in typeParameters)
            {
                result.GenericParameters.Add(new GenericParameter(typeParameter, result));
            }
            return result;
        }
/*

        public static IEnumerable<TypeDefinition> GetAllTypes(this ModuleDefinition module)
        {
            var stack = new Stack<TypeDefinition>();
            foreach (var type in module.Types)
            {
                stack.Push(type);
            }
            while (stack.Any())
            {
                var current = stack.Pop();
                yield return current;

                foreach (var nestedType in current.NestedTypes)
                {
                    stack.Push(nestedType);
                }
            }
        }
*/
    }
}