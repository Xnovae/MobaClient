using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Polybrush
{
	/**
	 *	Static helper methods for working with reflection.  Mostly used for ProBuilder
	 *	compatibility.
	 */
	public static class z_ReflectionUtil
	{
		static EditorWindow _pbEditor = null;
		public static bool enableWarnings = true;

		const BindingFlags ALL_FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		// Assembly Qualified name for ProBuilder Editor window.
		public const string PB_EDITOR_TYPE 					= "ProBuilder2.EditorCommon.pb_Editor";
		public const string PB_EDITOR_MESH_UTILITY_TYPE_OLD = "ProBuilder2.EditorCommon.pb_Editor_Mesh_Utility";
		public const string PB_EDITOR_MESH_UTILITY_TYPE 	= "ProBuilder2.EditorCommon.pb_EditorMeshUtility";


		private static void Warning(string text)
		{
			if(enableWarnings)
				Debug.LogWarning(text);
		}

		/**
		 *	Reference to the ProBuilder Editor window if it is avaiable.
		 */
		public static EditorWindow pbEditor
		{
			get
			{
				if(_pbEditor == null)
				{
					EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
					_pbEditor = windows.FirstOrDefault(x => x.GetType().ToString().Contains("pb_Editor"));
				}
				return _pbEditor;
			}
		}

		/**
		 *	Tests if ProBuilder is available in the project.
		 */
		public static bool ProBuilderExists()
		{
			return GetType("pb_Object") != null;
		}

		/**
		 *	Tests if a GameObject is a ProBuilder mesh or not.
		 */
		public static bool IsProBuilderObject(GameObject gameObject)
		{
			return gameObject != null && gameObject.GetComponent("pb_Object") != null;
		}

		/**
		 *	Get a component with type name.
		 */
		public static object GetComponent(this GameObject gameObject, string componentTypeName)
		{
			return gameObject.GetComponent(componentTypeName);
		}

		/**
		 *	Fetch a type with name and optional assembly name.  `type` should include namespace.
		 */
		public static Type GetType(string type, string assembly = null)
		{
			Type t = Type.GetType(type);

			if(t == null)
			{
				IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();

				if(assembly != null)
					assemblies = assemblies.Where(x => x.FullName.Contains(assembly));

				foreach(Assembly ass in assemblies)
				{
					t = ass.GetType(type);

					if(t != null)
						return t;
				}
			}

			return t;
		}

		private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();

		/**
		 *	Same as GetType except this function caches the result for quick lookups.
		 */
		public static Type GetTypeCached(string type, string assembly = null)
		{
			Type res = null;

			if( _cachedTypes.TryGetValue(type, out res) )
				return res;

			res = GetType(type);
			_cachedTypes.Add(type, res);

			return res;
		}

		/**
		 *	Call a method with args.
		 */
		public static object Invoke(object target,
									string method,
									BindingFlags flags = ALL_FLAGS,
									params object[] args)
		{
			if(target == null)
			{
				Warning("Invoke failed, target is null and no type was provided.");
				return null;
			}

			return Invoke(target, target.GetType(), method, null, flags, args);
		}

		public static object Invoke(object target,
									string type,
									string method,
									BindingFlags flags = ALL_FLAGS,
									string assembly = null,
									params object[] args)
		{
			Type t = GetType(type, assembly);

			if(t == null && target != null)
				t = target.GetType();

			if(t != null)
				return Invoke(target, t, method, null, flags, args);
			else
				Warning("Invoke failed, type is null: " + type);

			return null;
		}

		public static object Invoke(object target,
									Type type,
									string method,
									Type[] methodParams = null,
									BindingFlags flags = ALL_FLAGS,
									params object[] args)
		{
			MethodInfo mi = null;

			if(methodParams == null)
				mi = type.GetMethod(method, flags);
			else
				mi = type.GetMethod(method, flags, null, methodParams, null);

			if(mi == null)
			{
				Warning("Failed to find method " + method + " in type " + type);
				return null;
			}

			return mi.Invoke(target, args);
		}

		/**
		 *	Fetch a value using GetProperty or GetField.
		 */
		public static object GetValue(object target, string type, string member, BindingFlags flags = ALL_FLAGS)
		{
			Type t = GetType(type);

			if(t == null)
			{
				Warning(string.Format("Could not find type \"{0}\"!", type));
				return null;
			}
			else
				return GetValue(target, t, member, flags);
		}

		public static object GetValue(object target, Type type, string member, BindingFlags flags = ALL_FLAGS)
		{
			PropertyInfo pi = type.GetProperty(member, flags);

			if(pi != null)
				return pi.GetValue(target, null);

			FieldInfo fi = type.GetField(member, flags);

			if(fi != null)
				return fi.GetValue(target);

			Warning(string.Format("Could not find member \"{0}\" matching type {1}!", member, type));

			return null;
		}

		public static bool SetValue(object target, string member, object value, BindingFlags flags = ALL_FLAGS)
		{
			if(target == null)
				return false;

			PropertyInfo pi = target.GetType().GetProperty(member, flags);

			if(pi != null)
				pi.SetValue(target, value, flags, null, null, null);

			FieldInfo fi = target.GetType().GetField(member, flags);

			if(fi != null)
				fi.SetValue(target, value);

			return pi != null || fi != null;
		}

		private static MethodInfo _pbOptimizeMethod 		= null;
		private static MethodInfo _pbRefreshMethod 			= null;
		private static MethodInfo _pbRefreshWithMaskMethod 	= null;

		public static MethodInfo ProBuilder_OptimizeMethodInfo()
		{
			if(_pbOptimizeMethod == null)
			{
				Type editorMeshUtilityType = z_ReflectionUtil.GetType(PB_EDITOR_MESH_UTILITY_TYPE);

				if(editorMeshUtilityType == null)
					editorMeshUtilityType = z_ReflectionUtil.GetType(PB_EDITOR_MESH_UTILITY_TYPE_OLD);

				if(editorMeshUtilityType != null)
				{
					// 2.5.1
					_pbOptimizeMethod = editorMeshUtilityType.GetMethod("Optimize",
						BindingFlags.Public | BindingFlags.Static,
						null,
						new Type[] { GetTypeCached("pb_Object"), typeof(bool) },
						null );

					if(_pbOptimizeMethod == null)
					{
						_pbOptimizeMethod = editorMeshUtilityType.GetMethod("Optimize",
							BindingFlags.Public | BindingFlags.Static,
							null,
							new Type[] { GetTypeCached("pb_Object") },
							null );
					}
				}
			}

			return _pbOptimizeMethod;
		}

		/**
		 *	Fallback for ProBuilder 2.6.1 and lower (Refresh() with no params).
		 */
		public static MethodInfo ProBuilder_RefreshMethodInfo()
		{
			if(_pbRefreshMethod == null)
			{
				_pbRefreshMethod = GetTypeCached("pb_Object").GetMethod(
					"Refresh",
					BindingFlags.Public | BindingFlags.Instance);
			}

			return _pbRefreshMethod;
		}

		public static MethodInfo ProBuilder_RefreshWithMaskMethodInfo()
		{
			if(_pbRefreshWithMaskMethod == null)
			{
				Type refreshMaskType = GetTypeCached("ProBuilder2.Common.RefreshMask");

				if(refreshMaskType == null)
					return null;

				_pbRefreshWithMaskMethod = GetTypeCached("pb_Object").GetMethod(
					"Refresh",
					BindingFlags.Public | BindingFlags.Instance,
					null,
					new Type[] { refreshMaskType },
					null);
			}

			return _pbRefreshWithMaskMethod;
		}

		/**
		 *	Calls pb_EditorUtility.Optimize
		 */
		public static void ProBuilder_Optimize(object pb)
		{
			MethodInfo mi = ProBuilder_OptimizeMethodInfo();

			if(mi == null)
				return;

			ParameterInfo[] pi = mi.GetParameters();

			if(pi == null)
				return;

			object[] args = new object[pi.Length];

			args[0] = pb;

			// HasDefaultValue not available until .NET 4.5
			for(int i = 1; i < pi.Length; i++)
				args[i] = pi[i].DefaultValue;

			mi.Invoke(null, args);
		}

		private static object[] _refreshArgs = new object[1];

		public static void ProBuilder_Refresh(object pb, ushort mask)
		{
			MethodInfo refresh = ProBuilder_RefreshWithMaskMethodInfo();

			if(refresh != null)
			{
				_refreshArgs[0] = mask;
				refresh.Invoke(pb, _refreshArgs);
			}
			else
			{
				refresh = ProBuilder_RefreshMethodInfo();

				if(refresh != null)
					refresh.Invoke(pb, null);
				else
					Debug.LogWarning("ProBuilder_Refresh failed to find an appropriate `Refresh` method on `pb_Object` type");
			}
		}
	}
}
