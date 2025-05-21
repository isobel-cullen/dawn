namespace Microsoft.Xna.Framework

open System
open System.Diagnostics
open System.IO
open System.Reflection
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Xml

// adapted from https://github.com/FNA-XNA/FNA/blob/master/src/Utilities/FNADllMap.cs
// not 100% faithful adaption so lookout if you need the 'invert' functionality

[<AutoOpen>]
module FNADllMap =
    let dict = System.Collections.Generic.Dictionary<string,string> ()

    let getPlaformName () =
        if OperatingSystem.IsWindows () then "windows"
        else if OperatingSystem.IsMacOS () then "osx"
        else if OperatingSystem.IsLinux () then "linux"
        else if OperatingSystem.IsFreeBSD () then "freebsd"
        else "unknown"

    let mapAndLoad libraryName assembly searchPath =
        let mappedName =
            match dict.TryGetValue libraryName with
            | true,mapped -> mapped
            | _ -> libraryName

        NativeLibrary.Load (mappedName, assembly, searchPath)

    let assembly = Assembly.GetExecutingAssembly ()

    (* NativeAOT platforms don't perform dynamic loading,
     * so setting a DllImportResolver is unnecessary.
     *
     * However, iOS and tvOS with Mono AOT statically link
     * their dependencies, so we need special handling for them.
     *)
    if not RuntimeFeature.IsDynamicCodeCompiled then
        let loadStaticLibrary library assembly searchPath =
            NativeLibrary.GetMainProgramHandle ()

        if OperatingSystem.IsIOS () || OperatingSystem.IsTvOS () then 
                NativeLibrary.SetDllImportResolver (assembly, loadStaticLibrary)

    let os = getPlaformName ()
    let cpu = RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
    let wordsize = IntPtr.Size * 8 |> string
    
    let xmlPath = Path.Combine (
        AppContext.BaseDirectory,
        assembly.GetName().Name + ".dll.config")
    
    let forThisOs (node: XmlNode) =
        match isNull node with
        | false -> 
            let osAttr = node.Attributes.["os"]
            osAttr <> null && osAttr.Value.Contains os
        | _ -> false

    let hasDllTarget (node: XmlNode) =
        match node <> null with
        | true ->
            match node.Attributes.["dll"], node.Attributes.["target"] with
            | null,_ 
            | _, null -> false
            | dll,target -> 
                not (String.IsNullOrWhiteSpace dll.Value || String.IsNullOrWhiteSpace target.Value)
        | _ -> false

    match File.Exists xmlPath with
    | true ->
        let xmlDoc = XmlDocument ()
        xmlDoc.Load xmlPath

        if (xmlDoc.GetElementsByTagName "dllentry").Count > 0 then
            let msg = "Function remapping is not supported by .NET Core. Ignoring dllentry elements..."
            Console.WriteLine msg
            if Debugger.IsAttached then Debug.WriteLine msg

        for node in xmlDoc.GetElementsByTagName "dllmap" do
            if node |> forThisOs && node |> hasDllTarget then
                let oldLib = node.Attributes.["dll"].Value
                let newLib = node.Attributes.["target"].Value

                dict.TryAdd (oldLib, newLib) |> ignore
    | _ -> 
        let msg = "Unable to find " + xmlPath
        Console.WriteLine msg
        if Debugger.IsAttached then Debug.WriteLine msg 

    NativeLibrary.SetDllImportResolver (assembly, mapAndLoad)
