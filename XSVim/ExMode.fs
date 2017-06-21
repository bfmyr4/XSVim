﻿namespace XSVim
open MonoDevelop.Ide.Editor
open MonoDevelop.Ide.Editor.Extension

module exMode =
    let getFirstCharAndRest (s:string) = s.[0], s.[1..]

    let processCommand command =
        let firstChar, rest = getFirstCharAndRest command
        match firstChar, rest with
        | '/', _ -> [ runOnce (IncrementalSearch rest) Nothing ]
        | '?', _ -> [ runOnce (IncrementalSearchBackwards rest) Nothing ]
        | _ -> wait

    let processKey (state:VimState) (key:KeyDescriptor) =
        let setMessage message = { state with statusMessage = message }
        let normalMode = { state with statusMessage = None; mode = NormalMode } 
        match key.SpecialKey with
        | SpecialKey.BackSpace ->
            let message =
                match state.statusMessage with
                | Some msg ->
                    let len = msg.Length
                    msg.[0..len-2]
                | None -> ""
            if message.Length > 0 then
                setMessage (Some message), processCommand message
            else
                normalMode, resetKeys
        | SpecialKey.Return ->
            match state.statusMessage with
            | Some message ->
                let firstChar, rest = getFirstCharAndRest message
                match firstChar, rest with
                | '/', _ ->
                    { state with statusMessage = None; mode = NormalMode; lastSearch = Some (ToSearch rest) }
                    , [ runOnce Move (ToSearch rest)]
                | '?', _ ->
                    { state with statusMessage = None; mode = NormalMode; lastSearch = Some (ToSearchBackwards rest) }
                    , [ runOnce Move (ToSearchBackwards rest)]
                | _ -> normalMode, resetKeys
            | _ -> normalMode, resetKeys
        | SpecialKey.Escape ->
            normalMode, resetKeys
        | _ ->
            let message =
                match state.statusMessage with
                | Some msg -> sprintf "%s%c" msg key.KeyChar
                | None -> string key.KeyChar

            setMessage (Some message), processCommand message
