namespace WebSharper.Highlightjs.Sample

open WebSharper
open WebSharper.JavaScript
open WebSharper.HighlightJS

[<JavaScript>]
module Client =

    [<SPAEntryPoint>]
    let Main () =
        let hljsConfig = WebSharper.HighlightJS.Options(TabReplace = "    ", UseBR = false)
        Hljs.Configure(hljsConfig)
        Hljs.InitHighlightingOnLoad()
