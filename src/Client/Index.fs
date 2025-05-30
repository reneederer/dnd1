module Index

open Feliz
open Fable.Core.JsInterop
open Browser.Dom
open Browser.Types
open Elmish
open DragDrop
open Fable.Core

[<ReactComponent>]
let ExternalDraggableElement() =
    React.useEffectOnce(fun () ->
        let element = document.getElementById("externalEl")
        element.setAttribute("draggable", "true")
        
        let handleDragStart (e: Browser.Types.Event) =
            e?dataTransfer?setData("text/plain", "externalEl")
            e?dataTransfer?effectAllowed <- "move"
            
        let handleDragEnd (e: Browser.Types.Event) =
            // Clear any drag state
            ()
            
        element.addEventListener("dragstart", handleDragStart)
        element.addEventListener("dragend", handleDragEnd)
        
        { new System.IDisposable with
            member _.Dispose() =
                element.removeEventListener("dragstart", handleDragStart)
                element.removeEventListener("dragend", handleDragEnd)
        }
    )
    Html.none
type Model = {
    DroppedItem: string option
    DraggedItem: string option
    Slots: string list
}

type Msg =
    | ExternalDropped of string
    | DndKitDragStarted of string
    | DndKitDragEnded of obj

let init () = 
    { DroppedItem = None; DraggedItem = None; Slots = [ "slot-1"; "slot-2" ] }, Cmd.none

let update msg model =
    match msg with
    | ExternalDropped id ->
        console.log($"External item dropped: {id}")
        { model with DroppedItem = Some id }, Cmd.none
    | DndKitDragStarted id ->
        { model with DraggedItem = Some id }, Cmd.none
    | DndKitDragEnded ev ->
        console.log("Internal DnD event", ev)
        { model with DraggedItem = None }, Cmd.none

[<ReactComponent>]
let App model dispatch =
    Html.div [
        ExternalDraggableElement()
        
        Html.h1
          [ prop.draggable true
            prop.text "Drag and Drop Example" ]
        
        Html.div [
            prop.style [ 
                style.display.flex
                style.flexDirection.row
                style.gap 20
                style.padding 20
            ]
            prop.children [
                DndContext
                    (fun e -> 
                        if not (isNullOrUndefined e?over) then
                            printfn $"Drag from: {e?active?id} to {e?over?id}"
                        dispatch (DndKitDragEnded e)
                    )
                    [ 
                        DndDiv {
                            id = "dropZone1"
                            onDrop = (fun _ -> dispatch (ExternalDropped "externalEl"))
                            children = 
                                Html.div [
                                    prop.text (
                                        match model.DroppedItem with
                                        | Some item -> $"Dropped: {item}"
                                        | None -> "Drop Zone 1"
                                    )
                                ]
                        }
                    ]
            ]
        ]
    ]
let view model dispatch =
    App model dispatch