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
    let droppable = useDroppable {| 
        id = props.id 
        data = createObj [ "acceptsExternal" ==> true ]
    |}
    
    let dropRef = React.useRef<Browser.Types.Element option>(None)
    
    // Enhanced drag event handling with Event and e? syntax
    React.useEffect(fun () ->
        let handleDragEnter (e: Event) =
            e?preventDefault()
            console.log("dragEnter", props.id)
            droppable?isOver <- true
        
        let handleDragOver (e: Event) =
            e?preventDefault()
            e?dataTransfer?dropEffect <- "move"
            console.log("dragOver", props.id)  // Log dragOver events
            droppable?isOver <- true
        
        let handleDragLeave (e: Event) =
            console.log("dragLeave", props.id)
            droppable?isOver <- false
        
        let handleDrop (e: Event) =
            e?preventDefault()
            console.log("drop", props.id)
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
        )
        prop.style [
            style.border(2, borderStyle.dashed, "gray")
            style.padding 20
            style.minHeight 200
            style.minWidth 300
            style.display.flex
            style.alignItems.center
            style.justifyContent.center
            if unbox<bool> droppable?isOver then
                style.backgroundColor "lightblue"
        ]
        prop.children [ props.children ]
    ]

[<ReactComponent>]
let ExternalDraggableElement() =
    React.useEffectOnce(fun () ->
        let element = document.getElementById("externalEl")
        element?setAttribute("draggable", "true")
        
        let handleDragStart (e: Event) =
            console.log("External drag started")
            e?dataTransfer?setData("text/plain", "externalEl")
            e?dataTransfer?effectAllowed <- "move"
            
        element?addEventListener("dragstart", handleDragStart)
        
        { new System.IDisposable with
            member _.Dispose() =
                element?removeEventListener("dragstart", handleDragStart)
        }
    )
    Html.none






[<ReactComponent>]
let DndContext (onDragEnd: obj -> unit) (children: ReactElement list) =
    let sensors = useSensors(useSensor(pointerSensor))
    let activeId, setActiveId = React.useState<string option>(None)
    
    let contextProps = 
        [| 
            "onDragStart" ==> (fun e -> setActiveId(e?active?id))
            "onDragEnd" ==> (fun e ->
                setActiveId(None)
                onDragEnd e)
            "collisionDetection" ==> closestCenter
        |]
    
    let overlayContent =
        match activeId with
        | Some id -> 
            Html.div [
                prop.style [
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
        ReactBindings.React.createElement(
            dndContext,
            createObj (Array.append contextProps [|
                "sensors" ==> sensors
            |]),
            List.toArray children
        )
        
        ReactBindings.React.createElement(
            dragOverlay,
            createObj [||],
            [ overlayContent ]
        )
    ]