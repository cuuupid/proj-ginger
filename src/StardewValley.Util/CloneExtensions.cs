using System;
using System.Collections.Generic;
using System.Reflection;

namespace StardewValley.Util
{
	public static class CloneExtensions
	{
		private class ReferenceComparer : EqualityComparer<object>
		{
			public override bool Equals(object x, object y)
			{
				return x == y;
			}

			public override int GetHashCode(object obj)
			{
				return obj?.GetHashCode() ?? 0;
			}
		}

		private class ArrayTraverse
		{
			public int[] Position;

			private int[] maxLengths;

			public ArrayTraverse(Array array)
			{
				maxLengths = new int[array.Rank];
				for (int i = 0; i < array.Rank; i++)
				{
					maxLengths[i] = array.GetLength(i) - 1;
				}
				Position = new int[array.Rank];
			}

			public bool Step()
			{
				for (int i = 0; i < Position.Length; i++)
				{
					if (Position[i] < maxLengths[i])
					{
						Position[i]++;
						for (int j = 0; j < i; j++)
						{
							Position[j] = 0;
						}
						return true;
					}
				}
				return false;
			}
		}

		private static readonly MethodInfo CloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

		private static bool IsPrimitive(this Type type)
		{
			if (type == typeof(string))
			{
				return true;
			}
			return type.IsValueType & type.IsPrimitive;
		}

		public static T DeepClone<T>(this T obj)
		{
			return (T)DeepCloneObject(obj);
		}

		private static object DeepCloneObject(object originalObject)
		{
			return InternalCopy(originalObject, new Dictionary<object, object>(new ReferenceComparer()));
		}

		private static object InternalCopy(object originalObject, Dictionary<object, object> visited)
		{
			if (originalObject == null)
			{
				return null;
			}
			Type type = originalObject.GetType();
			if (type.IsPrimitive())
			{
				return originalObject;
			}
			if (visited.ContainsKey(originalObject))
			{
				return visited[originalObject];
			}
			if (typeof(Delegate).IsAssignableFrom(type))
			{
				return null;
			}
			object obj = CloneMethod.Invoke(originalObject, null);
			if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				if (!elementType.IsPrimitive())
				{
					Array clonedArray = (Array)obj;
					ForEach(clonedArray, delegate(Array array, int[] indices)
					{
						array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices);
					});
				}
			}
			visited.Add(originalObject, obj);
			CopyFields(originalObject, visited, obj, type);
			RecursiveCopyBaseTypePrivateFields(originalObject, visited, obj, type);
			return obj;
		}

		private static void RecursiveCopyBaseTypePrivateFields(object originalObject, Dictionary<object, object> visited, object cloneObject, Type typeToReflect)
		{
			if (typeToReflect.BaseType != null)
			{
				RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
				CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, (FieldInfo info) => info.IsPrivate);
			}
		}

		private static void CopyFields(object originalObject, Dictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
		{
			FieldInfo[] fields = typeToReflect.GetFields(bindingFlags);
			foreach (FieldInfo fieldInfo in fields)
			{
				if ((filter == null || filter(fieldInfo)) && !fieldInfo.FieldType.IsPrimitive())
				{
					object value = fieldInfo.GetValue(originalObject);
					object value2 = InternalCopy(value, visited);
					fieldInfo.SetValue(cloneObject, value2);
				}
			}
		}

		public static T Copy<T>(this T original)
		{
			return (T)((object)original).Copy();
		}

		private static void ForEach(Array array, Action<Array, int[]> action)
		{
			if (array.LongLength != 0L)
			{
				ArrayTraverse arrayTraverse = new ArrayTraverse(array);
				do
				{
					action(array, arrayTraverse.Position);
				}
				while (arrayTraverse.Step());
			}
		}
	}
}
