module Index

open Feliz
open Elmish
open DragDrop
open Browser.Dom
open Fable.Core
open JsInterop
open Fable.SimpleJson

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
    Html.div [
        // Native HTML draggable elements
        Html.div [
            prop.id "native-item-1"
            prop.draggable true
            prop.style [
                style.padding 10
                style.margin 5
                style.backgroundColor "#e2e2e2"
                style.cursor.move
            ]
        ]
        Html.div [
            prop.id "native-item-2" 
            prop.text "Native Item 2"
            prop.style [
                style.padding 10
                style.margin 5
                style.backgroundColor "#e2e2e2"
                style.cursor.move
            ]
        ]
        
        NativeDragManager ["native-item-1"; "native-item-2"]
        
        // React DnD Area
        DndContext (Some (fun ev -> 
            printfn "Drag ended: %A" ev
            console.log ev
        )) [
            // React draggable
            Draggable {
                id = "react-item-1"
                content = Html.text "React Draggable"
            }
            
            Droppable {
                id = "mixed-dropzone"
                //onDrop = fun element ->
                //    match element with
                //    | Some el -> 
                //        printfn "Dropped native element: %s" el.id
                //        // You can access all DOM element properties here
                //    | None -> 
                //        printfn "Dropped React element"
                children = fun isOver ->
                    Html.div [
                        prop.text (if isOver then "Release to drop!" else "Drop here (accepts both)")
                    ]
            }
        ]
    ]