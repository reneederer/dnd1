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
        // Your fancytree component (this would be from your UI framework)
        
        // Enable dragging for fancytree nodes
        
        // Regular DnD context
        DndContext
            (fun ev ->
                console.log ev
                if not <| isNullOrUndefined ev?over then
                    printfn $"Drag from: {ev?active?id} to {ev?over?id}"
            )
            [ DndDiv
                { id = "draggable1"
                  children = (fun isDragging isOver ->
                      Html.div
                        [
                          prop.style [ style.backgroundColor.red; style.display.block; style.minWidth 500]
                          prop.children
                            [ Html.br []
                              Html.text "abc"
                              Html.text $"draggable1 {isDragging} {isOver}"
                              Html.br []
                            ]
                        ]
                  )
                }
              Draggable
                { id = "draggable2"
                  content =
                    Html.div
                      [ //prop.style [ style.backgroundColor.brown; style.maxWidth 70 ]
                        prop.text "draggable2"
                      ]
                }
              Droppable
                { id = "dropzone1"
                  children = fun isOver ->
                      Html.div
                        [ prop. style [ style.minHeight 200; style.backgroundColor "red" ]
                          prop.children
                            [ Html.text (if isOver then "Drop node here!" else "Drop Zone")
                              Droppable
                                { 
                                  id = "dropzone2"
                                  children = fun isOver ->
                                      Html.div [
                                          prop.style
                                            [ style.backgroundColor "lightgreen"
                                              style.minHeight 60
                                            ]
                                          prop.text (if isOver then "Drop node here2!" else "Drop Zone2")
                                      ]
                              }
                            ]
                      ]
                  }
              Draggable
                { id = "draggable3"
                  content = Html.text "draggable3"
                }
      ]
    ]