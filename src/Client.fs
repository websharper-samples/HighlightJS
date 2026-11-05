namespace Samples

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Templating
open WebSharper.UI.Client
open WebSharper.HighlightJS

/// A small playground: edit a snippet, pick a language, and see highlight.js
/// colorize it as you type.
[<JavaScript>]
module Client =

    type Snippet =
        {
            Id: string
            Name: string
            Language: WebSharper.HighlightJS.Language
            Code: string
        }

    let private snippets =
        [|
            {
                Id = "fsharp"
                Name = "F#"
                Language = Language.Fsharp
                Code =
                    """let rec factorial n =
    if n <= 1 then 1
    else n * factorial (n - 1)

[<EntryPoint>]
let main _ =
    printfn "5! = %d" (factorial 5)
    0"""
            }
            {
                Id = "javascript"
                Name = "JavaScript"
                Language = Language.Javascript
                Code =
                    """const range = (n) => [...Array(n).keys()];

const squares = range(10)
  .map((x) => x * x)
  .filter((x) => x % 2 === 0);

console.log(squares);"""
            }
            {
                Id = "csharp"
                Name = "C#"
                Language = Language.Csharp
                Code =
                    """using System;
using System.Linq;

class Program
{
    static void Main()
    {
        var evens = Enumerable.Range(1, 10).Where(x => x % 2 == 0);
        Console.WriteLine(string.Join(", ", evens));
    }
}"""
            }
            {
                Id = "python"
                Name = "Python"
                Language = Language.Python
                Code =
                    """def fib(n):
    a, b = 0, 1
    for _ in range(n):
        a, b = b, a + b
    return a

print([fib(i) for i in range(10)])"""
            }
            {
                Id = "typescript"
                Name = "TypeScript"
                Language = Language.Typescript
                Code =
                    """interface Point {
    x: number;
    y: number;
}

const distance = (a: Point, b: Point): number =>
    Math.hypot(a.x - b.x, a.y - b.y);

console.log(distance({ x: 0, y: 0 }, { x: 3, y: 4 }));"""
            }
            {
                Id = "xml"
                Name = "HTML"
                Language = Language.Xml
                Code =
                    """<!DOCTYPE html>
<html lang="en">
  <head>
    <meta charset="utf-8" />
    <title>Hello</title>
  </head>
  <body>
    <h1 class="title">Hello, world!</h1>
  </body>
</html>"""
            }
            {
                Id = "json"
                Name = "JSON"
                Language = Language.Json
                Code =
                    """{
  "name": "websharper",
  "version": "10.1.9",
  "keywords": ["fsharp", "web", "spa"],
  "stable": true
}"""
            }
            {
                Id = "css"
                Name = "CSS"
                Language = Language.Css
                Code =
                    """.card {
  display: flex;
  padding: 1rem 1.25rem;
  border-radius: 16px;
  box-shadow: 0 10px 30px rgba(15, 23, 42, 0.07);
}"""
            }
            {
                Id = "sql"
                Name = "SQL"
                Language = Language.Sql
                Code =
                    """SELECT c.name, COUNT(o.id) AS orders
FROM customers AS c
JOIN orders AS o ON o.customer_id = c.id
WHERE o.placed_at >= '2026-01-01'
GROUP BY c.name
ORDER BY orders DESC;"""
            }
            {
                Id = "shell"
                Name = "Shell"
                Language = Language.Bash
                Code =
                    """#!/usr/bin/env bash
set -euo pipefail

for file in src/*.fsproj; do
  echo "building ${file}"
  dotnet build "${file}" -c Release
done"""
            }
        |]

    let private sampleOf id =
        snippets
        |> Array.tryFind (fun s -> s.Id = id)
        |> Option.map (fun s -> s.Code)
        |> Option.defaultValue ""

    type Template = Templating.Template<"wwwroot/index.html", ClientLoad.FromDocument>

    [<SPAEntryPoint>]
    let Main () =
        // The core build has no languages, so register the ones we use.
        for s in snippets do
            Hljs.RegisterLanguage(s.Id, s.Language)

        let language = Var.Create "fsharp"
        let source = Var.Create (sampleOf "fsharp")

        let select lang =
            language.Value <- lang
            source.Value <- sampleOf lang

        let isActive (id: string) : View<bool> =
            language.View |> View.Map (fun l -> l = id)

        let languages =
            Doc.Concat [
                for s in snippets ->
                    Doc.Button s.Name [
                        attr.``class`` "lang-btn"
                        Attr.DynamicClassPred "is-active" (isActive s.Id)
                    ] (fun () -> select s.Id)
            ]

        let highlighted =
            code [
                on.viewUpdate
                    (View.Map2 (fun l c -> l, c) language.View source.View)
                    (fun el (lang, code) ->
                        // highlight.js skips elements it has already processed.
                        el.RemoveAttribute "data-highlighted"
                        el.ClassName <- "language-" + lang
                        el.TextContent <- code
                        Hljs.HighlightElement(el))
            ] []

        Template()
            .Languages(languages)
            .Source(source)
            .Highlighted(highlighted)
            .Bind()
