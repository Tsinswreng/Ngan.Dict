// See https://aka.ms/new-console-template for more information

using Ngan.Dict.Server.Http.Test;

Console.WriteLine("Hello, World!");

await MkDbSchema.InitDb([@"E:\_code\CsNgan.Dict\Ngan.Dict.Server\ExternalRsrc\Ngan.Dict.Server.dev.jsonc"]);
