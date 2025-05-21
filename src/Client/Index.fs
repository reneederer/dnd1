module Index

open Elmish
open SAFE
open Shared
open Fable
open Feliz
open DndKitDynamic
open Fable.React
open Fable.Core.JsInterop
open Fable.Core
open Fable.SimpleJson
open FSharpPlus


// JS interop for Object.keys
[<Emit("Object.keys($0)")>]
let objectKeys (o: obj) : string[] = jsNative

// Convert JS object properties to Feliz props
let propsFromObject (obj: obj) : IReactProperty list =
    if isNull obj then []
    else
        objectKeys obj
        |> Array.map (fun key -> prop.custom(key, obj?(key)))
        |> Array.toList


let DraggableComponent =
    React.functionComponent(fun (id: string) ->
        let draggable: obj = useDraggable $ createObj [ "id" ==> id ]
        let transform = draggable?transform

        let baseProps = [
            prop.ref draggable?setNodeRef
            prop.style [
                if transform <> null then
                    style.custom("transform", $"translate({transform?x}px, {transform?y}px)")
                else
                    style.custom("transform", "none")
                style.border(1, borderStyle.solid, color.black)
                style.padding 10
                style.margin 10
                style.cursor.grab
            ]
            prop.text $"Drag me: {id}"
        ]

        let listenersProps = propsFromObject draggable?listeners
        let attributesProps = propsFromObject draggable?attributes

        let allProps = baseProps @ listenersProps @ attributesProps

        Html.div allProps
    )

let DroppableComponent =
    React.functionComponent(fun (id: string) ->
        let droppable: obj = useDroppable $ createObj [ "id" ==> id ]

        Html.div
          [ prop.ref droppable?setNodeRef
            prop.style [
                style.height 100
                style.backgroundColor (
                    if droppable?isOver then "lightgreen" else "lightgray"
                )
                style.display.flex
                style.alignItems.center
                style.justifyContent.center
                style.margin 10
            ]
            prop.text $"Drop here: {id}"
        ]
            
    )


let DndContext: obj = import "DndContext" "@dnd-kit/core"

let DndWrapper =
    React.functionComponent(fun () ->
        let handleDragEnd (evt: Browser.Types.MouseEvent) =
            let activeId = evt?active?id
            let overId =
                if isNull evt?over then
                    None
                else
                    Some (evt?over?id)
            printfn $"{evt |> SimpleJson.stringify}"
            printfn $"{evt.clientY}, {evt.pageY}, {evt.offsetY}, {evt.screenY}, {evt.y}"
            match overId with
            | Some id -> printfn $"Dragged {activeId |> SimpleJson.stringify} over {id |> SimpleJson.stringify}"
            | None -> printfn $"Dragged {activeId |> SimpleJson.stringify} but not over any droppable"

        Interop.reactApi.createElement(
            DndContext,
            createObj [
                "onDragEnd" ==> handleDragEnd
            ],
            [|
                DraggableComponent "item-1"
                DroppableComponent "drop-zone1"
                DroppableComponent "drop-zone2"
            |]
        )
    )


// Elmish MVU code unchanged below

type Model = {
    Todos: RemoteData<Todo list>
    Input: string
}

type Msg =
    | SetInput of string
    | LoadTodos of ApiCall<unit, Todo list>
    | SaveTodo of ApiCall<string, Todo list>

let todosApi = Api.makeProxy<ITodosApi> ()

let init () =
    let initialModel = { Todos = NotStarted; Input = "" }
    let initialCmd = LoadTodos(Start()) |> Cmd.ofMsg

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetInput value -> { model with Input = value }, Cmd.none
    | LoadTodos msg ->
        match msg with
        | Start() ->
            let loadTodosCmd = Cmd.OfAsync.perform todosApi.getTodos () (Finished >> LoadTodos)

            { model with Todos = model.Todos.StartLoading() }, loadTodosCmd
        | Finished todos -> { model with Todos = Loaded todos }, Cmd.none
    | SaveTodo msg ->
        match msg with
        | Start todoText ->
            let saveTodoCmd =
                let todo = Todo.create todoText
                Cmd.OfAsync.perform todosApi.addTodo todo (Finished >> SaveTodo)

            { model with Input = "" }, saveTodoCmd
        | Finished todos ->
            {
                model with
                    Todos = RemoteData.Loaded todos
            },
            Cmd.none


module ViewComponents =
    let todoAction model dispatch =
        Html.div [
            prop.className "flex flex-col sm:flex-row mt-4 gap-4"
            prop.children [
                Html.input [
                    prop.className
                        "shadow appearance-none border rounded w-full py-2 px-3 outline-none focus:ring-2 ring-teal-300 text-grey-darker text-sm sm:text-base"
                    prop.value model.Input
                    prop.placeholder "What needs to be done?"
                    prop.autoFocus true
                    prop.onChange (SetInput >> dispatch)
                    prop.onKeyPress (fun ev ->
                        if ev.key = "Enter" then
                            dispatch (SaveTodo(Start model.Input)))
                ]
                Html.button [
                    prop.className
                        "flex-no-shrink p-2 px-12 rounded bg-teal-600 outline-none focus:ring-2 ring-teal-300 font-bold text-white hover:bg-teal disabled:opacity-30 disabled:cursor-not-allowed text-sm sm:text-base"
                    prop.disabled (Todo.isValid model.Input |> not)
                    prop.onClick (fun _ -> dispatch (SaveTodo(Start model.Input)))
                    prop.text "Add"
                ]
            ]
        ]

    let todoList model dispatch =
        Html.div [
            prop.className "rounded-md p-2 sm:p-4 w-full"
            prop.children [
                Html.ol [
                    prop.className "list-decimal ml-4 sm:ml-6"
                    prop.children [
                        match model.Todos with
                        | NotStarted -> Html.text "Not Started."
                        | Loading None -> Html.text "Loading..."
                        | Loading (Some todos)
                        | Loaded todos ->
                            for todo in todos do
                                Html.li [
                                    prop.className "my-1 text-black text-base sm:text-lg break-words"
                                    prop.text todo.Description
                                ]
                    ]
                ]

                todoAction model dispatch
            ]
        ]


let view model dispatch =
    Html.div [
        DndWrapper()
    ]
