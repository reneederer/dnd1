module DragDrop

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Fable.React

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

type DraggableProps = { 
    id: string 
    content: ReactElement 
}

type DroppableProps = { 
    id: string 
    render: bool -> ReactElement 
}

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

[<ReactComponent>]
let Droppable (props: DroppableProps) =
    let dnd = useDroppable {| id = props.id |}
    
    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.children [ props.render (unbox<bool> dnd?isOver) ]
        prop.style [
            style.border(2, borderStyle.solid, color.blue)
            style.padding 10
            style.minHeight 60
            style.marginTop 10
            style.backgroundColor (if unbox<bool> dnd?isOver then "#dbeafe" else "#f9fafb")
        ]
    ]

[<ReactComponent>]
let DndContext (onDragEnd: obj -> unit) (children: ReactElement list) =
    let sensors = useSensors(useSensor(pointerSensor))
    
    ReactBindings.React.createElement(
        dndContext,
        createObj [
            "onDragEnd" ==> onDragEnd
            "sensors" ==> sensors
        ],
        List.toArray children
    )