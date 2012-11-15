// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.

#I "bin/release"
#I "bin/debug"
#r "EasyNetQ"
#r "FSharpx.Core"
#load "Core.fs"
open EasyNetQ
open EasyFQ

type MyMessage = { message : string }

let bus = RabbitHutch.CreateBus "host=localhost"

let messages = SeqSubscribe<MyMessage> bus "hello_world"

let tokenSource = new System.Threading.CancellationTokenSource()

let agent = MailboxProcessor<unit>.Start(fun inbox -> async {
        let rec inner () =
            let message = Seq.head messages
            printfn "%s" message.message
            inner ()
        inner()
    }, tokenSource.Token)

let publishMessage message =
    use pub = bus.OpenPublishChannel ()
    pub.Publish ({message = message })

let startTime = System.DateTime.Now

for i in [1..100000] do
    publishMessage (i.ToString())

tokenSource.Cancel ()

let endTime = System.DateTime.Now

printfn "Total time: %A" (endTime - startTime)
((endTime.Ticks - startTime.Ticks) / int64 100000)
|> System.TimeSpan.FromTicks
|> printfn "Time per message: %A" 