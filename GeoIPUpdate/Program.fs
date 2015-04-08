// http://geolite.maxmind.com/download/geoip/database/GeoLiteCity_CSV/GeoLiteCity-latest.zip

open System
open System.IO
open System.IO.Packaging
open System.Net
open System.IO.Compression

[<EntryPoint>]
let main argv = 

    printfn "Geo IP update tool. Generates the update SQL from the CSV stored in the cloud."

    let url = if (argv.Length < 1 || argv.[0] = null) then "http://geolite.maxmind.com/download/geoip/database/GeoIPCountryCSV.zip" else argv.[0]
    let tempFile = sprintf "GeoIP_%s.zip" (Guid.NewGuid().ToString("N"))

    printf "Downloading the file from %s and saving to %s... " url tempFile

    (new WebClient()).DownloadFile(url, tempFile)

    printfn "done."

    use pkg = ZipFile.OpenRead(tempFile)
    use csv = pkg.Entries |> Seq.head |> fun p -> p.Open()
    use text = new StreamReader(csv)

    let lines = seq {
        while not text.EndOfStream do
            yield text.ReadLine().Split(',') |> Array.map(fun s -> s.Trim('"') :> Object)} |>
                Seq.map(fun row -> String.Format("INSERT [dbo].[GeoIPCountryWhoIs] values ('{0}', '{1}', {2}, {3}, '{4}')", row))

    let outFile = tempFile + ".sql"

    printf "Writing the SQL to %s... " outFile

    use w = new StreamWriter(File.OpenWrite(outFile))

    w.WriteLine("-- USE eCom_Commmon")
    w.WriteLine("-- GO")
    w.WriteLine()
    w.WriteLine("TRUNCATE TABLE [dbo].[GeoIPCountryWhoIs]")
    w.WriteLine()

    for line in lines do 
        w.WriteLine(line)

    printfn "done."

    printfn "The program completed successfully. Press any key to exit..."

    Console.ReadKey() |> ignore
    0
