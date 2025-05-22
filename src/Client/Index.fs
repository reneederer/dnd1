module Index

open Feliz
open Elmish
open DragDrop
open Browser.Dom
open Fable.Core
open JsInterop

type Model = {
    Dragged: string option
    Slots: string list
}

type Msg =
    | DragEnded of string * string option

let init () = 
    { Dragged = None; Slots = [ "slot-1"; "slot-2" ] }, Cmd.none

let update msg model =
    match msg with
    | DragEnded (source, target) ->
        console.log($"""Dropped {source} into {Option.defaultValue "nothing" target}""")
        { model with Dragged = None }, Cmd.none

let view model dispatch =
    let handleDragEnd (ev: obj) =
        let activeId = ev?active?id
        let overId = 
            let over = ev?over
            if isNull over then None
            else Some (over?id |> string)
        
        dispatch (DragEnded(activeId, overId))

    Html.div [
        prop.style [ style.padding 20 ]
        prop.children [
            DndContext handleDragEnd [
                Draggable { id = "event-1"; content = Html.text "🟦 Event 1 (drag me)" }
                Droppable { id = "slot-1"; render = fun _ -> Html.text "Drop here" }
                Droppable { id = "slot-2"; render = fun _ -> Html.text "Another drop zone" }
                Draggable { id = "event-2"; content = Html.text "🟦 Event 2 (drag me)" }
                Droppable { id = "slot-3"; render = fun _ -> Html.text "Another drop zone" }
                Droppable { id = "slot-4"; render = fun _ -> Html.text "Another drop zone" }
                Droppable { id = "slot-5"; render = fun _ -> Html.text "Another drop zone" }
            ]
        ]
    ]