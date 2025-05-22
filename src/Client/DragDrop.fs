module DragDrop

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Fable.React

[<Import("useDraggable", from="@dnd-kit/core")>]
let private useDraggableHook: obj -> obj = jsNative

[<Import("useDroppable", from="@dnd-kit/core")>]
let private useDroppableHook: obj -> obj = jsNative

[<Import("DndContext", from="@dnd-kit/core")>]
let private dndContextComponent: obj = jsNative

[<Import("PointerSensor", from="@dnd-kit/core")>]
let private pointerSensor: obj = jsNative

[<Import("useSensor", from="@dnd-kit/core")>]
let private useSensorHook: obj -> obj = jsNative

[<Import("useSensors", from="@dnd-kit/core")>]
let private useSensorsHook: obj -> obj = jsNative

type DraggableProps = { 
    id: string 
    content: ReactElement 
}

type DroppableProps = { 
    id: string 
    render: bool -> ReactElement 
}

[<Emit("Object.keys($0)")>]
let private objectKeys (x: obj) : string[] = jsNative

[<ReactComponent>]
let Draggable (props: DraggableProps) =
    let dnd = useDraggableHook {| id = props.id |}
    
    // Create base props
    let baseProps = [
        prop.ref (unbox dnd?setNodeRef)
        prop.style [
            style.cursor.grab
            style.border(2, borderStyle.solid, color.red)
            style.padding 10
            style.marginBottom 8
            style.backgroundColor "#fee2e2"
        ]
        prop.children [ props.content ]
    ]

    // Helper function to safely convert JS props
    let convertJsProps (jsProps: obj) =
        if isNullOrUndefined jsProps then []
        else
            objectKeys(jsProps)
            |> Array.map (fun key -> 
                prop.custom(key, jsProps?(key)))
            |> Array.toList

    // Merge all props together
    let mergedProps = 
        baseProps
        @ (convertJsProps (dnd?attributes))
        @ (convertJsProps (dnd?listeners))

    Html.div mergedProps

[<ReactComponent>]
let Droppable (props: DroppableProps) =
    let dnd = useDroppableHook {| id = props.id |}
    
    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.children [ props.render (unbox<bool> dnd?isOver) ]
        prop.style [
            style.border(2, borderStyle.solid, color.blue)
            style.padding 10
            style.minHeight 60
            style.marginTop 10
            style.display.flex
            style.alignItems.center
            style.justifyContent.center
            style.backgroundColor (if unbox<bool> dnd?isOver then "#dbeafe" else "#f9fafb")
        ]
    ]

[<ReactComponent>]
let DndContext (onDragEnd: obj -> unit) (children: ReactElement list) =
    let sensors = 
        useSensorsHook(
            useSensorHook(pointerSensor)
        )
    
    ReactBindings.React.createElement(
        dndContextComponent,
        createObj [
            "onDragEnd" ==> onDragEnd
            "sensors" ==> sensors
        ],
        children
    )