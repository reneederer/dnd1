module DragDrop

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Fable.React
open Browser.Types
open Browser

// DnD Kit Imports (keeping your existing imports)
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
    children: bool -> ReactElement 
}

[<ReactComponent>]
let Draggable (props: DraggableProps) =
    let dnd = useDraggable {| id = props.id |}
    
    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.style [
            if not (isNull (box dnd?transform)) then
                let x = unbox<float> dnd?transform?x
                let y = unbox<float> dnd?transform?y
                style.transform.translate(int x, int y)
            
            style.cursor.grab
            style.position.relative
            style.zIndex (if unbox<bool> dnd?isDragging then 1 else 0)
            style.display.tableCell
        ]
        prop.onPointerDown (unbox dnd?listeners?onPointerDown)
        prop.onKeyDown (unbox dnd?listeners?onKeyDown)
        prop.children [ props.content ]
    ]

// Simplified Droppable Component (removed predefined styles)
[<ReactComponent>]
let Droppable (props: DroppableProps) =
    let dnd = useDroppable {| id = props.id |}
    
    Html.div [
        prop.ref (unbox dnd?setNodeRef)
        prop.children [ props.children (unbox<bool> dnd?isOver) ]
    ]

type DragAndDropProps = {
    id: string
    //onDrop: string -> unit // id of dropped item
    children: bool -> bool -> ReactElement // (isDragging, isOver) -> ReactElement
}

[<ReactComponent>]
let DndDiv (props: DragAndDropProps) =
    let draggable = useDraggable {| id = props.id |}
    let droppable = useDroppable {| id = props.id |}
    
    // Handle drop events
    React.useEffect(fun () ->
        if unbox<bool> droppable?isOver && not (unbox<bool> draggable?isDragging) then
            //props.onDrop(props.id)
            ()
    , [| box droppable?isOver; box draggable?isDragging |])
    
    Html.div [
        prop.ref (fun node ->
            unbox <| draggable?setNodeRef(node)
            unbox <| droppable?setNodeRef(node)
        )
        prop.style [ style.backgroundColor.black ]
        
        // Draggable props
        prop.onPointerDown (unbox draggable?listeners?onPointerDown)
        prop.onKeyDown (unbox draggable?listeners?onKeyDown)
        
        // Style combining both states
        prop.style [
            if not (isNull (box draggable?transform)) then
                let x = unbox<float> draggable?transform?x
                let y = unbox<float> draggable?transform?y
                style.transform.translate(int x, int y)
            
            style.cursor.grab
            style.position.relative
            style.zIndex (if unbox<bool> draggable?isDragging then 1 else 0)
            style.display.tableCell
            // Add your own styles here
        ]
        
        // Render children with both states
        prop.children [ 
            props.children 
                (unbox<bool> draggable?isDragging) 
                (unbox<bool> droppable?isOver) 
        ]
    ]

// DnD Context (unchanged from your original)
[<ReactComponent>]
let DndContext (onDragEnd: (obj -> unit)) (children: ReactElement list) =
    let sensors = useSensors(useSensor(pointerSensor))
    
    let contextProps = 
        [| "onDragEnd" ==> onDragEnd |]
    
    ReactBindings.React.createElement(
        dndContext,
        createObj (Array.append contextProps [|
            "sensors" ==> sensors
        |]),
        List.toArray children
    )