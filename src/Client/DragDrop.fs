module DragDrop

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Fable.React
open Browser.Types
open Browser

// DnD Kit Imports
[<Import("useDraggable", from="@dnd-kit/core")>]
let private useDraggable: obj -> obj = jsNative

[<Import("useDroppable", from="@dnd-kit/core")>]
let private useDroppable: obj -> obj = jsNative

[<Import("DndContext", from="@dnd-kit/core")>]
let private dndContext: obj = jsNative

[<Import("PointerSensor", from="@dnd-kit/core")>]
let private pointerSensor: obj = jsNative

[<Import("useSensor", from="@dnd-kit/core")>]
let private useSensor: obj -> obj = jsNative

[<Import("useSensors", from="@dnd-kit/core")>]
let private useSensors: obj -> obj = jsNative

// Types
type DraggableProps = { 
    id: string 
    content: ReactElement 
}

type DroppableProps = { 
    id: string 
    //onDrop: Element option -> unit // Accepts both React and native elements
    children: bool -> ReactElement 
}

// React Draggable Component
[<ReactComponent>]
let Draggable (props: DraggableProps) =
    let dnd = useDraggable {| id = props.id |}
    
    let transformStyle = 
        if not (isNull (box dnd?transform)) then
            let x = unbox<float> dnd?transform?x
            let y = unbox<float> dnd?transform?y
            [ style.transform.translate(int x, int y) ]
        else
            []

    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.style ([
            style.cursor.grab
            style.border(2, borderStyle.solid, color.red)
            style.padding 10
            style.marginBottom 8
            style.backgroundColor "#fee2e2"
            style.position.relative
            style.zIndex (if unbox<bool> dnd?isDragging then 1 else 0)
        ] @ transformStyle)
        prop.onPointerDown (unbox dnd?listeners?onPointerDown)
        prop.onKeyDown (unbox dnd?listeners?onKeyDown)
        prop.children [ props.content ]
    ]

// Droppable Component (handles both React and native drops)
[<ReactComponent>]
let Droppable (props: DroppableProps) =
    let dnd = useDroppable {| 
        id = props.id
        //onDrop = fun ev -> 
        //    let element = 
        //        if not (isNull ev?active?node?current) then 
        //            Some (unbox ev?active?node?current)
        //        else None
        //    props.onDrop element
    |}
    
    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.children [ props.children (unbox<bool> dnd?isOver) ]
        prop.style [
            style.border(2, borderStyle.solid, color.blue)
            style.padding 10
            style.minHeight 60
            style.marginTop 10
            style.backgroundColor (if unbox<bool> dnd?isOver then "#dbeafe" else "#f9fafb")
        ]
    ]

// DnD Context
[<ReactComponent>]
let DndContext (onDragEnd: (obj -> unit) option) (children: ReactElement list) =
    let sensors = useSensors(useSensor(pointerSensor))
    
    let contextProps = 
        match onDragEnd with
        | Some handler -> [| "onDragEnd" ==> handler |]
        | None -> [||]
    
    ReactBindings.React.createElement(
        dndContext,
        createObj (Array.append contextProps [|
            "sensors" ==> sensors
        |]),
        List.toArray children
    )

// Makes native HTML elements draggable
[<ReactComponent>]
let NativeDragManager (elementIds: string list) =
    React.useEffectOnce(fun () ->
        let setupElement (id: string) =
            let el = document.getElementById(id)
            if not (isNull el) then
                el.setAttribute("draggable", "true")
                el.addEventListener("dragstart", fun e ->
                    let dragEvent = e :?> DragEvent
                    dragEvent.dataTransfer.setData("text/plain", id)
                    // Required for Firefox
                    dragEvent.dataTransfer.effectAllowed <- "move"
                )
        
        // Setup all specified elements
        elementIds |> List.iter setupElement
        
        React.createDisposable(fun () -> 
            elementIds |> List.iter (fun id ->
                let el = document.getElementById(id)
                if not (isNull el) then
                    el.removeAttribute("draggable")
            )
        )
    )
    Html.none