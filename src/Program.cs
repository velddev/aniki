using Miki.Anilist;
using Miki.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnilistCLI
{
    class Program
    {
        static AnilistClient client;

        static async Task Main(string[] args)
        {
            client = new AnilistClient();

            new LogBuilder()
                .AddLogEvent((msg, level) => Console.WriteLine(msg))
                .Apply();

            while (true)
            {
                Dictionary<string, string> @params = new Dictionary<string, string>();

                Console.Write(">");
                var query = Console.ReadLine().Split(' ').ToList();
                Console.WriteLine("");

                for(int i = 0; i < query.Count; i++)
                {
                    if(query[i].StartsWith('-'))
                    {
                        var newParam = query[i].Split(':');
                        if(newParam.Length == 1)
                        {
                            @params.Add(newParam[0], "true");
                        }
                        else if(newParam.Length > 1)
                        {
                            @params.Add(newParam[0], string.Join(':', newParam.Skip(1)));
                        }
                        query.RemoveAt(i);
                        i--;
                    }
                }

                if(query.Count == 0)
                {
                    continue;
                }

                switch(query[0].ToLower())
                {
                    case "help":
                    {

                    } break;

                    case "character":
                    {
                        await CharacterAction(query.Skip(1).Take(query.Count - 1), @params);
                    } break;

                    case "media":
                    {
                        await AnimeAction(query.Skip(1).Take(query.Count - 1), @params);
                    } break;
                }
            }
        }

        static async Task AnimeAction(IEnumerable<string> args, Dictionary<string, string> @params)
        {
            List<MediaFormat> formats = new List<MediaFormat>();

            foreach(var f in Enum.GetNames(typeof(MediaFormat)))
            {
                if(@params.ContainsKey($"--{f.ToLower().Replace('_', '-')}"))
                {
                    formats.Add(Enum.Parse<MediaFormat>(f));
                }
            }

            bool nsfw = @params.ContainsKey("-nsfw");
        
            if (args.Count() == 0)
            {
                return;
            }

            switch(args.ElementAtOrDefault(0) ?? "")
            {
                case "search":
                {
                    int page = 0;
                    @params.TryGetValue("-p", out string pageCount);
                    int.TryParse(pageCount, out page);

                    var searchResult = await client.SearchMediaAsync(string.Join(" ", args.Skip(1).Take(args.Count() - 1)), page, nsfw, formats.ToArray());

                    if (searchResult == null)
                    {
                        Console.WriteLine("Nothing was found here!");
                        return;
                    }

                    for (int i = 0; i < searchResult.Items.Count; i++)
                    {
                        Console.WriteLine($"{searchResult.Items[i].Id.ToString().PadLeft(6)}: {searchResult.Items[i].DefaultTitle}");
                    }
                    Console.WriteLine("----------");
                    Console.WriteLine($"Page {searchResult.PageInfo.CurrentPage} of {searchResult.PageInfo.TotalPages}");
                } break;

                default:
                {
                    var media = await client.GetMediaAsync(string.Join(" ", args), formats.ToArray());
                    if (media == null)
                    {
                        Console.WriteLine("response was empty, most likely this media does not exist!");
                        return;
                    }

                    Console.WriteLine($"{media.EnglishTitle} ({media.NativeTitle})");
                    Console.WriteLine($"Parts: {media.Episodes ?? media.Volumes ?? 0} | Status: {media.Status}");
                    Console.WriteLine($"Score: {media.Score} | {string.Join(", ", media.Genres)}");
                    Console.WriteLine($"{media.Description}");
                }
                break;
            }
        }

        static async Task CharacterAction(IEnumerable<string> args, Dictionary<string, string> @params)
        {
            if(args.Count() == 0)
            {
                return;
            }

            switch(args.ElementAtOrDefault(0) ?? "")
            {
                case "search":
                {
                    int page = 0;
                    @params.TryGetValue("-p", out string pageCount);
                    int.TryParse(pageCount, out page);

                    var searchResult = await client.SearchCharactersAsync(string.Join(" ", args.Skip(1).Take(args.Count() - 1)), page);
                    
                    if(searchResult == null)
                    {
                        Console.WriteLine("Nothing was found here!");
                        return;
                    }

                    for(int i = 0; i < searchResult.Items.Count; i++)
                    {
                        Console.WriteLine($"{searchResult.Items[i].FirstName} {searchResult.Items[i].LastName} ({searchResult.Items[i].Id})");
                    }
                    Console.WriteLine("----------");
                    Console.WriteLine($"Page {searchResult.PageInfo.CurrentPage} of {searchResult.PageInfo.TotalPages}");
                } break;

                default:
                {
                    var character = await client.GetCharacterAsync(string.Join(" ", args));
                    if(character == null)
                    {
                        Console.WriteLine("response was empty, most likely this character does not exist!");
                        return;
                    }

                    Console.WriteLine($"{character.FirstName} {character.LastName} ({character.NativeName})");
                    Console.WriteLine($"{character.Description}");
                } break;
            }
        }
    }
}
