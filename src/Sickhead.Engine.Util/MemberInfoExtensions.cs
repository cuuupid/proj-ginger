using System;
using System.Reflection;

namespace Sickhead.Engine.Util
{
	public static class MemberInfoExtensions
	{
		public static Type GetDataType(this MemberInfo info)
		{
			if (info is PropertyInfo)
			{
				PropertyInfo propertyInfo = info as PropertyInfo;
				return propertyInfo.PropertyType;
			}
			if (info is FieldInfo)
			{
				FieldInfo fieldInfo = info as FieldInfo;
				return fieldInfo.FieldType;
			}
			throw new InvalidOperationException($"MemberInfo.GetDataType is not possible for type={info.GetType()}");
		}

		public static object GetValue(this MemberInfo info, object obj)
		{
			return info.GetValue(obj, null);
		}

		public static void SetValue(this MemberInfo info, object obj, object value)
		{
			info.SetValue(obj, value, null);
		}

		public static object GetValue(this MemberInfo info, object obj, object[] index)
		{
			if (info is PropertyInfo)
			{
				PropertyInfo propertyInfo = info as PropertyInfo;
				return propertyInfo.GetValue(obj, index);
			}
			if (info is FieldInfo)
			{
				FieldInfo fieldInfo = info as FieldInfo;
				return fieldInfo.GetValue(obj);
			}
			throw new InvalidOperationException($"MemberInfo.GetValue is not possible for type={info.GetType()}");
		}

		public static void SetValue(this MemberInfo info, object obj, object value, object[] index)
		{
			if (info is PropertyInfo)
			{
				PropertyInfo propertyInfo = info as PropertyInfo;
				propertyInfo.SetValue(obj, value, index);
				return;
			}
			if (info is FieldInfo)
			{
				FieldInfo fieldInfo = info as FieldInfo;
				fieldInfo.SetValue(obj, value);
				return;
			}
			if (info is MethodInfo)
			{
				MethodInfo methodInfo = info as MethodInfo;
			}
			throw new InvalidOperationException($"MemberInfo.SetValue is not possible for type={info.GetType()}");
		}

		public static bool IsStatic(this MemberInfo info)
		{
			if (info is PropertyInfo)
			{
				PropertyInfo propertyInfo = info as PropertyInfo;
				return propertyInfo.GetGetMethod(nonPublic: true).IsStatic;
			}
			if (info is FieldInfo)
			{
				FieldInfo fieldInfo = info as FieldInfo;
				return fieldInfo.IsStatic;
			}
			if (info is MethodInfo)
			{
				MethodInfo methodInfo = info as MethodInfo;
				return methodInfo.IsStatic;
			}
			throw new InvalidOperationException($"MemberInfo.IsStatic is not possible for type={info.GetType()}");
		}

		public static bool CanBeSet(this MemberInfo info)
		{
			if (info is PropertyInfo)
			{
				PropertyInfo propertyInfo = info as PropertyInfo;
				MethodAttributes attributes = propertyInfo.GetSetMethod().Attributes;
				if (propertyInfo.CanWrite)
				{
					if ((attributes & MethodAttributes.Public) != MethodAttributes.Public)
					{
						return (attributes & MethodAttributes.Assembly) != MethodAttributes.Assembly;
					}
					return false;
				}
				return true;
			}
			if (info is FieldInfo)
			{
				FieldInfo fieldInfo = info as FieldInfo;
				if (!fieldInfo.IsPrivate)
				{
					return !fieldInfo.IsFamily;
				}
				return false;
			}
			throw new InvalidOperationException($"MemberInfo.CanSet is not possible for type={info.GetType()}");
		}

		public static Delegate CreateDelegate(this MethodInfo method, Type type, object target)
		{
			return Delegate.CreateDelegate(type, target, method);
		}

		public static Delegate CreateDelegate(this MethodInfo method, Type type)
		{
			return Delegate.CreateDelegate(type, method);
		}
	}
}
