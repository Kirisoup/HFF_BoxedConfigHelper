global using System;
global using SoupExt;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;

using cfile = BepInEx.Configuration.ConfigFile;
using cdefi = BepInEx.Configuration.ConfigDefinition;
using cdesc = BepInEx.Configuration.ConfigDescription;

namespace HFF_BoxedConfigHelper;

public static class Binder {
    private static readonly Lazy<MethodInfo> _binder = new(() =>
        typeof(cfile)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .ToList()
            .Where(m => m.Name == "Bind")
        .FirstOrDefault(m =>
                m.GetParameters()
                    .Select(p => p.ParameterType)
                    .ToArray()
                    .IsWhen(a => a.Length == 3, out Type[] a) &&
                    a[0] == typeof(cdefi) &&
                    a[2] == typeof(cdesc)
            )
    );

    public static ConfigEntryBase TBind(
        this cfile file,
        cdefi def,
        object val,
        cdesc desc = null
    ) => _binder.Value?
            .MakeGenericMethod(val.GetType())
            .Invoke(file, new[] { def, val, desc }) as ConfigEntryBase;

    public static ConfigEntryBase TBind(
        this cfile file,
        string sec,
        string key,
        object val,
        cdesc desc = null
    ) => file.TBind(new(sec, key), val, desc);

    public static ConfigEntryBase TBind(
        this cfile file,
        string sec,
        string key,
        object val,
        string desc
    ) => file.TBind(new(sec, key), val, new(desc, null));
}
