module EasyFQ

open System
open EasyNetQ
open FSharp.Control

let inline SeqSubscribe<'T> (bus : IBus) subscriptionId =
    let bq = new BlockingQueueAgent<'T>(1)

    let rec messageSeq () = seq {
        yield bq.Get()
        yield! messageSeq()
    }

    bus.Subscribe(subscriptionId, fun message -> bq.Add message)
    messageSeq ()