module DragDrop

open Fable.Core
open Fable.Core.JsInterop
open Feliz
open Fable.React
open Browser.Types
open Browser

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

[<Import("closestCenter", from="@dnd-kit/core")>]
let private closestCenter: obj = jsNative

[<Import("DragOverlay", from="@dnd-kit/core")>]
let private dragOverlay: obj = jsNative

type DragAndDropProps = {
    id: string
    onDrop: string -> unit
    children: ReactElement
}

[<ReactComponent>]
let DndDiv (props: DragAndDropProps) =
    let draggable = useDraggable {| id = props.id |}
    let droppable = useDroppable {| id = props.id |}
    
    let dropRef = React.useRef<Browser.Types.Element option>(None)
    
    React.useEffect(fun () ->
        let handleDragEnter (e: Event) =
            e?preventDefault()
            droppable?isOver <- true
        
        let handleDragOver (e: Event) =
            e?preventDefault()
            e?dataTransfer?dropEffect <- "move"
            droppable?isOver <- true
            console.log ("over", e)
        
        let handleDragLeave (e: Event) =
            droppable?isOver <- false
        
        let handleDrop (e: Event) =
            e?preventDefault()
            let data = e?dataTransfer?getData("text/plain")
            if not (System.String.IsNullOrEmpty(data)) then
                props.onDrop(data)
            droppable?isOver <- false
        
        match dropRef.current with
        | Some node ->
            node?addEventListener("dragenter", handleDragEnter)
            node?addEventListener("dragover", handleDragOver)
            node?addEventListener("dragleave", handleDragLeave)
            node?addEventListener("drop", handleDrop)
            
            { new System.IDisposable with
                member _.Dispose() =
                    node?removeEventListener("dragenter", handleDragEnter)
                    node?removeEventListener("dragover", handleDragOver)
                    node?removeEventListener("dragleave", handleDragLeave)
                    node?removeEventListener("drop", handleDrop)
            }
        | None -> { new System.IDisposable with member _.Dispose() = () }
    , [| box dropRef.current |])
    
    Html.div [
        prop.ref (fun node ->
            if not (isNull node) then
                dropRef.current <- Some node
                droppable?setNodeRef(node)
                draggable?setNodeRef(node)
        )
        prop.style [
            style.border(2, borderStyle.dashed, "gray")
            style.padding 20
            style.minHeight 200
            style.minWidth 300
            style.display.flex
            style.alignItems.center
            style.justifyContent.center
            style.cursor.grab
            if droppable?isOver then
                style.backgroundColor "lightblue"
            if draggable?isDragging then
                style.opacity 0.5
        ]
        prop.onPointerDown draggable?listeners?onPointerDown
        prop.onKeyDown draggable?listeners?onKeyDown
        prop.children [ props.children ]
    ]

[<ReactComponent>]
let DndContext (onDragEnd: obj -> unit) (children: ReactElement list) =
    let sensors = useSensors(useSensor(pointerSensor))
    let activeId, setActiveId = React.useState<string option>(None)
    
    let handleDragStart (e: obj) =
        let activeId = string e?active?id
        console.log("Drag started", activeId)
        setActiveId(Some activeId)
    
    let handleDragEnd (e: obj) =
        let activeId = e?active?id
        console.log("Drag ended", activeId)
        setActiveId(None)
        onDragEnd e
    
    let contextProps = 
        createObj [|
            "onDragStart" ==> handleDragStart
            "onDragEnd" ==> handleDragEnd
            "collisionDetection" ==> closestCenter
            "sensors" ==> sensors
        |]
    
    let overlayContent =
        match activeId with
        | Some id -> 
            Html.div [
                prop.style [
                    style.position.absolute
                    style.pointerEvents.none
                    style.zIndex 9999
                    style.backgroundColor "#4CAF50"
                    style.color.white
                    style.padding 10
                    style.borderRadius 4
                    style.boxShadow(0, 2, 10, "rgba(0,0,0,0.3)")
                ]
                prop.text "Custom Ghost"
            ]
        | None -> Html.none
    
    Html.div [
        ReactBindings.React.createElement(dndContext, contextProps, List.toArray children)
        ReactBindings.React.createElement(dragOverlay, createObj [||], [ overlayContent ])
    ]