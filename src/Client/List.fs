module  X

module List =

    let map (f : 'a -> 'b) : ('a list -> 'b list) =
        (fun xs ->
            [ for x in xs do f x ]
        )


    let rec bind (f : 'a -> 'b list) : ('a list -> 'b list) =
        fun xs ->
            [ for x in xs do
                yield! f x
            ]

module Option =

    let map (f : 'a -> 'b) : ('a option -> 'b option) =
        (fun opt ->
            match opt with
            | None -> None
            | Some value -> Some (f value)
        )

    let bind (f : 'a -> 'b option) : ('a option -> 'b option) =
        fun opt ->
            match opt with
            | None ->
                None

            | Some value ->
                f value




