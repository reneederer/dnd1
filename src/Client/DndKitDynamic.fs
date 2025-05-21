module DndKitDynamic

open Fable.Core
open Fable.Core.JsInterop
open Fable.React

// Import DndContext as a component
let DndContext: obj = importMember "@dnd-kit/core"

// Hooks (useDraggable, useDroppable)
let useDraggable: obj = importMember "@dnd-kit/core"
let useDroppable: obj = importMember "@dnd-kit/core"
